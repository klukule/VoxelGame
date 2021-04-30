using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using VoxelGame.Assets;
using VoxelGame.Entities;
using VoxelGame.Rendering;
using VoxelGame.Worlds;

namespace VoxelGame.UI.Menus
{
    /// <summary>
    /// New world menu implementation
    /// </summary>
    public class NewWorldMenu : Menu
    {
        private readonly GUIStyle _titleStyle = (GUIStyle)GUI.DefaultLabelStyle.Clone();
        private readonly Texture _bg;
        private string seed = "";
        private string name = "";

        /// <summary>
        /// Menu to return to when exitting
        /// </summary>
        public Menu PreviousMenu { get; set; }

        public NewWorldMenu()
        {
            _bg = AssetDatabase.GetAsset<Texture>("Textures/GUI/menu_bg.png");
            _bg.WrapMode = TextureWrapMode.Repeat;
        }

        public override void Show()
        {
            // Setup customized title style
            _titleStyle.HorizontalAlignment = HorizontalAlignment.Middle;
            _titleStyle.VerticalAlignment = VerticalAlignment.Middle;
            _titleStyle.FontSize = 92;

            seed = "";
            name = "New World";

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
            GUI.Label("NEW WORLD", new Rect(0, 256, winWidth, 72), _titleStyle);

            var centerX = winWidth * 0.5f;
            var centerY = winHeight * 0.5f;

            // Draw seed input
            GUI.Textbox(ref name, "World Name", 32, new Rect(centerX - 200, centerY - 75, 400, 40));
            GUI.Textbox(ref seed, "World Seed", 32, new Rect(centerX - 200, centerY - 25, 400, 40));

            // Draw the buttons
            if (GUI.Button("Back", new Rect(centerX - 200, centerY + 25, 195, 40)))
            {
                if (PreviousMenu != null)
                    PreviousMenu.Show();

                Close();
            }

            if (GUI.Button("Apply", new Rect(centerX + 5, centerY + 25, 195, 40)))
            {
                // If no seed is entered create new 16 chars long alphanumberic seed
                if (string.IsNullOrEmpty(seed))
                {
                    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
                    var stringChars = new char[16];
                    var random = new Random();

                    for (int i = 0; i < stringChars.Length; i++)
                    {
                        stringChars[i] = chars[random.Next(chars.Length)];
                    }

                    seed = new string(stringChars);
                }

                Debug.Log("Seed: " + seed.GetSeed());
                _ = new World(name, seed);
                Player.SetMouseVisible(false);

                Close();
            }
        }
    }
}
