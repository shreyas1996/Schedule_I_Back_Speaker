#if !IL2CPP
using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;
using BackSpeakerMod.NewBackend.Utils;
using System.Linq;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    /// <summary>
    /// Mono-specific asset bundle wrapper using standard AssetBundle and AssetBundleModule
    /// </summary>
    public class MonoAssetBundleWrapper : IAssetBundle
    {
        private readonly AssetBundle bundle; // Standard Unity AssetBundle
        private readonly bool isValid;

        public MonoAssetBundleWrapper(AssetBundle assetBundle)
        {
            bundle = assetBundle;
            isValid = bundle != null;
            
            if (isValid)
            {
                NewLoggingSystem.Debug($"Created MonoAssetBundleWrapper with bundle: {bundle.name}", "MonoAssetBundleWrapper");
            }
            else
            {
                NewLoggingSystem.Error("Created MonoAssetBundleWrapper with null bundle", "MonoAssetBundleWrapper");
            }
        }

        public bool IsValid => isValid;

        public T[]? LoadAllAssets<T>() where T : UnityEngine.Object
        {
            NewLoggingSystem.Debug($"Loading all assets of type {typeof(T).Name} from standard AssetBundle", "MonoAssetBundleWrapper");
            
            if (!isValid)
            {
                NewLoggingSystem.Error("Cannot load assets: bundle is invalid", "MonoAssetBundleWrapper");
                return null;
            }

            try
            {
                // First, let's try to get all asset names to see what's in the bundle
                var allNames = GetAllAssetNames();
                NewLoggingSystem.Debug($"Standard AssetBundle contains {allNames.Length} assets: [{string.Join(", ", allNames)}]", "MonoAssetBundleWrapper");
                
                // Use direct call to LoadAllAssets on standard AssetBundle
                var assets = bundle.LoadAllAssets<T>();
                NewLoggingSystem.Debug($"LoadAllAssets returned {assets.Length} assets of type {typeof(T).Name}", "MonoAssetBundleWrapper");
                return assets;
            }
            catch (System.Exception ex)
            {
                NewLoggingSystem.Error($"Error loading assets from standard AssetBundle: {ex.Message}", "MonoAssetBundleWrapper");
                NewLoggingSystem.Debug($"Exception details: {ex}", "MonoAssetBundleWrapper");
                return null;
            }
        }

        public T? LoadAsset<T>(string name) where T : UnityEngine.Object
        {
            NewLoggingSystem.Debug($"Loading asset '{name}' of type {typeof(T).Name} from standard AssetBundle", "MonoAssetBundleWrapper");
            
            if (!isValid)
            {
                NewLoggingSystem.Error("Cannot load asset: bundle is invalid", "MonoAssetBundleWrapper");
                return null;
            }

            try
            {
                // Use direct call to LoadAsset on standard AssetBundle
                var asset = bundle.LoadAsset<T>(name);
                if (asset != null)
                {
                    NewLoggingSystem.Debug($"Successfully loaded asset '{name}' of type {typeof(T).Name}", "MonoAssetBundleWrapper");
                }
                else
                {
                    NewLoggingSystem.Debug($"Asset '{name}' not found or is not of type {typeof(T).Name}", "MonoAssetBundleWrapper");
                }
                return asset;
            }
            catch (System.Exception ex)
            {
                NewLoggingSystem.Error($"Error loading asset '{name}' from standard AssetBundle: {ex.Message}", "MonoAssetBundleWrapper");
                NewLoggingSystem.Debug($"Exception details: {ex}", "MonoAssetBundleWrapper");
                return null;
            }
        }

        public string[] GetAllAssetNames()
        {
            NewLoggingSystem.Debug("Getting all asset names from standard AssetBundle", "MonoAssetBundleWrapper");
            
            if (!isValid)
            {
                NewLoggingSystem.Error("Cannot get asset names: bundle is invalid", "MonoAssetBundleWrapper");
                return new string[0];
            }

            try
            {
                // Use direct call to GetAllAssetNames on standard AssetBundle
                var names = bundle.GetAllAssetNames();
                NewLoggingSystem.Debug($"GetAllAssetNames returned {names.Length} asset names", "MonoAssetBundleWrapper");
                return names;
            }
            catch (System.Exception ex)
            {
                NewLoggingSystem.Error($"Error getting asset names from standard AssetBundle: {ex.Message}", "MonoAssetBundleWrapper");
                NewLoggingSystem.Debug($"Exception details: {ex}", "MonoAssetBundleWrapper");
                return new string[0];
            }
        }

        public void Unload(bool unloadAllLoadedObjects)
        {
            NewLoggingSystem.Debug($"Unloading standard AssetBundle (unloadAllLoadedObjects: {unloadAllLoadedObjects})", "MonoAssetBundleWrapper");
            
            if (!isValid)
            {
                NewLoggingSystem.Error("Cannot unload: bundle is invalid", "MonoAssetBundleWrapper");
                return;
            }

            try
            {
                // Use direct call to Unload on standard AssetBundle
                bundle.Unload(unloadAllLoadedObjects);
                NewLoggingSystem.Debug("Successfully unloaded standard AssetBundle", "MonoAssetBundleWrapper");
            }
            catch (System.Exception ex)
            {
                NewLoggingSystem.Error($"Error unloading standard AssetBundle: {ex.Message}", "MonoAssetBundleWrapper");
                NewLoggingSystem.Debug($"Exception details: {ex}", "MonoAssetBundleWrapper");
            }
        }
    }
}
#endif
