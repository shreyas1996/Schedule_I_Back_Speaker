#if IL2CPP
using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;
using BackSpeakerMod.NewBackend.Utils;
using System.Linq;

namespace BackSpeakerMod.S1Wrapper.Il2Cpp
{
    /// <summary>
    /// IL2CPP-specific asset bundle wrapper using Il2CppAssetBundleManager and Il2CppAssetBundle
    /// </summary>
    public class Il2CppAssetBundleWrapper : IAssetBundle
    {
        private readonly Il2CppAssetBundle bundle; // This will be Il2CppAssetBundle
        private readonly bool isValid;

        public Il2CppAssetBundleWrapper(Il2CppAssetBundle il2cppAssetBundle)
        {
            bundle = il2cppAssetBundle;
            isValid = bundle != null;
            
            if (isValid)
            {
                NewLoggingSystem.Debug($"Created Il2CppAssetBundleWrapper with bundle type: {bundle.GetType().FullName}", "Il2CppAssetBundleWrapper");
            }
            else
            {
                NewLoggingSystem.Error("Il2CppAssetBundleWrapper created with null bundle", "Il2CppAssetBundleWrapper");
            }
        }

        public bool IsValid => isValid;

        public T[]? LoadAllAssets<T>() where T : UnityEngine.Object
        {
            NewLoggingSystem.Debug($"Loading all assets of type {typeof(T).Name} from Il2CppAssetBundle", "Il2CppAssetBundleWrapper");
            try {
                var bundleAssets = bundle.LoadAllAssets<T>();
                NewLoggingSystem.Debug($"Loaded {bundleAssets.Length} assets of type {typeof(T).Name} from Il2CppAssetBundle", "Il2CppAssetBundleWrapper");
                return bundleAssets;
            } catch (System.Exception ex) {
                NewLoggingSystem.Error($"Error loading all assets of type {typeof(T).Name} from Il2CppAssetBundle: {ex.Message}", "Il2CppAssetBundleWrapper");
                NewLoggingSystem.Debug($"Exception details: {ex}", "Il2CppAssetBundleWrapper");
                return null;
            }
        }

        public T? LoadAsset<T>(string name) where T : UnityEngine.Object
        {
            NewLoggingSystem.Debug($"Loading asset {name} of type {typeof(T).Name} from Il2CppAssetBundle", "Il2CppAssetBundleWrapper");
            try {
                var asset = bundle.LoadAsset<T>(name);
                NewLoggingSystem.Debug($"Loaded asset {name} of type {typeof(T).Name} from Il2CppAssetBundle", "Il2CppAssetBundleWrapper");
                return asset;
            } catch (System.Exception ex) {
                NewLoggingSystem.Error($"Error loading asset {name} of type {typeof(T).Name} from Il2CppAssetBundle: {ex.Message}", "Il2CppAssetBundleWrapper");
                NewLoggingSystem.Debug($"Exception details: {ex}", "Il2CppAssetBundleWrapper");
                return null;
            }
        }

        public string[] GetAllAssetNames()
        {
            NewLoggingSystem.Debug($"Getting all asset names from Il2CppAssetBundle type: {bundle?.GetType().Name}", "Il2CppAssetBundleWrapper");
            
            if (!isValid)
            {
                NewLoggingSystem.Error("Cannot get asset names: bundle is invalid", "Il2CppAssetBundleWrapper");
                return new string[0];
            }

            try {
                var bundleAssets = bundle.GetAllAssetNames();
                NewLoggingSystem.Debug($"Loaded {bundleAssets.Length} assets from Il2CppAssetBundle", "Il2CppAssetBundleWrapper");
                return bundleAssets;
            } catch (System.Exception ex) {
                NewLoggingSystem.Error($"Error getting all asset names from Il2CppAssetBundle: {ex.Message}", "Il2CppAssetBundleWrapper");
                NewLoggingSystem.Debug($"Exception details: {ex}", "Il2CppAssetBundleWrapper");
                return new string[0];
            }
        }

        public void Unload(bool unloadAllLoadedObjects)
        {
            NewLoggingSystem.Debug($"Unloading Il2CppAssetBundle (unloadAllLoadedObjects: {unloadAllLoadedObjects})", "Il2CppAssetBundleWrapper");
            
            if (!isValid)
            {
                NewLoggingSystem.Debug("Cannot unload: bundle is invalid", "Il2CppAssetBundleWrapper");
                return;
            }

            try {
                bundle.Unload(unloadAllLoadedObjects);
                NewLoggingSystem.Debug($"Unloaded Il2CppAssetBundle (unloadAllLoadedObjects: {unloadAllLoadedObjects})", "Il2CppAssetBundleWrapper");
            } catch (System.Exception ex) {
                NewLoggingSystem.Error($"Error unloading Il2CppAssetBundle (unloadAllLoadedObjects: {unloadAllLoadedObjects}): {ex.Message}", "Il2CppAssetBundleWrapper");
                NewLoggingSystem.Debug($"Exception details: {ex}", "Il2CppAssetBundleWrapper");
            }
        }
    }
}
#endif