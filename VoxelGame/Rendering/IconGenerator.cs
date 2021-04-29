using OpenTK;
using OpenTK.Graphics.OpenGL4;
using VoxelGame.Assets;
using VoxelGame.Worlds;
using VoxelGame.Items;
using VoxelGame.Rendering.Buffers;

namespace VoxelGame.Rendering
{
    /// <summary>
    /// Generates 2D icon previews
    /// </summary>
    public static class IconGenerator
    {
        /// <summary>
        /// Generates icons for all block items
        /// </summary>
        public static void GenerateBlockItemIcons()
        {
            const int ICON_SIZE = 64;

            FBO iconFbo = null;

            // Set clear color to transparent
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);

            // Predefined lighting
            LightingUniformBuffer lighting = new LightingUniformBuffer
            {
                AmbientColor = new Vector4(0.5f, 0.5f, 0.5f, 1),
                SunColor = new Vector4(1, 1, 1, 1),
                SunStrength = 1,
                SunDirection = new Vector4(0.5f, 0.5f, 0.5f, 1)
            };

            // Temporary chunk
            Chunk chunk = new Chunk(Vector2.Zero)
            {
                Blocks = new Chunk.BlockState[Chunk.WIDTH, Chunk.HEIGHT, Chunk.WIDTH]
            };

            // Orthographic camera
            Camera cam = new Camera
            {
                ProjectionType = CameraProjectionType.Orthographic,
                CameraSize = new Vector2(0.8f),
                Position = new Vector3(0, 0, 15f)
            };
            cam.UpdateProjectionMatrix();

            UniformBuffers.DirectionalLightBuffer.Update(lighting); // Update lighting UBO
            cam.Update();                                           // Update camera view, frustum etc..

            var items = ItemDatabase.GetItems();                    // Get all items

            for (int i = 0; i < items.Count; i++)
            {
                // Foreach block item
                if (items[i] is BlockItem item)
                {
                    iconFbo = new FBO(ICON_SIZE, ICON_SIZE, FBO.DepthBufferType.None);
                    iconFbo.Bind();
                    GL.Clear(ClearBufferMask.ColorBufferBit);

                    // Add the block to location 0,0,0
                    chunk.Blocks[0, 0, 0] = new Chunk.BlockState(0, 0, 0, chunk) { id = (short)item.Block.ID };

                    chunk.GenerateMesh();   // Generate mesh
                    chunk.RenderForIcon();  // Reder the chunk with transform for icon

                    Texture icon = new Texture(iconFbo, true); // Create new texture from FBO (takes over the ColorHandle)
                    AssetDatabase.RegisterAsset(icon, "GeneratedIcons/" + item.Key); // Register in VFS

                    // Store icon location
                    item.IconLocation = "GeneratedIcons/" + item.Key;

                    // Generate preview mesh
                    item.GenerateGraphics();
                }
            }

            // Unbind and dispose (iconFbo should be disposed by now from Texture constructor)
            iconFbo?.Unbind();
            iconFbo?.DisposeWithoutColorHandle();
            chunk.Dispose();

            // Reset the clear color
            GL.ClearColor(Window.CLEAR_COLOR.X, Window.CLEAR_COLOR.Y, Window.CLEAR_COLOR.Z, Window.CLEAR_COLOR.W);
        }
    }
}
