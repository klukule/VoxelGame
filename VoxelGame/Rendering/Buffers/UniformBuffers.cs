using OpenTK;

namespace VoxelGame.Rendering.Buffers
{
    /// <summary>
    /// Camera Uniform buffer data
    /// </summary>
    public struct CameraUniformBuffer
    {
        public Matrix4 ProjectionMat;
        public Matrix4 ViewMat;
        public Vector4 Position;
    }

    /// <summary>
    /// Time Uniform buffer data
    /// </summary>
    public struct TimeUniformBuffer
    {
        /// <summary>
        /// Game Time
        /// </summary>
        public float Time;

        /// <summary>
        /// Delta Time
        /// </summary>
        public float DeltaTime;
    }

    /// <summary>
    /// Dynamic lighting uniform buffer data
    /// </summary>
    public struct LightingUniformBuffer
    {
        /// <summary>
        /// Ambient color
        /// </summary>
        public Vector4 AmbientColor;

        /// <summary>
        /// Sun direction
        /// </summary>
        public Vector4 SunDirection;

        /// <summary>
        /// Sun color
        /// </summary>
        public Vector4 SunColor;

        /// <summary>
        /// Sun strength
        /// </summary>
        public float SunStrength;
    }

    /// <summary>
    /// Uniform buffer helper
    /// </summary>
    public static class UniformBuffers
    {
        /// <summary>
        /// Total UBOs
        /// </summary>
        public static int TotalUBOs { get; set; }

        /// <summary>
        /// Directional light buffer
        /// </summary>
        public static UBO<LightingUniformBuffer> DirectionalLightBuffer { get; }

        /// <summary>
        /// World camera buffer
        /// </summary>
        public static UBO<CameraUniformBuffer> WorldCameraBuffer { get; }

        /// <summary>
        /// Time buffer
        /// </summary>
        public static UBO<TimeUniformBuffer> TimeBuffer { get; }

        /// <summary>
        /// Initialize UBOs
        /// </summary>
        static UniformBuffers()
        {
            DirectionalLightBuffer = new UBO<LightingUniformBuffer>(default, "U_Lighting");
            WorldCameraBuffer = new UBO<CameraUniformBuffer>(default, "U_Camera");
            TimeBuffer = new UBO<TimeUniformBuffer>(default, "U_Time");
        }

        /// <summary>
        /// Dispose allocated resources
        /// </summary>
        public static void Dispose()
        {
            DirectionalLightBuffer.Dispose();
            WorldCameraBuffer.Dispose();
            TimeBuffer.Dispose();
        }

        /// <summary>
        /// Bind all UBOs to given shader
        /// </summary>
        public static void BindAll(int program)
        {
            DirectionalLightBuffer.Bind(program);
            WorldCameraBuffer.Bind(program);
            TimeBuffer.Bind(program);
        }

        /// <summary>
        /// Unbind all UBOs
        /// </summary>
        public static void UnbindAll()
        {
            DirectionalLightBuffer.Unbind();
            WorldCameraBuffer.Unbind();
            TimeBuffer.Unbind();
        }
    }
}
