using OpenTK;
using OpenTK.Graphics.OpenGL4;
using VoxelGame.Assets;
using VoxelGame.Entities;
using VoxelGame.Rendering;

namespace VoxelGame.UI.Menus
{
    /// <summary>
    /// Main Menu implementation
    /// </summary>
    public class MainMenu : Menu
    {
        private readonly GUIStyle _titleStyle = (GUIStyle)GUI.DefaultLabelStyle.Clone();
        private readonly OptionsMenu _options = new OptionsMenu();
        private readonly DebugMenu _debug = new DebugMenu();
        private Texture _bg;

        public MainMenu()
        {
            _bg = AssetDatabase.GetAsset<Texture>("Textures/GUI/menu_bg.png");
            _bg.WrapMode = TextureWrapMode.Repeat;
        }

        public override void Show()
        {
            Player.SetMouseVisible(true);

            _titleStyle.HorizontalAlignment = HorizontalAlignment.Middle;
            _titleStyle.VerticalAlignment = VerticalAlignment.Middle;
            _titleStyle.FontSize = 92;

            _options.PreviousMenu = this;

            base.Show();
        }

        public override void OnGUI()
        {
            float winWidth = Program.Window.Width;
            float winHeight = Program.Window.Height;

            // Draw tiled background
            Vector2 scale = new Vector2(winWidth / _bg.Width, winHeight / _bg.Height) / 4f;
            GUI.Image(_bg, new Rect(0, 0, winWidth, winHeight), Vector2.Zero, scale);

            // Draw "logo"
            GUI.Label("VOXEL GAME", new Rect(0, 256, winWidth, 72), _titleStyle);

            var centerX = winWidth * 0.5f;
            var centerY = winHeight * 0.5f;

            // Draw buttons
            if (GUI.Button("New world", new Rect(centerX - 200, centerY - 75, 400, 40)))
            {
                new NewWorldMenu { PreviousMenu = this }.Show();
                Close();
            }

            if (GUI.Button("Load world", new Rect(centerX - 200, centerY - 25, 400, 40)))
            {
                // TODO: Re-enable world loading once fixed
            }

            if (GUI.Button("Options", new Rect(centerX - 200, centerY + 25, 400, 40)))
            {
                _options.Show();
                Close();
            }

            if (GUI.Button("Quit", new Rect(centerX - 200, centerY + 75, 400, 40)))
            {
                Program.Window.Close();
            }

            if (GUI.Button(_debug.IsOpen ? "CLOSE" : "DEBUG", new Rect(winWidth - 210, winHeight - 50, 200, 40)))
            {
                if (_debug.IsOpen)
                    _debug.Close();
                else
                    _debug.Show();
            }
        }
    }
}
