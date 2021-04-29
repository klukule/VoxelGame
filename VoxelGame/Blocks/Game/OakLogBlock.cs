using OpenTK;
using VoxelGame.Entities;
using VoxelGame.Items;
using VoxelGame.Worlds;

namespace VoxelGame.Blocks
{
    /// <summary>
    /// Oak log
    /// </summary>
    public class OakLogBlock : LogBlock
    {
        public override string Key => "Block_Log_Oak";

        public override void OnBreak(Vector3 WorldPosition, Vector2 ChunkPosition)
        {
            World.Instance.AddEntity(new ItemEntity(GameItems.LOG_OAK) { Position = WorldPosition + new Vector3(.5f, 0, .5f) });
        }
    }
}
