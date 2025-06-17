using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using System.IO;
using System.Reflection;
using BackSpeakerMod.S1Wrapper;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.Core.Features.Headphones.Loading
{
    /// <summary>
    /// Simple, reliable headphone asset loader
    /// Loads embedded resource → temp file → persistent bundle → instantiate
    /// </summary>
    public class HeadphoneAssetLoader
    {
        private readonly HeadphoneConfig config;
        private IAssetBundle? persistentBundle = null;
        private GameObject? headphonePrefab = null;
        private string? tempBundlePath = null;
        private bool isLoaded = false;

        /// <summary>
        /// Whether headphone assets are loaded and ready
        /// </summary>
        public bool IsLoaded
        {
            get
            {
                // Validate that we have everything we need
                bool hasValidBundle = persistentBundle != null && persistentBundle != null;
                bool hasValidPrefab = headphonePrefab != null && headphonePrefab != null;
                
                // If bundle exists but prefab was garbage collected, try to reload
                if (isLoaded && hasValidBundle && !hasValidPrefab)
                {
                    LoggingSystem.Warning("Prefab was garbage collected, attempting to reload from bundle", "Headphones");
                    if (ReloadPrefabFromBundle())
                    {
                        hasValidPrefab = headphonePrefab != null && headphonePrefab;
                        LoggingSystem.Info($"Prefab reloaded successfully: {(hasValidPrefab ? headphonePrefab!.name : "failed")}", "Headphones");
                    }
                    else
                    {
                        LoggingSystem.Error("Failed to reload prefab from bundle", "Headphones");
                        isLoaded = false;
                    }
                }
                
                return isLoaded && hasValidBundle && hasValidPrefab;
            }
        }

        /// <summary>
        /// Get the loaded headphone prefab
        /// </summary>
        public GameObject? HeadphonePrefab => headphonePrefab;

        /// <summary>
        /// Initialize with config
        /// </summary>
        public HeadphoneAssetLoader(HeadphoneConfig? headphoneConfig = null)
        {
            config = headphoneConfig ?? new HeadphoneConfig();
            LoggingSystem.Info("HeadphoneAssetLoader created", "Headphones");
        }

        /// <summary>
        /// Load headphones using simple approach: embedded → temp file → bundle → prefab
        /// </summary>
        public bool LoadFromEmbeddedResource()
        {
            if (!FeatureFlags.Headphones.Enabled)
            {
                LoggingSystem.Info("Headphones feature is disabled", "Headphones");
                return false;
            }

            if (isLoaded)
            {
                LoggingSystem.Info("Headphones already loaded", "Headphones");
                return true;
            }

            LoggingSystem.Info($"Loading headphones from embedded resource: {config.EmbeddedResourceName}", "Headphones");

            try
            {
                // Step 1: Extract embedded resource to temp file
                if (!ExtractEmbeddedResourceToTempFile())
                {
                    LoggingSystem.Error("Failed to extract embedded resource to temp file", "Headphones");
                    return false;
                }

                // Step 2: Load bundle from temp file
                if (!LoadBundleFromTempFile())
                {
                    LoggingSystem.Error("Failed to load bundle from temp file", "Headphones");
                    return false;
                }

                // Step 3: Load prefab from bundle
                if (!LoadPrefabFromBundle())
                {
                    LoggingSystem.Error("Failed to load prefab from bundle", "Headphones");
                    return false;
                }

                isLoaded = true;
                LoggingSystem.Info($"✓ Headphones loaded successfully: {headphonePrefab!.name}", "Headphones");
                return true;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Exception loading headphones: {ex.Message}", "Headphones");
                Cleanup();
                return false;
            }
        }

        /// <summary>
        /// Step 1: Extract embedded resource to temp file
        /// </summary>
        private bool ExtractEmbeddedResourceToTempFile()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                if (assembly == null)
                {
                    throw new Exception("Failed to get executing assembly");
                }
                
                // Try different resource name formats
                string[] possibleNames = {
                    config.EmbeddedResourceName,
                    $"BackSpeakerMod.EmbeddedResources.{config.EmbeddedResourceName}",
                    $"EmbeddedResources.{config.EmbeddedResourceName}"
                };

                Stream? resourceStream = null;
                string? actualResourceName = null;

                foreach (var name in possibleNames)
                {
                    resourceStream = assembly.GetManifestResourceStream(name);
                    if (resourceStream != null)
                    {
                        actualResourceName = name;
                        break;
                    }
                }

                if (resourceStream == null || actualResourceName == null)
                {
                    LoggingSystem.Error($"Embedded resource not found: {config.EmbeddedResourceName}", "Headphones");
                    LogAvailableResources(assembly);
                    return false;
                }

                // Create temp file path
                var guid = global::System.Guid.NewGuid();
                tempBundlePath = Path.Combine(Path.GetTempPath(), $"headphones_{guid:N}.bundle");
                
                // Write resource to temp file
                using (var fileStream = File.Create(tempBundlePath!))
                {
                    resourceStream.CopyTo(fileStream);
                }
                
                resourceStream?.Close();
                resourceStream?.Dispose();

                LoggingSystem.Info($"✓ Extracted {actualResourceName} to temp file: {tempBundlePath}", "Headphones");
                return true;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to extract embedded resource: {ex.Message}", "Headphones");
                return false;
            }
        }

        /// <summary>
        /// Step 2: Load bundle from temp file
        /// </summary>
        private bool LoadBundleFromTempFile()
        {
            try
            {
                if (string.IsNullOrEmpty(tempBundlePath) || !File.Exists(tempBundlePath))
                {
                    LoggingSystem.Error("Temp bundle file does not exist", "Headphones");
                    return false;
                }

                persistentBundle = S1AssetBundleLoader.LoadFromFile(tempBundlePath!);
                
                if (persistentBundle == null)
                {
                    LoggingSystem.Error("Failed to load bundle from temp file", "Headphones");
                    return false;
                }

                LoggingSystem.Info($"✓ Bundle loaded from temp file", "Headphones");
                return true;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to load bundle from temp file: {ex.Message}", "Headphones");
                return false;
            }
        }

        /// <summary>
        /// Step 3: Load prefab from bundle
        /// </summary>
        private bool LoadPrefabFromBundle()
        {
            try
            {
                if (persistentBundle == null)
                {
                    LoggingSystem.Error("Cannot load prefab - bundle is null", "Headphones");
                    return false;
                }

                LoggingSystem.Debug($"Loading prefab from bundle. Config asset name: '{config.AssetName}'", "Headphones");

                // Try to load the specific asset name, or use the first GameObject
                if (!string.IsNullOrEmpty(config.AssetName))
                {
                    LoggingSystem.Debug($"Attempting to load specific asset: '{config.AssetName}'", "Headphones");
                    headphonePrefab = persistentBundle.LoadAsset<GameObject>(config.AssetName!);
                    if (headphonePrefab != null)
                    {
                        LoggingSystem.Info($"✓ Successfully loaded specific asset: {headphonePrefab.name}", "Headphones");
                    }
                    else
                    {
                        LoggingSystem.Warning($"Specific asset '{config.AssetName}' not found in bundle", "Headphones");
                    }
                }

                // Fallback: load first GameObject from bundle
                if (headphonePrefab == null)
                {
                    LoggingSystem.Debug("Attempting to load first GameObject from bundle", "Headphones");
                    var allAssets = persistentBundle.LoadAllAssets<GameObject>() ?? Array.Empty<GameObject>();
                    LoggingSystem.Debug($"Found {(allAssets?.Length ?? 0)} GameObjects in bundle", "Headphones");
                    
                    if (allAssets != null && allAssets.Length > 0)
                    {
                        headphonePrefab = allAssets[0];
                        LoggingSystem.Info($"✓ Using first GameObject in bundle: {headphonePrefab.name}", "Headphones");
                    }
                    else
                    {
                        LoggingSystem.Warning("No GameObjects found in bundle", "Headphones");
                    }
                }

                if (headphonePrefab == null)
                {
                    LoggingSystem.Error("No headphone prefab found in bundle", "Headphones");
                    LogBundleContents();
                    return false;
                }

                LoggingSystem.Info($"✓ Prefab loaded: {headphonePrefab.name}", "Headphones");
                
                // Ensure the prefab stays referenced - it should persist as long as this loader exists
                // We don't need DontDestroyOnLoad on the prefab itself, only on instances that cross scenes
                LoggingSystem.Debug($"Prefab '{headphonePrefab.name}' loaded and referenced by loader", "Headphones");
                
                return true;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to load prefab from bundle: {ex.Message}", "Headphones");
                return false;
            }
        }

        /// <summary>
        /// Create headphone instance (simple instantiation)
        /// </summary>
        public GameObject? CreateInstance(bool persistAcrossScenes = false)
        {
            if (!IsLoaded)
            {
                LoggingSystem.Warning("Cannot create instance - headphones not loaded", "Headphones");
                return null;
            }

            try
            {
                var instance = UnityEngine.Object.Instantiate(headphonePrefab!);
                if (instance == null)
                {
                    throw new Exception("Failed to instantiate headphone prefab");
                }

                if (instance == null)
                {
                    throw new Exception("Failed to instantiate headphone prefab");
                }

                instance.name = "HeadphoneInstance";
                
                // Only mark as DontDestroyOnLoad if the instance needs to persist across scenes
                if (persistAcrossScenes)
                {
                    UnityEngine.Object.DontDestroyOnLoad(instance);
                    LoggingSystem.Debug($"Created persistent headphone instance: {instance.name}", "Headphones");
                }
                else
                {
                    LoggingSystem.Debug($"Created headphone instance: {instance.name}", "Headphones");
                }
                
                return instance;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to create headphone instance: {ex.Message}", "Headphones");
                return null;
            }
        }

        /// <summary>
        /// Create instance at specific position/rotation
        /// </summary>
        public GameObject? CreateInstance(Vector3 position, Quaternion rotation, bool persistAcrossScenes = false)
        {
            if (!IsLoaded)
            {
                LoggingSystem.Warning("Cannot create instance - headphones not loaded", "Headphones");
                return null;
            }

            try
            {
                var instance = UnityEngine.Object.Instantiate(headphonePrefab!, position, rotation);
                if (instance == null)
                {
                    throw new Exception("Failed to instantiate headphone prefab");
                }

                if (instance == null)
                {
                    throw new Exception("Failed to instantiate headphone prefab");
                }

                instance.name = "HeadphoneInstance";
                
                // Only mark as DontDestroyOnLoad if the instance needs to persist across scenes
                if (persistAcrossScenes)
                {
                    UnityEngine.Object.DontDestroyOnLoad(instance);
                    LoggingSystem.Debug($"Created persistent headphone instance at {position}: {instance.name}", "Headphones");
                }
                else
                {
                    LoggingSystem.Debug($"Created headphone instance at {position}: {instance.name}", "Headphones");
                }
                
                return instance;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to create headphone instance: {ex.Message}", "Headphones");
                return null;
            }
        }

        /// <summary>
        /// Get simple, clear status
        /// </summary>
        public string GetStatus()
        {
            if (!FeatureFlags.Headphones.Enabled)
                return "Disabled";
            
            if (IsLoaded)
                return $"Loaded: {headphonePrefab!.name}";
            
            return "Not loaded";
        }

        /// <summary>
        /// Get detailed status for debugging
        /// </summary>
        public string GetDetailedStatus()
        {
            return $"isLoaded: {isLoaded}, headphonePrefab: {(headphonePrefab != null ? $"'{headphonePrefab.name}'" : "null")}, bundle: {(persistentBundle != null ? "loaded" : "null")}, enabled: {FeatureFlags.Headphones.Enabled}";
        }

        /// <summary>
        /// Clean up resources
        /// </summary>
        public void Unload()
        {
            try
            {
                LoggingSystem.Info("Unloading headphone assets", "Headphones");
                
                if (persistentBundle != null)
                {
                    persistentBundle.Unload(true);
                    persistentBundle = null;
                }

                if (!string.IsNullOrEmpty(tempBundlePath) && File.Exists(tempBundlePath))
                {
                    File.Delete(tempBundlePath);
                    tempBundlePath = null;
                }

                headphonePrefab = null;
                isLoaded = false;
                
                LoggingSystem.Info("Headphone assets unloaded", "Headphones");
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Error during unload: {ex.Message}", "Headphones");
            }
        }

        /// <summary>
        /// Cleanup on failure
        /// </summary>
        private void Cleanup()
        {
            try
            {
                if (persistentBundle != null)
                {
                    persistentBundle.Unload(true);
                    persistentBundle = null;
                }

                if (!string.IsNullOrEmpty(tempBundlePath) && File.Exists(tempBundlePath))
                {
                    File.Delete(tempBundlePath);
                    tempBundlePath = null;
                }

                headphonePrefab = null;
                isLoaded = false;
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        /// <summary>
        /// Log available embedded resources for debugging
        /// </summary>
        private void LogAvailableResources(Assembly assembly)
        {
            try
            {
                var resourceNames = assembly.GetManifestResourceNames();
                LoggingSystem.Debug($"Available embedded resources ({resourceNames.Length}):", "Headphones");
                foreach (var name in resourceNames)
                {
                    LoggingSystem.Debug($"  - {name}", "Headphones");
                }
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Error listing resources: {ex.Message}", "Headphones");
            }
        }

        /// <summary>
        /// Log bundle contents for debugging
        /// </summary>
        private void LogBundleContents()
        {
            try
            {
                if (persistentBundle == null)
                {
                    LoggingSystem.Debug("Cannot log bundle contents - bundle is null", "Headphones");
                    return;
                }

                var allAssets = persistentBundle.LoadAllAssets();
                LoggingSystem.Debug($"Bundle contains {allAssets.Length} assets:", "Headphones");
                
                foreach (var asset in allAssets)
                {
                    LoggingSystem.Debug($"  - {asset.name} ({asset.GetType().Name})", "Headphones");
                }
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Error logging bundle contents: {ex.Message}", "Headphones");
            }
        }

        /// <summary>
        /// Reload prefab from bundle
        /// </summary>
        private bool ReloadPrefabFromBundle()
        {
            try
            {
                if (persistentBundle == null)
                {
                    LoggingSystem.Error("Cannot reload prefab - bundle is null", "Headphones");
                    return false;
                }

                LoggingSystem.Debug($"Reloading prefab from bundle. Config asset name: '{config.AssetName}'", "Headphones");

                // Try to load the specific asset name, or use the first GameObject
                if (!string.IsNullOrEmpty(config.AssetName))
                {
                    LoggingSystem.Debug($"Attempting to load specific asset: '{config.AssetName}'", "Headphones");
                    headphonePrefab = persistentBundle.LoadAsset<GameObject>(config.AssetName!);
                    if (headphonePrefab != null)
                    {
                        LoggingSystem.Info($"✓ Successfully reloaded specific asset: {headphonePrefab.name}", "Headphones");
                    }
                    else
                    {
                        LoggingSystem.Warning($"Specific asset '{config.AssetName}' not found in bundle", "Headphones");
                    }
                }

                // Fallback: load first GameObject from bundle
                if (headphonePrefab == null)
                {
                    LoggingSystem.Debug("Attempting to load first GameObject from bundle", "Headphones");
                    var allAssets = persistentBundle.LoadAllAssets<GameObject>() ?? Array.Empty<GameObject>();
                    LoggingSystem.Debug($"Found {(allAssets?.Length ?? 0)} GameObjects in bundle", "Headphones");
                    
                    if (allAssets != null && allAssets.Length > 0)
                    {
                        headphonePrefab = allAssets[0];
                        LoggingSystem.Info($"✓ Using first GameObject in bundle: {headphonePrefab.name}", "Headphones");
                    }
                    else
                    {
                        LoggingSystem.Warning("No GameObjects found in bundle", "Headphones");
                    }
                }

                if (headphonePrefab == null)
                {
                    LoggingSystem.Error("No headphone prefab found in bundle", "Headphones");
                    LogBundleContents();
                    return false;
                }

                LoggingSystem.Info($"✓ Prefab reloaded successfully: {headphonePrefab.name}", "Headphones");
                
                // Ensure the prefab stays referenced - it should persist as long as this loader exists
                // We don't need DontDestroyOnLoad on the prefab itself, only on instances that cross scenes
                LoggingSystem.Debug($"Prefab '{headphonePrefab.name}' loaded and referenced by loader", "Headphones");
                
                return true;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to reload prefab from bundle: {ex.Message}", "Headphones");
                return false;
            }
        }
    }
} 