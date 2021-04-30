using System.Collections.Generic;
using System.Linq;

namespace VoxelGame.Blocks
{
    /// <summary>
    /// Block database implementation
    /// </summary>
    public static class BlockDatabase
    {
        // TODO: Optimize, not effective for accessing via numerical ID (during lightmap lookup) has O(n) - use separate list with numerical IDs for that, it has O(1) access
        // Block registry
        private static Dictionary<string, Block> _blocks = new Dictionary<string, Block>();

        /// <summary>
        /// Registers the block and assigns it numerical ID
        /// </summary>
        /// <param name="block">The block to register</param>
        public static void RegisterBlock(Block block)
        {
            if (_blocks.ContainsKey(block.Key))
            {
                Debug.Log("Block with key: " + block.Key + " already exists! Cancelling this addition", DebugLevel.Warning);
                return;
            }
            block.ID = 1 + _blocks.Count;
            _blocks.Add(block.Key, block);
        }

        /// <summary>
        /// Gets the block by unique id
        /// </summary>
        /// <param name="key">The block id</param>
        /// <returns>Block instance or null if not found</returns>
        public static Block GetBlock(string key)
        {
            if (_blocks.TryGetValue(key, out Block block))
                return block;

            return null;
        }

        /// <summary>
        /// Gets teh block by numerical id
        /// </summary>
        /// <param name="id">The id</param>
        /// <returns>Block instance or null if not found</returns>
        public static Block GetBlock(int id) => _blocks.Values.FirstOrDefault(x => x.ID == id);

        /// <summary>
        /// Updates existing registered block instance based on the unique id
        /// </summary>
        /// <param name="key">The id</param>
        /// <param name="block">The block</param>
        public static void SetBlock(string key, Block block)
        {
            if (_blocks.ContainsKey(key))
                _blocks[key] = block;
        }
    }
}
