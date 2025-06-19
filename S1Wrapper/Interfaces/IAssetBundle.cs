using UnityEngine;

namespace BackSpeakerMod.S1Wrapper.Interfaces
{
    /// <summary>
    /// Interface for Schedule One AssetBundle functionality
    /// Provides unified access to both IL2CPP and Mono AssetBundle systems
    /// </summary>
    public interface IAssetBundle
    {
        /// <summary>
        /// Whether the asset bundle is valid and can be used
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// Load a specific asset by name and type
        /// </summary>
        T? LoadAsset<T>(string name) where T : UnityEngine.Object;

        /// <summary>
        /// Load all assets of a specific type from the bundle
        /// </summary>
        T[]? LoadAllAssets<T>() where T : UnityEngine.Object;

        /// <summary>
        /// Get all asset names in this bundle
        /// </summary>
        string[] GetAllAssetNames();

        /// <summary>
        /// Unload the asset bundle
        /// </summary>
        void Unload(bool unloadAllLoadedObjects);
    }
}
