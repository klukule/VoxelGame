using OpenTK;
using VoxelGame.Entities;
using VoxelGame.Items;
using VoxelGame.Worlds;

namespace VoxelGame.Blocks
{
    /// <summary>
    /// Sand block
    /// </summary>
    public class SandBlock : Block
    {
        public override string Key => "Block_Sand";

        public override void OnBreak(Vector3 WorldPosition, Vector2 ChunkPosition)
        {
            World.Instance.AddEntity(new ItemEntity(GameItems.SAND) { Position = WorldPosition + new Vector3(.5f, 0, .5f) });
        }
    }
}
