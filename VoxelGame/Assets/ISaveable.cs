namespace VoxelGame.Assets
{
    /// <summary>
    /// Saveable interface, implemented in any asset that can be saved back to disk
    /// </summary>
    public interface ISaveable
    {
        /// <summary>
        /// Path to save asset to
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Save asset to file
        /// </summary>
        void Save();
    }
}
