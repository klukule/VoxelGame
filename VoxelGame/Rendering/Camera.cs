using OpenTK;
using VoxelGame.Rendering.Buffers;

namespace VoxelGame.Rendering
{
    /// <summary>
    /// Camera type
    /// </summary>
    public enum CameraProjectionType
    {
        Perspective,
        Orthographic
    }

    /// <summary>
    /// Camera
    /// </summary>
    public class Camera
    {
        private CameraUniformBuffer _ubo = new CameraUniformBuffer();

        /// <summary>
        /// Camera position
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Camera rotation - Pitch, Yaw, Roll
        /// </summary>
        public Vector3 Rotation { get; set; }

        /// <summary>
        /// Camera projection
        /// </summary>
        public CameraProjectionType ProjectionType { get; set; } = CameraProjectionType.Perspective;

        /// <summary>
        /// Camera view matrix
        /// </summary>
        public Matrix4 ViewMatrix { get; private set; }

        /// <summary>
        /// Camera projection matrix
        /// </summary>
        public Matrix4 ProjectionMatrix { get; private set; }

        /// <summary>
        /// This is only used when the camera is Orthographic
        /// </summary>
        public Vector2 CameraSize { get; set; } = Vector2.One;

        /// <summary>
        /// Camera near plane
        /// </summary>
        public float NearPlane { get; } = 0.1f;

        /// <summary>
        /// Camera far plane
        /// </summary>
        public float FarPlane { get; } = 10000f;

        /// <summary>
        /// Camera frustum
        /// </summary>
        public Frustum Frustum { get; } = new Frustum(Matrix4.Identity);

        /// <summary>
        /// Forward vector the camera is facing
        /// </summary>
        public Vector3 ForwardVector => Mathf.GetForwardFromRotation(Rotation);

        /// <summary>
        /// Right vector from camera's point of view
        /// </summary>
        public Vector3 RightVector => Mathf.GetRightFromRotation(Rotation);

        /// <summary>
        /// Up vector from camera's point of view
        /// </summary>
        public Vector3 UpVector => Mathf.GetUpFromRotation(Rotation);

        /// <summary>
        /// Create new camera
        /// </summary>
        public Camera()
        {
            UpdateProjectionMatrix();
            Program.Window.Resize += (sender, args) => UpdateProjectionMatrix();
        }

        /// <summary>
        /// Update camera
        /// </summary>
        public void Update()
        {
            // TODO: calculate matrix only when dirty
            ViewMatrix = Matrix4.LookAt(Position, Position + ForwardVector, UpVector);

            // Update frustum
            Frustum.UpdateMatrix(ViewMatrix * ProjectionMatrix);

            // Update UBO data
            _ubo.ProjectionMat = ProjectionMatrix;
            _ubo.ViewMat = ViewMatrix;
            _ubo.Position = new Vector4(Position, 1);

            // Update UBO
            UniformBuffers.WorldCameraBuffer.Update(_ubo);
        }

        /// <summary>
        /// Calculate projection matrix
        /// </summary>
        public void UpdateProjectionMatrix()
        {
            switch (ProjectionType)
            {
                case CameraProjectionType.Perspective:
                    ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(Program.Settings.FieldOfView), Window.WindowWidth / (float)Window.WindowHeight, NearPlane, FarPlane);
                    break;
                case CameraProjectionType.Orthographic:
                    ProjectionMatrix = Matrix4.CreateOrthographic(CameraSize.X * 2f, CameraSize.Y * 2f, NearPlane, FarPlane);
                    break;
            }
        }
    }
}
