using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Linq;
using VoxelGame.Assets;
using VoxelGame.Entities;
using VoxelGame.Items;
using VoxelGame.Rendering;
namespace VoxelGame.UI.Menus
{
    public class DebugMenu : Menu
    {
        private Texture _background;
        private Texture _slotBackground;
        private Texture _tooltipBackground;
        private int _activeTab = 0;
        private FBO _dropPreviewFbo;
        private Texture _dropPreviewTexture;
        private ItemEntity _dropIconEntity;
        public override void Show()
        {
            _background = AssetDatabase.GetAsset<Texture>("Textures/GUI/inventory_bg.png");
            _slotBackground = AssetDatabase.GetAsset<Texture>("Textures/GUI/inventory_slot.png");
            _tooltipBackground = AssetDatabase.GetAsset<Texture>("Textures/GUI/txt_normal.png");

            _dropPreviewFbo = new FBO(512, 512, FBO.DepthBufferType.Texture);
            _dropPreviewTexture = new Texture(_dropPreviewFbo, false);
            _dropIconEntity = new ItemEntity(GameItems.GRASS);
            base.Show();
        }

        public override void Close()
        {
            if (_dropPreviewFbo != null)
            {
                _dropPreviewFbo.Dispose();
                _dropPreviewFbo = null;
            }

            if (_dropPreviewTexture != null)
            {
                _dropPreviewTexture.Dispose();
                _dropPreviewTexture = null;
            }

            base.Close();
        }

        public override void OnGUI()
        {
            base.OnGUI();

            var winWidth = Window.WindowWidth;
            var winHeight = Window.WindowHeight;

            var windowRect = new Rect(50, 50, winWidth - 100, winHeight - 100);
            GUI.Image(_background, windowRect, 5);

            var buttonRect = new Rect(windowRect.X + 10, windowRect.Y + 10, 150, 40);
            if (GUI.Button("Font atlas", buttonRect))
                _activeTab = 0;

            buttonRect.X += 160;
            if (GUI.Button("Item icons", buttonRect))
                _activeTab = 1;

            buttonRect.X += 160;
            if (GUI.Button("Drop icon", buttonRect))
                _activeTab = 2;

            var contentRect = new Rect(windowRect.X + 10, windowRect.Y + 10 + buttonRect.Height + 10, windowRect.Width - 20, windowRect.Height - 20 - buttonRect.Height - 20);

            // Font atlas debugging
            if (_activeTab == 0)
            {
                var atlasAspect = GUI.DefaultLabelStyle.Font.AtlasWidth / GUI.DefaultLabelStyle.Font.AtlasHeight;
                contentRect.Height = contentRect.Width / atlasAspect;
                GUI.Image(_slotBackground, contentRect, 2);
                GUI.Image(GUI.DefaultLabelStyle.Font.AtlasTexture, contentRect.Shrink(10));
            }

            // Item icon debugging
            if (_activeTab == 1)
            {

                string tooltip = "";
                Vector2 tooltipPosition = Vector2.Zero;
                bool tooltipActive = false;

                var items = ItemDatabase.GetItems().OrderBy(i => i.ID);

                var slotRect = new Rect(contentRect.X, contentRect.Y, 69, 69);
                foreach (var item in items)
                {
                    if (contentRect.X - slotRect.X + slotRect.Width > contentRect.Width)
                    {
                        slotRect.X = contentRect.X;
                        slotRect.Y += 79;
                    }

                    if (item.Icon != null)
                    {
                        GUI.Image(_slotBackground, slotRect, 2);
                        GUI.Image(item.Icon, slotRect.Shrink(5));

                        if (slotRect.IsPointInside(GUI.MousePosition))
                        {
                            tooltip = $"ID: {item.ID}\nKey: {item.Key}\nIcon: {item.IconLocation}";
                            tooltipPosition = GUI.MousePosition;
                            tooltipActive = true;
                        }
                    }

                    slotRect.X += 79;
                }

                if (tooltipActive)
                {
                    // TODO: Calculate layout
                    var tooltipRect = new Rect(tooltipPosition.X + 16, tooltipPosition.Y + 16, 550, 60);
                    GUI.Image(_tooltipBackground, tooltipRect, 2);
                    GUI.Label(tooltip, tooltipRect.Shrink(5));
                }
            }

            // 3D drop icon preview
            if (_activeTab == 2)
            {
                _dropPreviewFbo.Bind();
                GL.ClearColor(0, 0, 0, 0);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                // Draw rotating icon here

                _dropPreviewFbo.Unbind();

                var size = Math.Min(contentRect.Width, contentRect.Height);
                var textureRect = new Rect(contentRect.X + contentRect.Width * 0.5f - size * 0.5f, contentRect.Y + contentRect.Height * 0.5f - size * 0.5f, size, size);
                GUI.Image(_dropPreviewTexture, textureRect);
            }
        }
    }
}
