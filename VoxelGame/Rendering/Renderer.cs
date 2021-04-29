using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using VoxelGame.Assets;
using VoxelGame.Rendering.Buffers;
using VoxelGame.Rendering.Vertex;
using VoxelGame.Worlds;

namespace VoxelGame.Rendering
{
    /// <summary>
    /// Main render system
    /// </summary>
    public static class Renderer
    {
        /// <summary>
        /// Drawcall batch
        /// </summary>
        private struct DrawBatch
        {
            /// <summary>
            /// List off drawcalls
            /// </summary>
            public List<DrawCall> DrawCalls;

            /// <summary>
            /// Material draw with
            /// </summary>
            public Material Material;
        }

        /// <summary>
        /// Single batched draw call
        /// </summary>
        private struct DrawCall
        {
            /// <summary>
            /// The mesh to draw
            /// </summary>
            public Mesh Mesh;

            /// <summary>
            /// Transform
            /// </summary>
            public Matrix4 Transform;

            /// <summary>
            /// Contains incremental changes in uniforms between draw calls
            /// </summary>
            public Dictionary<string, object> UniformDelta { get; set; }

            /// <summary>
            /// Contains incremental changes in bound textures between draw calls
            /// </summary>
            public Dictionary<int, Texture> TextureDelta { get; set; }

            /// <summary>
            /// Contains bound uniform state
            /// </summary>
            public Dictionary<string, object> Uniforms { get; set; }

            /// <summary>
            /// Contains bound texture state
            /// </summary>
            public Dictionary<int, Texture> Textures { get; set; }
        }

        // Draw queue
        private static OrderedDictionary _drawQueue = new OrderedDictionary();

        // Temporary FBO to allow access to screen data in temporary passes
        private static FBO _temporaryFBO = new FBO(Program.Window.Width, Program.Window.Height, FBO.DepthBufferType.Texture);

        // Material for texture blit
        private static readonly Material _blitMaterial = AssetDatabase.GetAsset<Material>("Materials/FBO.mat");

        // Fullscreen quad mesh for blit
        private static readonly Mesh _blitMesh = new Mesh(
            new VertexContainer(
                new[] { new Vector3(-1, -1, 0), new Vector3(-1, 1, 0), new Vector3(1, -1, 0), new Vector3(1, 1, 0) },   // Positions in NDC
                new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) }                    // UVs
            ),
            new uint[] { 1, 2, 0, 3, 2, 1 }                                                                             // Indices
        );

        /// <summary>
        /// The amount of draw calls this frame
        /// </summary>
        public static int DrawCalls { get; set; }

        /// <summary>
        /// The amount of meshes culled
        /// </summary>
        public static int CulledCount { get; set; }

        /// <summary>
        /// Main rendering framebuffer
        /// </summary>
        public static FBO FrameBuffer { get; private set; } = new FBO(Program.Window.Width, Program.Window.Height, FBO.DepthBufferType.Texture);

        static Renderer()
        {
            // Resize framebuffers with resolution change
            Program.Window.Resize += (sender, args) =>
            {
                FrameBuffer = new FBO(Program.Window.Width, Program.Window.Height, FBO.DepthBufferType.Texture);
                _temporaryFBO = new FBO(Program.Window.Width, Program.Window.Height, FBO.DepthBufferType.Texture);
            };
        }


        public static void DrawRequest(Mesh mesh, Material material, Matrix4 transform = default)
        {
            if (mesh.IBO.Length == 0) return;                           // Skip empty meshes
            if (transform == default) transform = Matrix4.Identity;     // Assign identity when no transform is specified

            // Frustum culling
            if (World.Instance != null && !World.Instance.WorldCamera.Frustum.Intersects(mesh.Bounds.Transform(transform.ExtractTranslation(), Vector3.One)))
            {
                CulledCount++;
                return;
            }

            // Load fallback material if none specified
            if (material == null) material = AssetDatabase.GetAsset<Material>("Materials/Fallback.mat");

            // TODO: Instead of storing per-drawcall material, store only uniform and texture delta and bind only those

            // If draw queue contains batch for this material
            if (_drawQueue.Contains(material.Name))
            {
                // Add to the batch
                var batch = (DrawBatch)_drawQueue[material.Name];
                var last = batch.DrawCalls.Last(); // There is always at least one preceeding this

                // TODO: Optimize linq out

                // Calculate delta in uniforms
                var uniforms = material.Shader.Uniforms.ToDictionary(u => u.Key, u => u.Value.Value);
                var uniformDelta = new Dictionary<string, object>();
                foreach (var uniform in uniforms)
                    if (uniform.Value != last.Uniforms[uniform.Key])
                        uniformDelta.Add(uniform.Key, uniform.Value);

                // Calcualte delta for textures
                var textures = material.Textures.Select((t, i) => new { Texture = t, Index = i }).ToDictionary(o => o.Index, o => o.Texture);
                var textureDelta = new Dictionary<int, Texture>();
                foreach (var texture in textures)
                    if (!last.Textures.ContainsKey(texture.Key) || last.Textures.ContainsKey(texture.Key) && last.Textures[texture.Key] != texture.Value)
                        textureDelta.Add(texture.Key, texture.Value);

                batch.DrawCalls.Add(new DrawCall() { Mesh = mesh, Transform = transform, Uniforms = uniforms, UniformDelta = uniformDelta, Textures = textures, TextureDelta = textureDelta });
            }
            else if (material.Shader.IsTransparent)
            {
                // Add transparent materials to the end of the queue

                var call = new DrawCall()
                {
                    Mesh = mesh,
                    Transform = transform,
                    Uniforms = material.Shader.Uniforms.ToDictionary(u => u.Key, u => u.Value.Value),
                    Textures = material.Textures.Select((t, i) => new { Texture = t, Index = i }).ToDictionary(o => o.Index, o => o.Texture)
                };

                call.UniformDelta = call.Uniforms; // Delta for new materials is complete shapshot
                call.TextureDelta = call.Textures;

                _drawQueue.Add(material.Name, new DrawBatch()
                {
                    DrawCalls = new List<DrawCall>() { call },
                    Material = material,
                });
            }
            else
            {
                var call = new DrawCall()
                {
                    Mesh = mesh,
                    Transform = transform,
                    Uniforms = material.Shader.Uniforms.ToDictionary(u => u.Key, u => u.Value.Value),
                    Textures = material.Textures.Select((t, i) => new { Texture = t, Index = i }).ToDictionary(o => o.Index, o => o.Texture)
                };

                call.UniformDelta = call.Uniforms; // Delta for new materials is complete shapshot
                call.TextureDelta = call.Textures;

                // Insert opaque materials to the beginning
                _drawQueue.Insert(0, material.Name, new DrawBatch()
                {
                    DrawCalls = new List<DrawCall>() { call },
                    Material = material,
                });
            }

            // Update draw calls
            // TODO: Optimize or remove
            DrawCalls = 0;
            foreach (var batch in _drawQueue.Values)
                DrawCalls += ((DrawBatch)batch).DrawCalls.Count;
        }

        /// <summary>
        /// Immediate draw without queueing (used for things other than the world itself)
        /// </summary>
        /// <param name="mesh">The mesh to draw</param>
        /// <param name="material">The material to draw with</param>
        /// <param name="transform">The transform matrix</param>
        public static void DrawNow(Mesh mesh, Material material, Matrix4 transform = default)
        {
            // Setup material
            if (material == null) material = AssetDatabase.GetAsset<Material>("Materials/Fallback.mat");
            if (transform == default) transform = Matrix4.Identity;
            if (transform != default) material.Shader.SetUniform("u_World", transform);

            // Bind everything
            UniformBuffers.BindAll(material.Shader.Handle);
            material.Bind();
            mesh.VAO.Bind();
            mesh.IBO.Bind();

            // Draw
            GL.DrawElements(PrimitiveType.Triangles, mesh.IBO.Length, DrawElementsType.UnsignedInt, 0);

            // Unbind
            mesh.VAO.Unbind();
            mesh.IBO.Unbind();
            material.Unbind();
            UniformBuffers.UnbindAll();
        }

        /// <summary>
        /// Draw queued requests
        /// </summary>
        public static void DrawQueue()
        {
            // Clean framebuffers
            FrameBuffer.Bind();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            FrameBuffer.Unbind();

            _temporaryFBO.Bind();
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            _temporaryFBO.Unbind();

            // Foreach batch
            for (int i = 0; i < _drawQueue.Count; i++)
            {
                bool isLast = i == _drawQueue.Count - 1;    // Is last batch
                bool copyToTemp = false;                    // Whether to draw the output to secondary FBO

                var batch = (DrawBatch)_drawQueue[i];

                if (!isLast) // If not last batch
                {
                    // Current call needs to be opaque and next call needs to by transparent
                    var nextBatch = (DrawBatch)_drawQueue[i + 1];
                    if (!batch.Material.Shader.IsTransparent && nextBatch.Material.Shader.IsTransparent)
                        copyToTemp = true;
                }
                else // Last one is always drawn to temp
                    copyToTemp = true;

                // Bind material and uniform buffers
                batch.Material.Bind();
                UniformBuffers.BindAll(batch.Material.Shader.Handle);

                // Foreach drawcall
                for (int j = 0; j < batch.DrawCalls.Count; j++)
                {
                    // Update uniform if any changed
                    foreach (var uniform in batch.DrawCalls[j].UniformDelta)
                    {
                        batch.Material.Shader.SetUniform(uniform.Key, uniform.Value);
                        batch.Material.Shader.BindUniform(uniform.Key);
                    }

                    // Bind texture if changed
                    foreach (var texture in batch.DrawCalls[j].TextureDelta)
                        texture.Value.Bind(texture.Key);

                    // Bind non-default transform
                    if (batch.DrawCalls[j].Transform != default)
                    {
                        batch.Material.Shader.SetUniform("u_World", batch.DrawCalls[j].Transform);
                        batch.Material.Shader.BindUniform("u_World");
                    }

                    // Bind VAO and VBO
                    batch.DrawCalls[j].Mesh.VAO.Bind();
                    batch.DrawCalls[j].Mesh.IBO.Bind();

                    // Bind temporary fbo textures for transparent materials
                    if (batch.Material.Shader.IsTransparent)
                    {
                        if (batch.Material.Shader.ContainsUniform("u_Src"))
                        {
                            batch.Material.SetScreenSourceTexture("u_Src", _temporaryFBO.ColorHandle, 1);
                            batch.Material.Shader.BindUniform("u_Src");
                        }

                        if (batch.Material.Shader.ContainsUniform("u_Depth"))
                        {
                            batch.Material.SetScreenSourceTexture("u_Depth", _temporaryFBO.DepthHandle, 2);
                            batch.Material.Shader.BindUniform("u_Depth");
                        }
                    }

                    // Bind output framebuffer
                    FrameBuffer.Bind();

                    // Try rendering the mesh
                    try
                    {
                        var length = batch.DrawCalls[j].Mesh.IBO.Length;
                        GL.DrawElements(PrimitiveType.Triangles, length, DrawElementsType.UnsignedInt, 0);
                    }
                    catch
                    {
                        Debug.Log("Error when trying to render an object");
                    }

                    // Unbind
                    batch.DrawCalls[j].Mesh.VAO.Unbind();
                    batch.DrawCalls[j].Mesh.IBO.Unbind();
                    FrameBuffer.Unbind();
                }
                batch.DrawCalls.Clear(); // Clear the batch

                // Unbind all buffers if is last batch
                if (isLast)
                {
                    UniformBuffers.UnbindAll();
                    batch.Material.Unbind();
                }

                // If should copy to temp
                if (copyToTemp)
                {
                    if (!isLast) // Opaque -> Transparent
                    {
                        // Just blit one frambuffer to other one
                        GL.BlitNamedFramebuffer(FrameBuffer.Handle, _temporaryFBO.Handle, 0, 0, FrameBuffer.Width, FrameBuffer.Height, 0, 0, _temporaryFBO.Width, _temporaryFBO.Height,
                            ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit, BlitFramebufferFilter.Nearest);
                    }
                    else
                    {
                        GL.Disable(EnableCap.DepthTest);

                        _blitMaterial.SetScreenSourceTexture("u_Src", FrameBuffer.ColorHandle, 0);
                        DrawNow(_blitMesh, _blitMaterial);

                        GL.Enable(EnableCap.DepthTest);
                        _temporaryFBO.Unbind();
                    }
                }
            }

            _drawQueue.Clear();
        }

    }
}
