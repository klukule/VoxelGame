using OpenTK;
using VoxelGame.Entities;
using VoxelGame.Items;
using VoxelGame.Worlds;

namespace VoxelGame.Blocks
{
    /// <summary>
    /// Dirt block
    /// </summary>
    public class DirtBlock : Block
    {
        public override string Key => "Block_Dirt";

        public override void OnBreak(Vector3 WorldPosition, Vector2 ChunkPosition)
        {
            World.Instance.AddEntity(new ItemEntity(GameItems.DIRT) { Position = WorldPosition + new Vector3(.5f, 0, .5f) });
        }
    }
}
