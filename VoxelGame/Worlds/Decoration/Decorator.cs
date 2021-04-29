using System;

namespace VoxelGame.Worlds.Decoration
{
    /// <summary>
    /// Base decorator class
    /// </summary>
    public abstract class Decorator : IDisposable, IDecorator
    {
        public abstract void Dispose();
        public abstract void DecorateAtBlock(Chunk chunk, int x, int y, int z);
    }
}
