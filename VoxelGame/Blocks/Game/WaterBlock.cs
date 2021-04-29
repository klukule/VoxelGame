using OpenTK.Graphics;
using VoxelGame.Physics;

namespace VoxelGame.Blocks
{
    /// <summary>
    /// Water block
    /// </summary>
    public class WaterBlock : Block
    {
        public override string Key => "Block_Water";
        public override bool IsTransparent => true;
        public override bool TransparencyCullsSelf => true;
        public override BlockColorDelegate BlockColor { get; set; } = (x, y, z) => Color4.Blue;
        public override BoundingBox CollisionShape => null; // Water has no collision - cannot be broken
    }
}
