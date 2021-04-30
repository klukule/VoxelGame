using Newtonsoft.Json;
using OpenTK;
using System;
using VoxelGame.Items;

namespace VoxelGame.Containers
{
    /// <summary>
    /// Item stack state
    /// </summary>
    public enum ItemStackState
    {
        Normal,
        Empty,
        Full
    }

    /// <summary>
    /// Item stack
    /// </summary>
    public class ItemStack : ICloneable
    {
        private int _stackSize;

        /// <summary>
        /// Selected item id
        /// </summary>
        [JsonProperty]
        public string ItemKey { get; private set; }

        /// <summary>
        /// Selected item
        /// </summary>
        [JsonIgnore]
        public Item Item => ItemDatabase.GetItem(ItemKey);

        /// <summary>
        /// Amount of items in the stack
        /// </summary>
        public int StackSize
        {
            get => _stackSize;
            set => _stackSize = MathHelper.Clamp(value, 0, Item.MaxStackSize);
        }

        /// <summary>
        /// Container location
        /// </summary>
        [JsonConverter(typeof(Vector2Converter))]
        public Vector2 LocationInContainer { get; set; }

        /// <summary>
        /// Parent container
        /// - used to pair floating selection with originating container
        /// </summary>
        [JsonIgnore]
        public Container PreviousParent { get; set; }

        /// <summary>
        /// Whether the item stack is full or not
        /// </summary>
        [JsonIgnore]
        public bool IsStackFull => Item != null && Item.MaxStackSize == StackSize;

        /// <summary>
        /// Create empty item stack
        /// </summary>
        public ItemStack() { }

        /// <summary>
        /// Create stack of one item at container location
        /// </summary>
        /// <param name="item">The item</param>
        /// <param name="location">Container location</param>
        public ItemStack(Item item, Vector2 location) : this(item.Key, 1, location) { }

        /// <summary>
        /// Create stack of one item at container location
        /// </summary>
        /// <param name="itemKey">The item</param>
        /// <param name="location">Container location</param>
        public ItemStack(string itemKey, Vector2 location) : this(itemKey, 1, location) { }


        /// <summary>
        /// Create stack of N items at container location
        /// </summary>
        /// <param name="item">The item</param>
        /// <param name="stackSize">Stack size</param>
        /// <param name="location">Container location</param>
        public ItemStack(Item item, int stackSize, Vector2 location) : this(item.Key, stackSize, location) { }

        /// <summary>
        /// Create stack of N items at container location
        /// </summary>
        /// <param name="itemKey">The item</param>
        /// <param name="stackSize">Stack size</param>
        /// <param name="location">Container location</param>
        public ItemStack(string itemKey, int stackSize, Vector2 location)
        {
            ItemKey = itemKey;
            StackSize = stackSize;
            LocationInContainer = location;
        }

        /// <summary>
        /// Whether the stack will be full by adding given amount of items to it
        /// </summary>
        /// <param name="num">The number of items added</param>
        /// <returns>True if full; otherwise false</returns>
        public bool WillStackBeFull(int num) => Item != null && Item.MaxStackSize >= StackSize + num;

        /// <summary>
        /// Add item(s) to stack
        /// </summary>
        /// <param name="num">The number of items to add</param>
        /// <returns>State of the stack after adding</returns>
        public ItemStackState AddToStack(int num = 1)
        {
            StackSize += num;
            return IsStackFull ? ItemStackState.Full : ItemStackState.Normal;
        }

        /// <summary>
        /// Remove item(s) from stack
        /// </summary>
        /// <param name="num">The number of items to remove</param>
        /// <returns>State of the stack after removing</returns>
        public ItemStackState RemoveFromStack(int num = 1)
        {
            StackSize -= num;
            return StackSize <= 0 ? ItemStackState.Empty : ItemStackState.Normal;
        }

        public object Clone() => new ItemStack(Item, StackSize, LocationInContainer);
    }
}
