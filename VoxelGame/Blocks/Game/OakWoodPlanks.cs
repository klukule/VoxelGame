using OpenTK;
using VoxelGame.Entities;
using VoxelGame.Items;
using VoxelGame.Worlds;

namespace VoxelGame.Blocks
{
    /// <summary>
    /// Oak wood plangs
    /// </summary>
    public class OakWoodPlanks : Block
    {
        public override string Key => "Block_Oak_Wood_Planks";

        public override void OnBreak(Vector3 WorldPosition, Vector2 ChunkPosition)
        {
            World.Instance.AddEntity(new ItemEntity(GameItems.PLANKS_OAK) { Position = WorldPosition + new Vector3(.5f, 0, .5f) });
        }
    }
}
