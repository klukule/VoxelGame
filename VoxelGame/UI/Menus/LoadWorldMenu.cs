using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VoxelGame.Assets;
using VoxelGame.Entities;
using VoxelGame.Rendering;
using VoxelGame.Worlds;

namespace VoxelGame.UI.Menus
{
    /// <summary>
    /// Load world menu implementation
    /// </summary>
    public class LoadWorldMenu : Menu
    {
        private readonly GUIStyle _titleStyle = (GUIStyle)GUI.DefaultLabelStyle.Clone();
        private readonly Texture _bg;

        private readonly Dictionary<string, string> _worlds = new Dictionary<string, string>();

        /// <summary>
        /// Menu to return to when exitting
        /// </summary>
        public Menu PreviousMenu { get; set; }

        public LoadWorldMenu()
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

            _worlds.Clear();
            var saves = Directory.GetDirectories(World.WORLD_SAVE_DIRECTORY);
            foreach (var save in saves)
                if(File.Exists(save + "\\Save.world"))
                    _worlds.Add(save.Split('\\', StringSplitOptions.RemoveEmptyEntries).Last(), save + "\\Save.world");

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
            GUI.Label("LOAD WORLD", new Rect(0, 256, winWidth, 72), _titleStyle);

            var centerX = winWidth * 0.5f;
            var centerY = winHeight * 0.5f;

            var worldListHeight = 50 * _worlds.Count;
            var worldListRect = new Rect(centerX - 200, centerY - (worldListHeight * 0.5f), 400, 40);

            foreach (var save in _worlds)
            {
                if(GUI.Button(save.Key, worldListRect))
                {
                    AssetDatabase.GetAsset<World>(save.Value, false);
                    Player.SetMouseVisible(false);
                    Close();
                    // Load
                }
                worldListRect.Y += 50;
            }

            // Draw the buttons
            if (GUI.Button("Back", new Rect(centerX - 200, worldListRect.Y + worldListRect.Height + 20, 400, 40)))
            {
                if (PreviousMenu != null)
                    PreviousMenu.Show();

                Close();
            }
        }
    }
}
