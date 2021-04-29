using Ionic.Zip;
using System;
using System.Collections.Generic;

namespace VoxelGame.Assets
{
    /// <summary>
    /// Asset database implementation
    /// </summary>
    // TODO: Add support for multiple loaded packages (or at least two where one is override for the default)
    public static class AssetDatabase
    {
        /// <summary>
        /// Default database filename to load
        /// </summary>
        public const string DEFAULT_DATABASE = "Default";

        // Asset cache
        private static readonly Dictionary<string, ILoadable> _assets = new Dictionary<string, ILoadable>();
        private static ZipFile _loadedPackage;

        /// <summary>
        /// Currently loaded asset package
        /// </summary>
        public static ZipFile Package => _loadedPackage;

        /// <summary>
        /// Loads asset package
        /// </summary>
        /// <param name="file"></param>
        public static void Load(string file)
        {
            _loadedPackage = ZipFile.Read("Resources/" + file + ".pak");
        }

        /// <summary>
        /// Gets asset from specified path
        /// </summary>
        /// <typeparam name="T">Type of asset to load</typeparam>
        /// <param name="assetPath">Path to the asset</param>
        /// <param name="cache">Whether the asset should be cached after load</param>
        /// <returns>Loaded asset</returns>
        public static T GetAsset<T>(string assetPath, bool cache = true) where T : ILoadable
        {
            //Check cache
            if (_assets.TryGetValue(assetPath, out ILoadable asset))
                if (asset is T cast)
                    return cast; // If file could be casted to requested type return

            // Else try load the asset
            var importable = CreateAsset<T>(assetPath);
            if (cache)
                _assets.Add(assetPath, importable);

            return importable;
        }

        /// <summary>
        /// Checks whether the given asset is loaded and is of the requested type or not
        /// </summary>
        /// <typeparam name="T">Type of the asset</typeparam>
        /// <param name="path">Path to the asset</param>
        /// <param name="type">Requested type</param>
        /// <returns>True if asset is loaded and is of correct type; otherwise false</returns>
        public static bool ContainsAssetOfType<T>(string path, T type) where T : Type
        {
            if (!_assets.ContainsKey(path))
                return false;

            if (_assets[path].GetType() == type)
                return true;

            return false;
        }

        /// <summary>
        /// Registers new virtual asset
        /// </summary>
        /// <typeparam name="T">Type of the asset</typeparam>
        /// <param name="asset">Asset to register</param>
        /// <param name="path">Path to register at</param>
        /// <returns>True if successfully registered, else false</returns>
        public static bool RegisterAsset<T>(T asset, string path) where T : ILoadable
        {
            if (_assets.ContainsKey(path))
                return false;

            _assets.Add(path, asset);

            return true;
        }

        /// <summary>
        /// Creates new instance of the asset type and loads it from the package
        /// </summary>
        /// <typeparam name="T">Type of the asset</typeparam>
        /// <param name="path">Path to the asset</param>
        /// <returns>Instance of the asset</returns>
        public static T CreateAsset<T>(string path) where T : ILoadable
        {
            // Create importer instance - assets return final instance could not be the same as importer
            var importable = (ILoadable)Activator.CreateInstance<T>();
            return (T)importable.Load(path, _loadedPackage);
        }

        /// <summary>
        /// Disposes any loaded assets
        /// </summary>
        public static void Dispose()
        {
            foreach (var value in _assets.Values)
            {
#pragma warning disable 618
                value.Dispose();
#pragma warning restore 618
            }
        }
    }
}
