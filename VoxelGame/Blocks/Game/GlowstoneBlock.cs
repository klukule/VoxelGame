using OpenTK;
using OpenTK.Graphics;
using VoxelGame.Entities;
using VoxelGame.Items;
using VoxelGame.Worlds;

namespace VoxelGame.Blocks
{
    /// <summary>
    /// Glowstone block
    /// </summary>
    public class GlowstoneBlock : Block
    {
        public override string Key => "Block_Glowstone";

        public override Color4 Emission => new Color4(255, 255, 0, 255); // Yellow - intensity/range 16
        public override bool IsEmissive => true;

        public override void OnBreak(Vector3 WorldPosition, Vector2 ChunkPosition)
        {
            World.Instance.AddEntity(new ItemEntity(GameItems.GLOWSTONE) { Position = WorldPosition + new Vector3(.5f, 0, .5f) });
        }
    }
}
