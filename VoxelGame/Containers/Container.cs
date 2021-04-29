using OpenTK;
using OpenTK.Graphics;
using System.Collections.Generic;
using System.Linq;
using VoxelGame.Assets;
using VoxelGame.Items;
using VoxelGame.Rendering;
using VoxelGame.UI;

namespace VoxelGame.Containers
{
    /// <summary>
    /// Base for any physical or virtual in-world container
    /// </summary>
    public class Container
    {
        /// <summary>
        /// Size of the container
        /// </summary>
        public virtual Vector2 ContainerSize { get; } = new Vector2(9, 5);

        /// <summary>
        /// Items stored in the list
        /// </summary>
        public List<ItemStack> ItemsList { get; private set; } = new List<ItemStack>();

        /// <summary>
        /// Whether the container is opened
        /// </summary>
        public bool IsOpen { get; set; }

        /// <summary>
        /// Selected slot
        /// </summary>
        public Vector2 SelectedSlot { get; set; }

        /// <summary>
        /// Slot GUI Style
        /// </summary>
        public GUIStyle SlotStyle = new GUIStyle()
        {
            FontSize = 48,
            Font = AssetDatabase.GetAsset<Font>("Fonts/Minecraft.ttf"),
            HorizontalAlignment = HorizontalAlignment.Middle,
            VerticalAlignment = VerticalAlignment.Middle,
            SlicedBorderSize = 1f,

            Normal = new GUIStyleOption()
            {
                Background = null,
                TextColor = Color4.White
            },
            Hover = new GUIStyleOption()
            {
                Background = AssetDatabase.GetAsset<Texture>("Textures/GUI/slot_hover.png"),
                TextColor = Color4.White
            },
            Active = new GUIStyleOption()
            {
                Background = AssetDatabase.GetAsset<Texture>("Textures/GUI/slot_hover.png"),
                TextColor = Color4.White
            }
        };


        public bool IsFull => GetFirstEmptyLocationInContainer() == new Vector2(-1, -1);

        /// <summary>
        /// Adds item to the container
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(Item item)
        {
            // Check if container is full
            if (IsFull) return;

            // Check if we can add to existing stack
            var stack = GetFirstEmptyStackByItem(item);
            if (stack != null && stack.WillStackBeFull(1))
            {
                stack.AddToStack();
            }
            else // If we can't create new stack
            {
                stack = new ItemStack(item, GetFirstEmptyLocationInContainer());
                ItemsList.Add(stack);
            }
        }

        /// <summary>
        /// Remove item from the container
        /// </summary>
        /// <param name="item">The item</param>
        public void RemoveItem(Item item)
        {
            RemoveItem(item.Key);
        }

        /// <summary>
        /// Remove item from the container
        /// </summary>
        /// <param name="itemKey">The item ID</param>
        public void RemoveItem(string itemKey)
        {
            var stack = GetFirstStackByItem(itemKey);
            if (stack != null && stack.RemoveFromStack() == ItemStackState.Empty)
                ItemsList.Remove(stack);
        }

        /// <summary>
        /// Removes item from stack
        /// </summary>
        /// <param name="item">The item to remove</param>
        /// <param name="stack">THe item stack to remove from</param>
        public void RemoveItemFromStack(Item item, ItemStack stack)
        {
            RemoveItemFromStack(item.Key, stack);
        }

        /// <summary>
        /// Removes item from stack
        /// </summary>
        /// <param name="item">The item key to remove</param>
        /// <param name="stack">THe item stack to remove from</param>
        public void RemoveItemFromStack(string itemKey, ItemStack stack)
        {
            if (stack != null && stack.RemoveFromStack() == ItemStackState.Empty)
                ItemsList.Remove(stack);
        }

        /// <summary>
        /// Checks whether the slot is free
        /// </summary>
        /// <param name="slot">The slot</param>
        /// <returns>True if there is nothing in the slot; otherwise false</returns>
        public bool GetIsSlotFree(Vector2 slot) => ItemsList.FirstOrDefault(x => x.LocationInContainer == slot) == null;

        /// <summary>
        /// Gets location of first empty slot
        /// </summary>
        /// <returns>Slot it; if none found returns [-1,-1]</returns>
        public Vector2 GetFirstEmptyLocationInContainer()
        {
            // Go through the container line by line from top to bottom
            for (int y = 0; y < ContainerSize.Y; y++)
            {
                for (int x = 0; x < ContainerSize.X; x++)
                {
                    // Get stack at location
                    var stack = ItemsList.FirstOrDefault(v =>
                        v.LocationInContainer.X == x && v.LocationInContainer.Y == y);

                    // If none, we have empty slot
                    if (stack == null)
                        return new Vector2(x, y);
                }
            }

            // No empty slot found
            return new Vector2(-1, -1);
        }

        /// <summary>
        /// Gets the location of first filled slot
        /// </summary>
        /// <returns>Location of the first filled slot; otherwise [-1,-1] if everything is empty</returns>
        public Vector2 GetFirstFilledLocationInContainer()
        {
            for (int y = 0; y < ContainerSize.Y; y++)
            {
                for (int x = 0; x < ContainerSize.X; x++)
                {
                    var stack = ItemsList.FirstOrDefault(v =>
                        v.LocationInContainer.X == x && v.LocationInContainer.Y == y);

                    if (stack != null)
                        return new Vector2(x, y);
                }
            }

            return new Vector2(-1, -1);
        }

        /// <summary>
        /// Get first non-full stack of given item type
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>Non-full stack for given item; if not found returns null</returns>
        private ItemStack GetFirstEmptyStackByItem(Item item) => ItemsList.FirstOrDefault(x => x.ItemKey == item.Key && !x.IsStackFull);

        /// <summary>
        /// Get first non-full stack of given item type
        /// </summary>
        /// <param name="key">The item</param>
        /// <returns>Non-full stack for given item; if not found returns null</returns>
        private ItemStack GetFirstEmptyStackByItem(string key) => ItemsList.FirstOrDefault(x => x.ItemKey == key && !x.IsStackFull);

        /// <summary>
        /// Gets first stack for given item
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>Item stack or null if not found</returns>
        private ItemStack GetFirstStackByItem(Item item) => GetFirstStackByItem(item.Key);

        /// <summary>
        /// Gets first stack for given item
        /// </summary>
        /// <param name="key">The item</param>
        /// <returns>Item stack or null if not found</returns>
        private ItemStack GetFirstStackByItem(string key) => ItemsList.FirstOrDefault(x => x.ItemKey == key);

        /// <summary>
        /// Gets item stack from location
        /// </summary>
        /// <param name="x">X</param>
        /// <param name="y">Y</param>
        /// <returns>The item stack or null if not found</returns>
        public ItemStack GetItemStackByLocation(int x, int y) => GetItemStackByLocation(new Vector2(x, y));

        /// <summary>
        /// Gets item stack from location
        /// </summary>
        /// <param name="loc">The location</param>
        /// <returns>The item stack or null if not found</returns>
        public ItemStack GetItemStackByLocation(Vector2 loc) => ItemsList.FirstOrDefault(x => x.LocationInContainer == loc);

        /// <summary>
        /// Opens the container UI
        /// </summary>
        public void Open() => ContainerRenderer.OpenContainer(this);

        /// <summary>
        /// Closes the container UI
        /// </summary>
        public void Close() => ContainerRenderer.CloseContainer(this);

        /// <summary>
        /// Renders clickable item cell at given location
        /// </summary>
        /// <param name="x">Container slot X location</param>
        /// <param name="y">Container slot Y location</param>
        /// <param name="rect">Rectangle to draw in</param>
        protected void RenderCell(int x, int y, Rect rect, bool drawBackground = true)
        {
            // Icon rectangle
            var rectIcon = rect.Shrink(2);

            // Draw slot background
            if(drawBackground)
                GUI.Image(ContainerRenderer.ContainerSlot, rect, 2);

            // Get item stack
            var stack = GetItemStackByLocation(x, y);
            if (stack != null) // If there is something
            {
                if (GUI.PressButton(stack.Item.Icon, rectIcon, SlotStyle)) // Clickable icon
                {
                    // If no stack is selected or stack is not blocked for selection - select the clicked item stack
                    if (ContainerRenderer.SelectedStack == null && ContainerRenderer.StackBlockedForSelection != stack)
                    {
                        // Store original container the stack belonged to
                        stack.PreviousParent = this;
                        ContainerRenderer.SelectedStack = stack;
                        ItemsList.Remove(stack);
                        ItemRemovedFromContainer(stack);
                    }
                }

                // Item stack number
                GUI.Label(stack.StackSize.ToString(), rect, ContainerRenderer.SlotNumberStyle);
            }
        }

        /// <summary>
        /// Render container GUI
        /// </summary>
        public virtual void RenderGUI()
        {
            float winWidth = Window.WindowWidth;
            float winHeight = Window.WindowHeight;
            float slotSize = ContainerRenderer.SLOT_SIZE;
            Vector2 size = ContainerSize * slotSize * 2;

            // Container background rectangle
            Rect parentRect = new Rect((winWidth / 2f) - (size.X / 4f) - slotSize / 2f, (winHeight / 2f) - (size.Y / 4f) - slotSize / 2f,
                (size.X / 2f + 35), (size.Y / 2f + 30));

            // Draw background
            GUI.Image(ContainerRenderer.ContainerBackground, parentRect, 5);

            // Draw slots
            bool anySlotSelected = false;
            for (int x = 0; x < ContainerSize.X; x++)
            {
                for (int y = 0; y < ContainerSize.Y; y++)
                {
                    // Slot rectangle
                    var rect = new Rect(x * (slotSize + 2) + parentRect.X + 10, y * (slotSize + 2) + parentRect.Y + 10,
                        slotSize, slotSize);

                    // Mouse hovering above the slot - mark it as such
                    if (rect.IsPointInside(GUI.MousePosition))
                    {
                        SelectedSlot = new Vector2(x, y);
                        anySlotSelected = true;
                    }

                    RenderCell(x, y, rect);
                }
            }

            // No slot hovered, deselect
            if (anySlotSelected == false)
                SelectedSlot = new Vector2(-1, -1);
        }

        /// <summary>
        /// Item (stack) was dropped to the container
        /// </summary>
        /// <param name="itemStack">The stack</param>
        public virtual void ItemDroppedIntoContainer(ItemStack itemStack) { }

        /// <summary>
        /// Item (stack) was removed from the container
        /// </summary>
        /// <param name="itemStack">The stack</param>
        public virtual void ItemRemovedFromContainer(ItemStack itemStack) { }
    }
}
