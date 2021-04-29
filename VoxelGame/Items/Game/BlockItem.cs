using OpenTK;
using VoxelGame.Blocks;
using VoxelGame.Worlds;

namespace VoxelGame.Items
{
    /// <summary>
    /// Base for any item based on the block
    /// </summary>
    public class BlockItem : Item
    {
        /// <summary>
        /// Corresponding block for this item
        /// </summary>
        public Block Block { get; }
        public override string Key => $"Item_{Name}";

        /// <summary>
        /// Create blck item instance
        /// </summary>
        /// <param name="block">The block</param>
        public BlockItem(Block block)
        {
            Block = block;
            Name = block.Key;
            // TODO: Override flat pixel mesh generation with cube for blocks
            GenerateGraphics();
            ItemDatabase.RegisterItem(this);
        }


        public override void OnInteract(Vector3 position, Chunk chunk)
        {
            // Standard behavior of block item is to place the block
            chunk.PlaceBlock((int)position.X, (int)position.Y,
                (int)position.Z, Block, true);
        }
    }
}
