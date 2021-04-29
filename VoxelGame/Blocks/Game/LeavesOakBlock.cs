using OpenTK;

namespace VoxelGame.Blocks
{
    /// <summary>
    /// Oak leaf block
    /// </summary>
    public class LeavesOakBlock : LeavesBlock
    {
        public override string Key => "Block_Leaves_Oak";

        public override void OnBreak(Vector3 WorldPosition, Vector2 ChunkPosition)
        {
            // TODO: Drop sapling
        }
    }
}
