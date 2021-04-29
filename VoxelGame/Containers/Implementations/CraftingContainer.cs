using OpenTK;
using System.Collections.Generic;
using VoxelGame.Crafting;
using VoxelGame.UI;

namespace VoxelGame.Containers
{
    /// <summary>
    /// Crafting container logic
    /// </summary>
    public class CraftingContainer : Container
    {
        public override Vector2 ContainerSize { get; } = new Vector2(2, 2);

        /// <summary>
        /// The crafting recipe that matches input items
        /// </summary>
        public CraftingRecipe OutputRecipe { get; private set; } = null;

        /// <summary>
        /// The item stack that matches input items
        /// </summary>
        public ItemStack OutputStack { get; private set; } = null;

        /// <summary>
        /// Updates the output recipe and stack
        /// </summary>
        public void UpdateOutput()
        {
            // Get matching recipe for this container
            OutputRecipe = CraftingRecipeDatabase.GetMatchingRecipe(this);

            // Create output item stack if recipe found
            if (OutputRecipe != null)
                OutputStack = new ItemStack(OutputRecipe.Output.ItemKey, OutputRecipe.Output.Count, new Vector2(-1, -1));
            else
                OutputStack = null;
        }

        public override void RenderGUI()
        {
            float winWidth = Window.WindowWidth;
            float winHeight = Window.WindowHeight;

            float centerX = winWidth / 2f;
            float centerY = winHeight / 2f;

            float slotSize = ContainerRenderer.SLOT_SIZE;
            Vector2 size = new Vector2(ContainerSize.X + 1, ContainerSize.Y) * slotSize;
            
            // Container size
            Rect parentRect = new Rect(centerX - size.X * 0.5f, centerY - (3.5f * slotSize), size.X + slotSize, size.Y);

            // Draw input slots
            bool anySlotSelected = false;
            for (int x = 0; x < ContainerSize.X; x++)
            {
                for (int y = 0; y < ContainerSize.Y; y++)
                {
                    var rect = new Rect(x * slotSize + parentRect.X, (ContainerSize.Y - 1 - y) * slotSize + parentRect.Y + 10, slotSize, slotSize);

                    if (rect.IsPointInside(GUI.MousePosition))
                    {
                        SelectedSlot = new Vector2(x, y);
                        anySlotSelected = true;
                    }

                    RenderCell(x, y, rect);
                }
            }

            // Draw output slot
            var outputRect = new Rect(2 * slotSize + slotSize + parentRect.X, (ContainerSize.Y - 1 - 0.5f) * slotSize + parentRect.Y + 10, slotSize, slotSize);
            RenderOutputCell(outputRect);

            if (anySlotSelected == false)
                SelectedSlot = new Vector2(-1, -1);
        }

        public override void ItemDroppedIntoContainer(ItemStack itemStack) => UpdateOutput();
        public override void ItemRemovedFromContainer(ItemStack itemStack) => UpdateOutput();

        /// <summary>
        /// Removes one from each item in the input grid
        /// </summary>
        private void RemoveOneOfEach()
        {
            List<ItemStack> indicesToRemove = new List<ItemStack>();

            // Remove from each stack - if stack is now empty mark is for removal
            for (int i = 0; i < ItemsList.Count; i++)
                if (ItemsList[i].RemoveFromStack() == ItemStackState.Empty)
                    indicesToRemove.Add(ItemsList[i]);

            // Remove any empty item stack
            for (int i = 0; i < indicesToRemove.Count; i++)
                ItemsList.Remove(indicesToRemove[i]);
        }

        /// <summary>
        /// Renders output slot rectangle
        /// </summary>
        /// <param name="rect">Rectangle to draw in</param>
        protected void RenderOutputCell(Rect rect)
        {
            var rectIcon = rect.Shrink(2);

            // Draw background
            GUI.Image(ContainerRenderer.ContainerSlot, rect, 2);

            // If we have crafable item
            var stack = OutputStack;
            if (stack != null)
            {
                // Draw icon
                if (GUI.PressButton(stack.Item.Icon, rectIcon, SlotStyle)) // If pressed
                {
                    // If nothing is selected
                    if (ContainerRenderer.SelectedStack == null)
                    {
                        // Create stack copy and set is as selection
                        stack = (ItemStack)stack.Clone();
                        stack.PreviousParent = this;
                        ContainerRenderer.SelectedStack = stack;

                        // Remove consumed items on input
                        RemoveOneOfEach();

                        // Update output
                        UpdateOutput();
                    }
                    // TODO: Append to existing selection, if is the same item and can be added
                }

                // Draw item stack size
                GUI.Label(stack.StackSize.ToString(), rect, ContainerRenderer.SlotNumberStyle);
            }
        }
    }
}
