using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Common.Loaders;
using BackSpeakerMod.Core.Features.Headphones.Data;

namespace BackSpeakerMod.Core.Features.Headphones.Loading
{
    /// <summary>
    /// Specific asset loading for headphones
    /// </summary>
    public class HeadphoneAssetLoader
    {
        private readonly HeadphoneConfig config;
        private Il2CppAssetBundle loadedBundle;
        private GameObject headphonePrefab;
        private GameObject persistentHeadphonePrefab; // Persistent copy that won't get destroyed

        /// <summary>
        /// Whether headphone assets are loaded
        /// </summary>
        public bool IsLoaded => persistentHeadphonePrefab != null;

        /// <summary>
        /// Get the loaded headphone prefab (returns the persistent copy)
        /// </summary>
        public GameObject HeadphonePrefab => persistentHeadphonePrefab;

        /// <summary>
        /// Initialize headphone asset loader
        /// </summary>
        public HeadphoneAssetLoader(HeadphoneConfig headphoneConfig = null)
        {
            config = headphoneConfig ?? new HeadphoneConfig();
            LoggingSystem.Info("HeadphoneAssetLoader initialized", "Headphones");
        }

        /// <summary>
        /// Load headphones from embedded resource
        /// </summary>
        public bool LoadFromEmbeddedResource()
        {
            if (!FeatureFlags.Headphones.Enabled)
            {
                LoggingSystem.Warning("Headphones feature is disabled", "Headphones");
                return false;
            }

            if (IsLoaded)
            {
                LoggingSystem.Info("Headphones already loaded", "Headphones");
                return true;
            }

            LoggingSystem.Info("Loading headphones from embedded resource", "Headphones");

            // Load bundle from embedded resource
            loadedBundle = AssetBundleLoader.LoadFromEmbeddedResource(config.EmbeddedResourceName);
            if (loadedBundle == null)
            {
                LoggingSystem.Error("Failed to load headphone asset bundle from embedded resource", "Headphones");
                return false;
            }

            // Load headphone prefab
            return LoadPrefabFromBundle();
        }

        /// <summary>
        /// Load prefab from the loaded bundle
        /// </summary>
        private bool LoadPrefabFromBundle()
        {
            if (loadedBundle == null)
            {
                LoggingSystem.Error("Cannot load prefab - no bundle loaded", "Headphones");
                return false;
            }

            // Try to load by specific name first
            headphonePrefab = AssetBundleLoader.LoadAsset<GameObject>(loadedBundle, config.AssetName);
            
            // If not found, try to load first GameObject
            if (headphonePrefab == null)
            {
                LoggingSystem.Warning($"Asset '{config.AssetName}' not found, trying first GameObject", "Headphones");
                headphonePrefab = AssetBundleLoader.LoadFirstAsset<GameObject>(loadedBundle);
            }

            if (headphonePrefab == null)
            {
                LoggingSystem.Error("No suitable headphone prefab found in bundle", "Headphones");
                AssetBundleLoader.LogBundleContents(loadedBundle);
                UnloadAssets();
                return false;
            }

            LoggingSystem.Info($"Successfully loaded headphone prefab from bundle: {headphonePrefab.name}", "Headphones");
            
            // Simple approach: Keep the prefab reference and mark it as persistent
            // The key insight: DON'T aggressively unload the bundle!
            LoggingSystem.Debug("=== Preserving prefab reference with DontDestroyOnLoad ===", "Headphones");
            LoggingSystem.Debug($"Prefab: {headphonePrefab?.name ?? "null"}", "Headphones");
            
            try
            {
                // Keep the original prefab reference and mark it persistent
                persistentHeadphonePrefab = headphonePrefab;
                
                if (persistentHeadphonePrefab != null)
                {
                    // Mark the prefab as persistent so it survives scene changes
                    UnityEngine.Object.DontDestroyOnLoad(persistentHeadphonePrefab);
                    
                    LoggingSystem.Info($"Preserved prefab reference: {persistentHeadphonePrefab.name}", "Headphones");
                    LoggingSystem.Debug($"IsLoaded check: {IsLoaded}", "Headphones");
                }
                else
                {
                    LoggingSystem.Error("Failed to preserve prefab reference!", "Headphones");
                    return false;
                }
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to preserve prefab reference: {ex.Message}", "Headphones");
                LoggingSystem.Error($"Exception stack trace: {ex.StackTrace}", "Headphones");
                UnloadAssets();
                return false;
            }
            
            LoggingSystem.Debug("=== Prefab reference preservation completed ===", "Headphones");
            
            // DON'T unload the bundle aggressively - this was the problem!
            // We can unload the bundle metadata but keep the loaded objects
            LoggingSystem.Debug("Unloading bundle metadata but keeping loaded objects", "Headphones");
            AssetBundleLoader.UnloadBundle(loadedBundle, false); // false = keep loaded objects
            loadedBundle = null; // Clear our reference to the bundle
            
            if (FeatureFlags.Headphones.ShowDebugInfo)
            {
                LogPrefabInfo();
            }

            return true;
        }

        /// <summary>
        /// Create instance of headphone prefab
        /// </summary>
        public GameObject CreateInstance()
        {
            if (!IsLoaded)
            {
                LoggingSystem.Warning("Cannot create instance - persistent prefab not available", "Headphones");
                return null;
            }

            try
            {
                var instance = UnityEngine.Object.Instantiate(persistentHeadphonePrefab);
                if (instance != null)
                {
                    instance.name = "HeadphoneInstance";
                    instance.SetActive(true); // Ensure the instance is active
                    LoggingSystem.Debug("Created headphone instance from persistent prefab", "Headphones");
                }
                return instance;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Exception instantiating headphone from prefab: {ex.Message}", "HeadphoneLoader");
                return null;
            }
        }

        /// <summary>
        /// Create a configured headphone instance
        /// </summary>
        public GameObject CreateInstanceWithConfig(Vector3 position, Quaternion rotation)
        {
            var instance = CreateInstance(position, rotation);
            if (instance == null) return null;

            // Apply headphone-specific configuration
            var renderer = instance.GetComponent<Renderer>();
            if (renderer != null)
            {
                // Set transparent material for preview mode
                var material = new Material(Shader.Find("Standard"));
                material.color = new Color(1f, 1f, 1f, 0.5f);
                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                renderer.material = material;
            }

            return instance;
        }

        /// <summary>
        /// Create headphone instance at position
        /// </summary>
        private GameObject CreateInstance(Vector3 position, Quaternion rotation)
        {
            try
            {
                if (persistentHeadphonePrefab == null)
                {
                    LoggingSystem.Warning("Persistent prefab not available - call LoadAssets() first", "HeadphoneLoader");
                    return null;
                }

                var instance = UnityEngine.Object.Instantiate(persistentHeadphonePrefab, position, rotation);
                if (instance != null)
                {
                    instance.name = "HeadphoneInstance";
                    instance.SetActive(true); // Ensure the instance is active
                    LoggingSystem.Debug("Created headphone instance from persistent prefab", "Headphones");
                }
                return instance;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Exception instantiating headphone from prefab: {ex.Message}", "HeadphoneLoader");
                return null;
            }
        }

        /// <summary>
        /// Unload headphone assets (soft unload - keeps persistent objects)
        /// </summary>
        public void UnloadAssets()
        {
            LoggingSystem.Info("Soft unloading headphone assets (keeping persistent objects)", "Headphones");

            // Don't clear persistent prefab reference during normal unload
            // Only clear bundle reference since it should already be unloaded
            if (loadedBundle != null)
            {
                LoggingSystem.Debug("Unloading remaining bundle metadata (keeping objects)", "Headphones");
                AssetBundleLoader.UnloadBundle(loadedBundle, false); // false = keep loaded objects
                loadedBundle = null;
            }
            
            LoggingSystem.Debug("Soft unload complete - persistent objects preserved", "Headphones");
        }

        /// <summary>
        /// Force unload all assets including persistent objects (for shutdown/reload)
        /// </summary>
        public void ForceUnloadAssets()
        {
            LoggingSystem.Info("Force unloading all headphone assets", "Headphones");

            // Destroy persistent objects
            if (persistentHeadphonePrefab != null)
            {
                LoggingSystem.Debug("Destroying persistent prefab", "Headphones");
                try
                {
                    UnityEngine.Object.Destroy(persistentHeadphonePrefab);
                }
                catch (global::System.Exception ex)
                {
                    LoggingSystem.Warning($"Exception destroying persistent prefab: {ex.Message}", "Headphones");
                }
                persistentHeadphonePrefab = null;
            }

            headphonePrefab = null;

            // Force unload bundle if still present
            if (loadedBundle != null)
            {
                LoggingSystem.Debug("Force unloading bundle with all objects", "Headphones");
                AssetBundleLoader.UnloadBundle(loadedBundle, true); // true = destroy all objects
                loadedBundle = null;
            }
            
            LoggingSystem.Debug("Force unload complete - all assets destroyed", "Headphones");
        }

        /// <summary>
        /// Log prefab information for debugging
        /// </summary>
        private void LogPrefabInfo()
        {
            if (persistentHeadphonePrefab == null) return;

            LoggingSystem.Debug($"Persistent prefab name: {persistentHeadphonePrefab.name}", "Headphones");
            LoggingSystem.Debug($"Persistent prefab components:", "Headphones");

            var components = persistentHeadphonePrefab.GetComponents<Component>();
            foreach (var component in components)
            {
                LoggingSystem.Debug($"  - {component.GetType().Name}", "Headphones");
            }

            var renderer = persistentHeadphonePrefab.GetComponent<Renderer>();
            if (renderer != null)
            {
                LoggingSystem.Debug($"Renderer bounds: {renderer.bounds}", "Headphones");
            }

            var meshFilter = persistentHeadphonePrefab.GetComponent<MeshFilter>();
            if (meshFilter?.sharedMesh != null)
            {
                var mesh = meshFilter.sharedMesh;
                LoggingSystem.Debug($"Mesh: {mesh.name}, vertices: {mesh.vertexCount}, triangles: {mesh.triangles.Length / 3}", "Headphones");
            }
        }

        /// <summary>
        /// Get loader status information
        /// </summary>
        public string GetStatus()
        {
            // Comprehensive debug logging
            LoggingSystem.Debug($"=== HeadphoneAssetLoader.GetStatus() Debug ===", "Headphones");
            LoggingSystem.Debug($"FeatureFlags.Headphones.Enabled: {FeatureFlags.Headphones.Enabled}", "Headphones");
            LoggingSystem.Debug($"headphonePrefab != null: {headphonePrefab != null}", "Headphones");
            LoggingSystem.Debug($"persistentHeadphonePrefab != null: {persistentHeadphonePrefab != null}", "Headphones");
            LoggingSystem.Debug($"loadedBundle != null: {loadedBundle != null}", "Headphones");
            
            if (persistentHeadphonePrefab != null)
            {
                try
                {
                    LoggingSystem.Debug($"persistentHeadphonePrefab.name: {persistentHeadphonePrefab.name}", "Headphones");
                    LoggingSystem.Debug($"persistentHeadphonePrefab == null (Unity null check): {persistentHeadphonePrefab == null}", "Headphones");
                }
                catch (global::System.Exception ex)
                {
                    LoggingSystem.Error($"Exception accessing persistentHeadphonePrefab: {ex.Message}", "Headphones");
                    LoggingSystem.Error("Persistent prefab appears to be destroyed!", "Headphones");
                    persistentHeadphonePrefab = null; // Clean up the reference
                }
            }
            
            LoggingSystem.Debug($"IsLoaded property result: {IsLoaded}", "Headphones");
            LoggingSystem.Debug($"=== End Debug ===", "Headphones");
            
            if (!FeatureFlags.Headphones.Enabled)
                return "Headphones feature disabled";
                
            if (!IsLoaded)
                return "Headphones not loaded";
                
            return $"Headphones loaded: {persistentHeadphonePrefab?.name ?? "unknown"}";
        }
    }
} 