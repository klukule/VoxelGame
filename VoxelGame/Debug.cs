using System;

namespace VoxelGame
{
    /// <summary>
    /// Debug level
    /// </summary>
    public enum DebugLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Debug = 3
    }

    /// <summary>
    /// Debug logger
    /// </summary>
    public static class Debug
    {
        /// <summary>
        /// Minimum Debug Level to be logged
        /// </summary>
        public static DebugLevel DebugLevel { get; set; } = DebugLevel.Warning;

        /// <summary>
        /// Causes assertion to be called
        /// </summary>
        /// <param name="msg">Message to display</param>
        public static void Assert(string msg = null)
        {
            Program.Window.CursorGrabbed = false;
            Program.Window.CursorVisible = true;

            if (string.IsNullOrEmpty(msg))
                System.Diagnostics.Debug.Assert(false);
            else
                System.Diagnostics.Debug.Assert(false, msg);
        }

        /// <summary>
        /// Logs message to console
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="level">Log level</param>
        public static void Log(string message, DebugLevel level = DebugLevel.Debug)
        {
            if (DebugLevel >= level)
                Console.WriteLine($"[{level.ToString().ToUpper(),-7}] {message}");
        }
        /// <summary>
        /// Logs message to console
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="level">Log level</param>
        public static void Log(object message, DebugLevel level = DebugLevel.Debug) => Log(message.ToString(), level);
    }
}
