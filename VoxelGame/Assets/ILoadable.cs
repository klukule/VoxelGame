using Ionic.Zip;
using System;

namespace VoxelGame.Assets
{
    /// <summary>
    /// Loadable interface, implemented in any asset that can be loaded from disk
    /// </summary>
    public interface ILoadable
    {
        /// <summary>
        /// Import/Load asset from the package (or from filesystem as a backup for some assets)
        /// </summary>
        /// <param name="path">Path to load from</param>
        /// <param name="pack">Package to load from</param>
        /// <returns>Imported asset</returns>
        ILoadable Load(string path, ZipFile pack);

        /// <summary>
        /// Disposes of any resources acquired by this asset
        /// </summary>
        [Obsolete("This should only be called by the Asset System! Are you sure you want to dispose of this object?")] // Handy warning to avoid stupidity :)
        void Dispose();
    }
}
