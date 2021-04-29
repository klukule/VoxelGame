using OpenTK;
using VoxelGame.Assets;
using VoxelGame.Worlds;

namespace VoxelGame.Rendering
{
    /// <summary>
    /// Simple skybox
    /// </summary>
    public class Skybox
    {
        private readonly Mesh _mesh;         // Sky sphere
        private readonly Material _material; // Sky material
        private Camera _camera = World.Instance.WorldCamera;

        /// <summary>
        /// Creates new skybox
        /// </summary>
        /// <param name="mat">Skybox material</param>
        public Skybox(Material mat)
        {
            _mesh = AssetDatabase.GetAsset<Mesh>("Models/SkySphere.obj");
            _material = mat;
        }

        /// <summary>
        /// Renders the skybox
        /// </summary>
        public void Render()
        {
            // Get camera if not null
            if (_camera == null) _camera = World.Instance.WorldCamera;

            // Draw skybox at camera position with scale reaching half-way to the far plane to avoid any potential clipping issues (still should be more than enough not to obscure the chunks)
            Renderer.DrawRequest(_mesh, _material, Matrix4.CreateScale(_camera.FarPlane / 2f) * Matrix4.CreateTranslation(_camera.Position));
        }

        /// <summary>
        /// Disposes of the data
        /// </summary>
        public void Dispose() => _mesh.Dispose();
    }
}
