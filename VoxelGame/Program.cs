using System;

namespace VoxelGame
{
    public static class Program
    {
        /// <summary>
        /// Game Settings
        /// </summary>
        public static Settings Settings;

        /// <summary>
        /// Main Game Window
        /// </summary>
        public static Window Window { get; private set; }

        /// <summary>
        /// Whether the game is running or not
        /// </summary>
        public static bool IsRunning { get; private set; }

        [STAThread]
        static void Main(string[] args)
        {
            //Debug.DebugLevel = DebugLevel.Warning;

#if !DEBUG
            try
            {
#endif
            IsRunning = true;
            LoadSettings();
            using (Window = new Window(Settings.WindowWidth, Settings.WindowHeight, "Voxel Game"))
                Window.Run();
            IsRunning = false;
#if (!DEBUG)
            }
            catch(Exception ex)
            {
                IsRunning = false;
                // TODO: Add WinForms as dependency - or add custom MessageBox impl.
                //MessageBox.Show(ex.Message + " " + ex.StackTrace, "Fatal Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
#endif      
        }

        static void LoadSettings()
        {
            Settings = Settings.Load();
        }
    }
}
