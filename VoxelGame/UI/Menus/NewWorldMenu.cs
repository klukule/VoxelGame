using OpenTK;
using OpenTK.Graphics.OpenGL4;
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
            GUI.Textbox(ref seed, "World Seed", 100, new Rect(centerX - 200, centerY - 27, 400, 40));

            // Draw the buttons
            if (GUI.Button("Back", new Rect(centerX - 200, centerY + 25, 195, 40)))
            {
                if (PreviousMenu != null)
                    PreviousMenu.Show();

                Close();
            }

            if (GUI.Button("Apply", new Rect(centerX + 5, centerY + 25, 195, 40)))
            {
                Debug.Log("Seed: " + seed.GetSeed());
                _ = new World("New World", seed);
                Player.SetMouseVisible(false);

                Close();
            }
        }
    }
}
