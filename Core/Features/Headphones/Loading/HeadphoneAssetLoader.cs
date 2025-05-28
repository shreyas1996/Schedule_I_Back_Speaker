using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Common.Loaders;
using BackSpeakerMod.Core.Features.Headphones.Data;
using System.Threading.Tasks;
using System.Collections;

namespace BackSpeakerMod.Core.Features.Headphones.Loading
{
    /// <summary>
    /// Simple streaming asset loader for headphones
    /// </summary>
    public class HeadphoneAssetLoader
    {
        private readonly HeadphoneConfig config;
        private GameObject headphonePrefab;
        private bool isLoading = false;

        /// <summary>
        /// Whether headphone assets are loaded
        /// </summary>
        public bool IsLoaded => headphonePrefab != null;

        /// <summary>
        /// Get the loaded headphone prefab
        /// </summary>
        public GameObject HeadphonePrefab => headphonePrefab;

        /// <summary>
        /// Initialize with config
        /// </summary>
        public HeadphoneAssetLoader(HeadphoneConfig headphoneConfig = null)
        {
            config = headphoneConfig ?? new HeadphoneConfig();
        }

        /// <summary>
        /// Load headphones using streaming assets (async)
        /// </summary>
        public async Task<bool> LoadAsync()
        {
            if (!FeatureFlags.Headphones.Enabled || IsLoaded || isLoading)
                return IsLoaded;

            isLoading = true;

            try
            {
                // Load bundle from streaming assets
                var bundleRequest = Il2CppAssetBundleManager.LoadFromFileAsync(
                    global::System.IO.Path.Combine(Application.streamingAssetsPath, config.AssetBundleName)
                );

                // Wait for bundle to load
                while (!bundleRequest.isDone)
                    await Task.Delay(10);

                var bundle = bundleRequest.assetBundle;
                if (bundle == null)
                {
                    LoggingSystem.Error("Failed to load headphone bundle", "Headphones");
                    return false;
                }

                // Load asset from bundle
                var assetRequest = bundle.LoadAssetAsync<GameObject>(config.AssetName);
                while (!assetRequest.isDone)
                    await Task.Delay(10);

                headphonePrefab = assetRequest.asset.Cast<GameObject>();

                if (headphonePrefab == null)
                {
                    LoggingSystem.Error("Headphone prefab not found in bundle", "Headphones");
                    bundle.Unload(true);
                    return false;
                }

                LoggingSystem.Info($"Headphones loaded: {headphonePrefab.name}", "Headphones");
                return true;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Error loading headphones: {ex.Message}", "Headphones");
                return false;
            }
            finally
            {
                isLoading = false;
            }
        }

        /// <summary>
        /// Load headphones synchronously from embedded resources (fallback)
        /// </summary>
        public bool LoadFromEmbeddedResource()
        {
            if (!FeatureFlags.Headphones.Enabled || IsLoaded)
                return IsLoaded;

            var bundle = AssetBundleLoader.LoadFromEmbeddedResource(config.EmbeddedResourceName);
            if (bundle == null)
                return false;

            headphonePrefab = AssetBundleLoader.LoadAsset<GameObject>(bundle, config.AssetName);
            
            if (headphonePrefab == null)
            {
                bundle.Unload(true);
                return false;
            }

            LoggingSystem.Info($"Headphones loaded from embedded: {headphonePrefab.name}", "Headphones");
            return true;
        }

        /// <summary>
        /// Create headphone instance
        /// </summary>
        public GameObject CreateInstance()
        {
            if (!IsLoaded)
                return null;

            var instance = UnityEngine.Object.Instantiate(headphonePrefab);
            instance.name = "HeadphoneInstance";
            return instance;
        }

        /// <summary>
        /// Create instance at specific position
        /// </summary>
        public GameObject CreateInstance(Vector3 position, Quaternion rotation)
        {
            if (!IsLoaded)
                return null;

            var instance = UnityEngine.Object.Instantiate(headphonePrefab, position, rotation);
            instance.name = "HeadphoneInstance";
            return instance;
        }

        /// <summary>
        /// Get simple status
        /// </summary>
        public string GetStatus()
        {
            if (!FeatureFlags.Headphones.Enabled)
                return "Disabled";
            if (isLoading)
                return "Loading...";
            if (IsLoaded)
                return $"Loaded: {headphonePrefab.name}";
            return "Not loaded";
        }

        /// <summary>
        /// Clean up
        /// </summary>
        public void Unload()
        {
            headphonePrefab = null;
            isLoading = false;
        }
    }
} 