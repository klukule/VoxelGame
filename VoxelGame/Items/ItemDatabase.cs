using System.Collections.Generic;
using System.Linq;

namespace VoxelGame.Items
{
    /// <summary>
    /// Item database
    /// </summary>
    public static class ItemDatabase
    {
        // Item cache
        private static readonly Dictionary<string, Item> _items = new Dictionary<string, Item>();

        /// <summary>
        /// Returns ordered list of ingame items
        /// </summary>
        /// <returns></returns>
        public static List<Item> GetItems() => _items.Values.ToList().OrderBy(x => x.Key).ToList();

        /// <summary>
        /// Registers new item
        /// </summary>
        /// <param name="item">The item</param>
        public static void RegisterItem(Item item)
        {
            if (_items.ContainsKey(item.Key))
            {
                Debug.Log("Item with key: " + item.Key + " already exists! Cancelling this addition", DebugLevel.Warning);
                return;
            }
            item.ID = 1 + _items.Count;
            _items.Add(item.Key, item);
        }

        /// <summary>
        /// Gets item by it's unique key
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>Item instance if found; otherwise null</returns>
        public static Item GetItem(string key)
        {
            if (key == null) return null;

            if (_items.TryGetValue(key, out Item item))
                return item;

            return null;
        }

        /// <summary>
        /// Gets item by it's unique id
        /// </summary>
        /// <param name="key">The id</param>
        /// <returns>Item instance if found; otherwise null</returns>
        public static Item GetItem(int id) => _items.Values.FirstOrDefault(x => x.ID == id);

        /// <summary>
        /// Updates the item for given key
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="item">The item</param>
        public static void SetItem(string key, Item item)
        {
            if (_items.ContainsKey(key))
                _items[key] = item;
        }
    }
}
