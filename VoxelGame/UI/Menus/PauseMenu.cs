using VoxelGame.Entities;
using VoxelGame.Worlds;

namespace VoxelGame.UI.Menus
{
    /// <summary>
    /// Pause menu implementation
    /// </summary>
    public class PauseMenu : Menu
    {
        private readonly GUIStyle _titleStyle = (GUIStyle)GUI.DefaultLabelStyle.Clone();
        private readonly OptionsMenu _options = new OptionsMenu();

        public override void Show()
        {
            _options.PreviousMenu = this;
            _options.Close();

            Player.SetMouseVisible(true);
            Player.SetControlsActive(false);

            _titleStyle.HorizontalAlignment = HorizontalAlignment.Middle;
            _titleStyle.VerticalAlignment = VerticalAlignment.Middle;
            _titleStyle.FontSize = 92;

            base.Show();
        }

        public override void OnGUI()
        {
            float winWidth = Program.Window.Width;
            float winHeight = Program.Window.Height;


            var centerX = winWidth * 0.5f;
            var centerY = winHeight * 0.5f;

            // Draw the buttons

            if (GUI.Button("Resume", new Rect(centerX - 200, centerY - 75, 400, 40)))
            {
                Close();
                Player.SetMouseVisible(false);
                Player.SetControlsActive(true);

            }

            if (GUI.Button("Options", new Rect(centerX - 200, centerY - 25, 400, 40)))
            {
                this.Close();
                _options.Show();
            }

            if (GUI.Button("Quit", new Rect(centerX - 200, centerY + 25, 200 * 2, 40)))
            {
                Close();
                World.Instance.Dispose();
                Program.Window.MainMenu.Show();
            }
        }
    }
}
