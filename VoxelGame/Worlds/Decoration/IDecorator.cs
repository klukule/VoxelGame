namespace VoxelGame.Worlds.Decoration
{
    /// <summary>
    /// Decorator interface
    /// </summary>
    public interface IDecorator
    {
        /// <summary>
        /// Spawn decoration at location
        /// </summary>
        /// <param name="chunk">Chunk to spawn in</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <param name="z">Z coordinate</param>
        void DecorateAtBlock(Chunk chunk, int x, int y, int z);
    }
}
