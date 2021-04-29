using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Linq;
using VoxelGame.Assets;
using VoxelGame.Rendering;

namespace VoxelGame.UI.Menus
{
    /// <summary>
    /// Options menu implementation
    /// </summary>
    public class OptionsMenu : Menu
    {
        private readonly GUIStyle _titleStyle = (GUIStyle)GUI.DefaultLabelStyle.Clone();
        private readonly Texture _bg;
        private bool fullscreenVal = false;
        private IList<DisplayResolution> possibleDisplayResolutions;
        private int selectedRes = 0;

        /// <summary>
        /// Menu to return to when exitting
        /// </summary>
        public Menu PreviousMenu { get; set; }

        public OptionsMenu()
        {
            // Gety currently selected resolution
            var cur = DisplayDevice.GetDisplay(DisplayIndex.Default).SelectResolution(
                Program.Settings.WindowWidth,
                Program.Settings.WindowHeight,
                Program.Settings.BitsPerPixel,
                Program.Settings.RefreshRate
            );

            // Get all possible
            possibleDisplayResolutions = DisplayDevice.GetDisplay(DisplayIndex.Default).AvailableResolutions.Where(x => x.BitsPerPixel == cur.BitsPerPixel)
                .Distinct() // Get only distinct (multi-monitor setups return duplicite values)
                .OrderBy(r => r.Width) // Order the resolutions
                .ThenBy(r => r.Height)
                .ThenBy(r => r.RefreshRate)
                .ToList();

            // Select current
            selectedRes = possibleDisplayResolutions.IndexOf(cur);

            _bg = AssetDatabase.GetAsset<Texture>("Textures/GUI/menu_bg.png");
            _bg.WrapMode = TextureWrapMode.Repeat;

        }

        public override void Show()
        {
            // Setup customized title style
            _titleStyle.HorizontalAlignment = HorizontalAlignment.Middle;
            _titleStyle.VerticalAlignment = VerticalAlignment.Middle;
            _titleStyle.FontSize = 92;

            // TODO: Move currently selected resoltuion resolve here

            // Load current fullscreen value
            fullscreenVal = Program.Settings.Fullscreen;

            base.Show();
        }

        public override void OnGUI()
        {
            float winWidth = Program.Window.Width;
            float winHeight = Program.Window.Height;

            // Draw tiled background
            Vector2 scale = new Vector2(winWidth / _bg.Width, winHeight / _bg.Height) / 4f;
            GUI.Image(_bg, new Rect(0, 0, winWidth, winHeight), Vector2.Zero, scale);

            // Draw section title
            GUI.Label("SETTINGS", new Rect(0, 256, winWidth, 72), _titleStyle);

            var centerX = winWidth * 0.5f;
            var centerY = winHeight * 0.5f;

            var cur = possibleDisplayResolutions[selectedRes];

            // Draw buttons
            if (GUI.Button($"Size: {cur.Width}x{cur.Height}@{cur.RefreshRate}hz", new Rect(centerX - 200, centerY - 75, 400, 40)))
            {
                if (++selectedRes >= possibleDisplayResolutions.Count)
                    selectedRes = 0;
            }

            if (GUI.Button($"Fullscreen: {(fullscreenVal ? "ON" : "OFF")}", new Rect(centerX - 200, centerY - 25, 400, 40)))
            {
                fullscreenVal = !fullscreenVal;
            }

            if (GUI.Button("Back", new Rect(centerX - 200, centerY + 25, 195, 40)))
            {
                if (PreviousMenu != null)
                    PreviousMenu.Show();

                Close();

                fullscreenVal = Program.Settings.Fullscreen;
            }

            if (GUI.Button("Apply", new Rect(centerX + 5, centerY + 25, 195, 40)))
            {
                if (PreviousMenu != null)
                    PreviousMenu.Show();

                Close();

                Program.Settings.ApplyWindowSettings(cur, fullscreenVal);
            }
        }
    }
}
