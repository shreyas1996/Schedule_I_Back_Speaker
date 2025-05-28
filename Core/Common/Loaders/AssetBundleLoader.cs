using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using MelonLoader;
using System.IO;
using System.Reflection;
using Il2CppInterop.Runtime.InteropTypes;

namespace BackSpeakerMod.Core.Common.Loaders
{
    /// <summary>
    /// Reusable asset bundle loading helper
    /// Supports both file-based and embedded resource loading
    /// </summary>
    public static class AssetBundleLoader
    {
        /// <summary>
        /// Load asset bundle from file path
        /// </summary>
        public static Il2CppAssetBundle LoadFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    LoggingSystem.Warning($"Asset bundle file not found: {filePath}", "AssetLoader");
                    return null;
                }

                LoggingSystem.Info($"Loading asset bundle from file: {filePath}", "AssetLoader");
                var bundle = Il2CppAssetBundleManager.LoadFromFile(filePath);

                if (bundle == null)
                {
                    LoggingSystem.Error($"Failed to load asset bundle from: {filePath}", "AssetLoader");
                    return null;
                }

                LoggingSystem.Info($"Successfully loaded asset bundle from file", "AssetLoader");
                return bundle;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Exception loading asset bundle from file: {ex.Message}", "AssetLoader");
                return null;
            }
        }

        /// <summary>
        /// Load asset bundle from embedded resource
        /// </summary>
        public static Il2CppAssetBundle LoadFromEmbeddedResource(string resourceName, Assembly assembly = null)
        {
            try
            {
                assembly ??= Assembly.GetExecutingAssembly();
                
                // Try multiple possible resource name formats
                string[] possibleNames = {
                    resourceName,
                    $"BackSpeakerMod.EmbeddedResources.{resourceName}",
                    $"EmbeddedResources.{resourceName}",
                    $"BackSpeakerMod.{resourceName}"
                };
                
                LoggingSystem.Info($"Loading asset bundle from embedded resource: {resourceName}", "AssetLoader");
                LoggingSystem.Debug("Available embedded resources:", "AssetLoader");
                
                // Log all available resources for debugging
                var availableResources = assembly.GetManifestResourceNames();
                foreach (var res in availableResources)
                {
                    LoggingSystem.Debug($"  - {res}", "AssetLoader");
                }

                global::System.IO.Stream stream = null;
                string actualResourceName = null;
                
                // Try each possible name format
                foreach (var name in possibleNames)
                {
                    stream = assembly.GetManifestResourceStream(name);
                    if (stream != null)
                    {
                        actualResourceName = name;
                        break;
                    }
                }

                if (stream == null)
                {
                    LoggingSystem.Error($"Embedded resource not found. Tried: {string.Join(", ", possibleNames)}", "AssetLoader");
                    return null;
                }

                LoggingSystem.Info($"Found embedded resource: {actualResourceName}", "AssetLoader");

                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                stream.Dispose();

                LoggingSystem.Info($"Loaded {buffer.Length} bytes from embedded resource", "AssetLoader");

                var bundle = Il2CppAssetBundleManager.LoadFromMemory(buffer);
                if (bundle == null)
                {
                    LoggingSystem.Error($"Failed to load asset bundle from embedded resource: {actualResourceName}", "AssetLoader");
                    return null;
                }

                LoggingSystem.Info($"Successfully loaded embedded asset bundle from {actualResourceName}", "AssetLoader");
                return bundle;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Exception loading asset bundle from embedded resource: {ex.Message}", "AssetLoader");
                return null;
            }
        }

        /// <summary>
        /// Load specific asset from bundle
        /// </summary>
        public static T LoadAsset<T>(Il2CppAssetBundle bundle, string assetName) where T : UnityEngine.Object
        {
            try
            {
                if (bundle == null)
                {
                    LoggingSystem.Warning($"Cannot load asset '{assetName}' - bundle is null", "AssetLoader");
                    return null;
                }

                LoggingSystem.Debug($"Loading asset '{assetName}' of type {typeof(T).Name}", "AssetLoader");
                
                var asset = bundle.LoadAsset<T>(assetName);
                if (asset == null)
                {
                    LoggingSystem.Warning($"Asset '{assetName}' not found in bundle", "AssetLoader");
                    LogBundleContents(bundle);
                    return null;
                }

                LoggingSystem.Debug($"Successfully loaded asset: {assetName}", "AssetLoader");
                return asset;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Exception loading asset '{assetName}': {ex.Message}", "AssetLoader");
                return null;
            }
        }

        /// <summary>
        /// Load first asset of type from bundle
        /// </summary>
        public static T LoadFirstAsset<T>(Il2CppAssetBundle bundle) where T : UnityEngine.Object
        {
            try
            {
                if (bundle == null)
                {
                    LoggingSystem.Warning($"Cannot load first asset of type {typeof(T).Name} - bundle is null", "AssetLoader");
                    return null;
                }

                var allAssets = bundle.LoadAllAssets<T>();
                if (allAssets == null || allAssets.Length == 0)
                {
                    LoggingSystem.Warning($"No assets of type {typeof(T).Name} found in bundle", "AssetLoader");
                    return null;
                }

                var asset = allAssets[0];
                LoggingSystem.Debug($"Loaded first asset of type {typeof(T).Name}: {asset.name}", "AssetLoader");
                return asset;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Exception loading first asset of type {typeof(T).Name}: {ex.Message}", "AssetLoader");
                return null;
            }
        }

        /// <summary>
        /// Safely unload asset bundle
        /// </summary>
        public static void UnloadBundle(Il2CppAssetBundle bundle, bool unloadAllLoadedObjects = false)
        {
            try
            {
                if (bundle == null) return;

                LoggingSystem.Info($"Unloading asset bundle", "AssetLoader");
                bundle.Unload(unloadAllLoadedObjects);
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Exception unloading asset bundle: {ex.Message}", "AssetLoader");
            }
        }

        /// <summary>
        /// Log available embedded resources for debugging
        /// </summary>
        public static void LogAvailableResources(Assembly assembly = null)
        {
            try
            {
                assembly ??= Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames();
                
                LoggingSystem.Debug($"Available embedded resources ({resourceNames.Length}):", "AssetLoader");
                foreach (var name in resourceNames)
                {
                    LoggingSystem.Debug($"  - {name}", "AssetLoader");
                }
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Exception listing embedded resources: {ex.Message}", "AssetLoader");
            }
        }

        /// <summary>
        /// Log contents of asset bundle for debugging
        /// </summary>
        public static void LogBundleContents(Il2CppAssetBundle bundle)
        {
            try
            {
                if (bundle == null)
                {
                    LoggingSystem.Debug("Cannot log bundle contents - bundle is null", "AssetLoader");
                    return;
                }

                var allAssets = bundle.LoadAllAssets();
                LoggingSystem.Debug($"Bundle contains {allAssets.Length} assets:", "AssetLoader");
                
                foreach (var asset in allAssets)
                {
                    LoggingSystem.Debug($"  - {asset.name} ({asset.GetType().Name})", "AssetLoader");
                }
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Exception logging bundle contents: {ex.Message}", "AssetLoader");
            }
        }

        /// <summary>
        /// Check if asset bundle contains specific asset
        /// </summary>
        public static bool ContainsAsset(Il2CppAssetBundle bundle, string assetName)
        {
            try
            {
                if (bundle == null) return false;

                var allAssets = bundle.LoadAllAssets();
                foreach (var asset in allAssets)
                {
                    if (asset.name == assetName)
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Exception checking for asset '{assetName}': {ex.Message}", "AssetLoader");
                return false;
            }
        }
    }
} 