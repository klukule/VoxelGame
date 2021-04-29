
namespace VoxelGame
{
    /// <summary>
    /// Game time wrapper - uses OpenTK callbacks as time source
    /// </summary>
    public static class Time
    {
        private static float _lastFpsUpdateTime = 0;
        
        /// <summary>
        /// Absolute game time since start
        /// </summary>
        public static float GameTime { get; set; }

        /// <summary>
        /// Delta time between frames
        /// </summary>
        public static float DeltaTime { get; set; }

        /// <summary>
        /// Frames per second
        /// </summary>
        public static float FramesPerSecond { get; private set; }

        // TODO: Move to setter
        /// <summary>
        /// Update FPS
        /// </summary>
        /// <param name="fps">The FPS</param>
        public static void UpdateFrameRate(float fps)
        {
            if (GameTime > _lastFpsUpdateTime + 0.5)
            {
                FramesPerSecond = fps;
                _lastFpsUpdateTime = GameTime;
            }
        }
    }
}
