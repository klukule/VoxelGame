using OpenTK;
using VoxelGame.Entities;
using VoxelGame.Items;
using VoxelGame.Worlds;

namespace VoxelGame.Blocks
{
    /// <summary>
    /// Stone block
    /// </summary>
    public class StoneBlock : Block
    {
        public override string Key => "Block_Stone";

        public override void OnBreak(Vector3 WorldPosition, Vector2 ChunkPosition)
        {
            World.Instance.AddEntity(new ItemEntity(GameItems.STONE) { Position = WorldPosition + new Vector3(.5f, 0, .5f) });
        }
    }
}
