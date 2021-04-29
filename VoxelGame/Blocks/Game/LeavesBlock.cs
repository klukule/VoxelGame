using OpenTK.Graphics;

namespace VoxelGame.Blocks
{
    /// <summary>
    /// Leaf block
    /// </summary>
    public class LeavesBlock : Block
    {
        public override bool IsTransparent => true;
        public override sbyte Opacity => 6;

        public override BlockColorDelegate BlockColor => (x, y, z) =>
        {
            var biome = 1;
            return new Color4(0.25f * biome, 0.75f * biome, 0.16f * biome, 1);
        };
    }
}
