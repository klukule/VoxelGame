using OpenTK;
using OpenTK.Graphics;
using OpenTK.Input;
using System.Collections.Generic;
using VoxelGame.Assets;
using VoxelGame.Rendering;
using VoxelGame.UI;

namespace VoxelGame.Containers
{
    /// <summary>
    /// Container rendering and interaction logic
    /// </summary>
    public static class ContainerRenderer
    {
        // List of open containers to draw
        private static readonly List<Container> _toDraw = new List<Container>();

        // Slot size
        public const float SLOT_SIZE = 40;

        /// <summary>
        /// Container background texture
        /// </summary>
        public static Texture ContainerBackground { get; private set; }

        /// <summary>
        /// Container slot texture
        /// </summary>
        public static Texture ContainerSlot { get; private set; }

        /// <summary>
        /// Stack that can't be selected
        /// </summary>
        public static ItemStack StackBlockedForSelection { get; set; }

        /// <summary>
        /// Currently selected stack
        /// </summary>
        public static ItemStack SelectedStack { get; set; }

        /// <summary>
        /// Slot number style
        /// </summary>
        public static GUIStyle SlotNumberStyle { get; private set; }

        static ContainerRenderer()
        {
            // Load textures
            ContainerBackground = AssetDatabase.GetAsset<Texture>("Textures/GUI/inventory_bg.png");
            ContainerSlot = AssetDatabase.GetAsset<Texture>("Textures/GUI/inventory_slot.png");

            // Create slot number style
            SlotNumberStyle = new GUIStyle()
            {
                Font = GUI.DefaultButtonStyle.Font,
                FontSize = 28,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                AlignmentOffset = new Vector2(9, 8),
                Normal = new GUIStyleOption() { TextColor = Color4.White }
            };

            // Hook mouse click
            Input.MouseDown += (sender, args) =>
            {
                if (args.Button == MouseButton.Left) // Drop all
                {
                    // If we have something selected
                    if (SelectedStack != null)
                    {

                        for (int i = 0; i < _toDraw.Count; i++)
                        {
                            // Skip containers that the selection doesn't originate from
                            if (_toDraw[i].SelectedSlot == new Vector2(-1, -1)) continue;

                            // If selected slot is free
                            if (_toDraw[i].GetIsSlotFree(_toDraw[i].SelectedSlot))
                            {
                                // Drop the selection to that slot
                                SelectedStack.LocationInContainer = _toDraw[i].SelectedSlot;
                                _toDraw[i].ItemsList.Add(SelectedStack);
                                _toDraw[i].ItemDroppedIntoContainer(SelectedStack);
                                StackBlockedForSelection = SelectedStack;
                                SelectedStack = null;
                                return;
                            }
                            else // Selected slot is occupied
                            {
                                // Get the stack there
                                var stackAtLocation = _toDraw[i].GetItemStackByLocation(_toDraw[i].SelectedSlot);
                                if (stackAtLocation.ItemKey == SelectedStack.ItemKey) // If same item
                                {
                                    // Add to that stack
                                    if (stackAtLocation.AddToStack(SelectedStack.StackSize) == ItemStackState.Full) // Did not add everything
                                    {
                                        // Set remaining size in the selection
                                        SelectedStack.StackSize = stackAtLocation.StackSize + stackAtLocation.StackSize - stackAtLocation.Item.MaxStackSize;
                                        StackBlockedForSelection = stackAtLocation;
                                    }
                                    else // Added everything, selection is now empty
                                    {
                                        StackBlockedForSelection = SelectedStack;
                                        SelectedStack = null;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
                else if (args.Button == MouseButton.Right) // Add one
                {
                    // If we have something selected
                    if (SelectedStack != null)
                    {
                        for (int i = 0; i < _toDraw.Count; i++)
                        {
                            // Skip containers that the selection doesn't originate from
                            if (_toDraw[i].SelectedSlot == new Vector2(-1, -1)) continue;

                            // If selected slot is free
                            if (_toDraw[i].GetIsSlotFree(_toDraw[i].SelectedSlot))
                            {
                                ItemStack newStack = new ItemStack(SelectedStack.ItemKey, 1, _toDraw[i].SelectedSlot);
                                _toDraw[i].ItemsList.Add(newStack);
                                _toDraw[i].ItemDroppedIntoContainer(newStack);

                                if (SelectedStack.RemoveFromStack() == ItemStackState.Empty)
                                    SelectedStack = null;

                                return;
                            }
                            else // Selected slot is occupied
                            {
                                // Check if target slot is of same item and is not full
                                var stackAtLocation = _toDraw[i].GetItemStackByLocation(_toDraw[i].SelectedSlot);
                                if (stackAtLocation.ItemKey == SelectedStack.ItemKey && !stackAtLocation.IsStackFull)
                                {
                                    // Add one
                                    stackAtLocation.AddToStack();

                                    // Check if selection is empty
                                    if (SelectedStack.RemoveFromStack() == ItemStackState.Empty)
                                        SelectedStack = null;
                                    return;
                                }
                            }
                        }
                    }
                }

                // Unblock
                StackBlockedForSelection = null;
            };
        }

        /// <summary>
        /// Open container
        /// </summary>
        /// <param name="container">The container</param>
        public static void OpenContainer(Container container)
        {
            if (!_toDraw.Contains(container))
            {
                _toDraw.Add(container);
                container.IsOpen = true;
            }
        }

        /// <summary>
        /// Close container
        /// </summary>
        /// <param name="container">The container</param>
        public static void CloseContainer(Container container)
        {
            if (_toDraw.Contains(container))
            {
                _toDraw.Remove(container);
                container.IsOpen = false;
            }
        }

        /// <summary>
        /// Render container GUI
        /// </summary>
        public static void RenderGUI()
        {
            // Draw container GUI
            for (int i = 0; i < _toDraw.Count; i++)
                _toDraw[i].RenderGUI();

            // Draw floating selection at cursor position
            if (SelectedStack != null)
            {
                var rect = new Rect(GUI.MousePosition.X, GUI.MousePosition.Y, SLOT_SIZE - 8, SLOT_SIZE - 8);
                if (GUI.PressButton(SelectedStack.Item.Icon, rect)) { } // Item icon
                GUI.Label(SelectedStack.StackSize.ToString(), rect, SlotNumberStyle); // Slot number
            }
        }
    }
}
