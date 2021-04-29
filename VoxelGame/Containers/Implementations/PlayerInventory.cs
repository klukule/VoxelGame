using OpenTK;
using VoxelGame.Assets;
using VoxelGame.Rendering;
using VoxelGame.UI;

namespace VoxelGame.Containers
{
    /// <summary>
    /// Player inventory UI and logic
    /// </summary>
    public class PlayerInventory : Container
    {
        private static Texture _toolbarBg;
        private static Texture _toolbarItem;
        private static Texture _toolbarSelectedOverlay;

        // Player inventory size
        private Vector2 _size = new Vector2(9, 4);
        public override Vector2 ContainerSize => _size;

        /// <summary>
        /// Selected toolbar slot
        /// </summary>
        public int SelectedItemIndex { get; set; } = 0;

        public PlayerInventory()
        {
            _toolbarBg = AssetDatabase.GetAsset<Texture>("Textures/GUI/toolbar_bg.png");
            _toolbarItem = AssetDatabase.GetAsset<Texture>("Textures/GUI/toolbar_slot.png");
            _toolbarSelectedOverlay = AssetDatabase.GetAsset<Texture>("Textures/GUI/toolbar_selected.png");
        }

        /// <summary>
        /// Render toolbar
        /// </summary>
        public void RenderToolBar()
        {
            float winWidth = Window.WindowWidth;
            float winHeight = Window.WindowHeight;

            float centerX = winWidth * 0.5f;
            float centerY = winHeight * 0.5f;

            float slotSize = ContainerRenderer.SLOT_SIZE;
            Vector2 size = new Vector2(ContainerSize.X, 1) * slotSize;

            // Draw background
            Rect toolbarRect = new Rect(centerX - size.X * 0.5f, winHeight - size.Y * 0.5f - slotSize, size.X, size.Y);
            GUI.Image(_toolbarBg, toolbarRect.Expand(2), 2);

            Rect selectedItem = null;

            // Draw container items
            for (int x = 0; x < ContainerSize.X; x++)
            {
                var slotRect = new Rect(toolbarRect.X + slotSize * x, toolbarRect.Y, slotSize, slotSize);

                // Draw cell with custom background
                GUI.Image(_toolbarItem, slotRect);
                RenderCell(x, 0, slotRect.Shrink(1), false);

                if (x == SelectedItemIndex) selectedItem = slotRect;
            }

            // Draw overlay on top
            if (selectedItem != null)
                GUI.Image(_toolbarSelectedOverlay, selectedItem.Expand(4));
        }

        // Standard Container UI logic
        public override void RenderGUI()
        {
            float winWidth = Window.WindowWidth;
            float winHeight = Window.WindowHeight;

            float centerX = winWidth * 0.5f;
            float centerY = winHeight * 0.5f;

            float slotSize = ContainerRenderer.SLOT_SIZE;
            Vector2 size = new Vector2(ContainerSize.X, ContainerSize.Y + 3) * slotSize; // 3 spaces for crafting UI (2 slots + one space)

            // Draw background
            Rect parentRect = new Rect(centerX - size.X * 0.5f, centerY - size.Y * 0.5f, size.X, size.Y).Expand(10, 15); // Add 20px horizontal, and 30 px vertical

            GUI.Image(ContainerRenderer.ContainerBackground, parentRect, 5);

            // Draw the slots
            bool anyHovered = false;
            for (int x = 0; x < ContainerSize.X; x++)
            {
                for (int y = (int)ContainerSize.Y - 1; y >= 0; y--)
                {
                    var rect = new Rect(x * slotSize + parentRect.X + 10, (ContainerSize.Y - 1 - (y - 3)) * slotSize + parentRect.Y + 10, slotSize, slotSize);
                    if (y == 0) rect.Y += 10;

                    if (rect.IsPointInside(GUI.MousePosition))
                    {
                        SelectedSlot = new Vector2(x, y);
                        anyHovered = true;
                    }

                    RenderCell(x, y, rect);
                }
            }

            if (!anyHovered) SelectedSlot = new Vector2(-1, -1);
        }
    }
}