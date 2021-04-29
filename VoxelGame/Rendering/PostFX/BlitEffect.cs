using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using VoxelGame.Rendering.Vertex;

namespace VoxelGame.Rendering.PostFX
{
    /// <summary>
    /// Base for any PostFX
    /// </summary>
    public class BlitEffect : IDisposable
    {
        // Shared fullscreen quad mesh
        protected static Mesh BlitMesh = new Mesh(
            new VertexContainer(
                new[] { new Vector3(-1, -1, 0), new Vector3(-1, 1, 0), new Vector3(1, -1, 0), new Vector3(1, 1, 0) },   // Positions in NDC
                new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 0), new Vector2(1, 1) }                    // UVs
            ),
            new uint[] { 1, 2, 0, 3, 2, 1 }                                                                             // Indices
        );

        /// <summary>
        /// Material used during the blit operation
        /// </summary>
        public virtual Material BlitMaterial { get; set; }

        /// <summary>
        /// Whether this effect is the last in the stack or not (if true, draws to screen)
        /// </summary>
        protected bool IsLastEffectInStack;

        /// <summary>
        /// Output frame buffer - most of the time used as a source for next effect in the stack
        /// </summary>
        public FBO SourceFbo { get; private set; } = new FBO(Program.Window.Width, Program.Window.Height, FBO.DepthBufferType.RenderBuffer);

        public BlitEffect()
        {
            Program.Window.Resize += delegate (object sender, EventArgs args)
            {
                SourceFbo = new FBO(Program.Window.Width, Program.Window.Height, FBO.DepthBufferType.RenderBuffer);
            };
        }

        /// <summary>
        /// Apply the effect
        /// </summary>
        /// <param name="src">Source framebuffer</param>
        public virtual void Render(FBO src)
        {
            BlitMaterial.SetScreenSourceTexture("u_Src", src.ColorHandle);
            Renderer.DrawNow(BlitMesh, BlitMaterial);
        }

        /// <summary>
        /// Blit from source to destionation fbo
        /// </summary>
        /// <param name="source">Source framebuffer</param>
        /// <param name="destination">Destination framebuffer</param>
        /// <param name="material">Used namterial</param>
        protected void Blit(FBO source, FBO destination, Material material)
        {
            // Blit from source to destination using material
            destination.Bind();
            material.SetScreenSourceTexture("u_Src", source.ColorHandle);
            Renderer.DrawNow(BlitMesh, material);
            destination.Unbind();
        }

        /// <summary>
        /// Setup for rendering
        /// </summary>
        /// <param name="isLast">Is last in the stack</param>
        public virtual void PreRender(bool isLast)
        {
            IsLastEffectInStack = isLast;
            GL.Disable(EnableCap.DepthTest);

            if (!isLast)
                SourceFbo.Bind();
            else
                SourceFbo.Unbind();
        }

        /// <summary>
        /// Cleanup after rendering
        /// </summary>
        /// <param name="isLast">Is last in the stack</param>
        public virtual void PostRender(bool isLast)
        {
            IsLastEffectInStack = isLast;
            GL.Enable(EnableCap.DepthTest);

            SourceFbo.Unbind();
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            BlitMaterial?.Dispose();
            if (BlitMesh != null)
            {
                BlitMesh.Dispose();
                BlitMesh = null;
            }
        }
    }
}
