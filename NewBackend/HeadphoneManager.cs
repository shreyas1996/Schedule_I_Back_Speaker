using System;
using System.Collections;
using UnityEngine;
using MelonLoader;
using BackSpeakerMod.S1Wrapper;
using BackSpeakerMod.S1Wrapper.Interfaces;
using BackSpeakerMod.NewBackend.Utils;
using BackSpeakerMod.NewBackend.Configs;

namespace BackSpeakerMod.NewBackend
{
    /// <summary>
    /// Manages headphone asset loading and attachment to player
    /// </summary>
    public class HeadphoneManager
    {
        private IPlayer? _player;
        private GameObject? _headphoneObject;
        private bool _headphonesAttached = false;
        
        private readonly string _assetBundleName = "BackSpeakerMod.EmbeddedResources.scheduleoneheadphones";
        public event Action<bool>? OnHeadphonesStateChanged;
        
        public bool AreHeadphonesAttached => _headphonesAttached;
        
        /// <summary>
        /// Initialize and load headphone prefab from asset bundle
        /// </summary>
        public IEnumerator Initialize(IPlayer player)
        {
            _player = player;
            NewLoggingSystem.Info("Initializing HeadphoneManager", "HeadphoneManager");

            try
            {
                // Load asset bundle from embedded resources
                NewLoggingSystem.Debug("Loading headphone asset bundle from embedded resources", "HeadphoneManager");
                var assetBundle = S1AssetBundleLoader.LoadFromEmbeddedResource("scheduleoneheadphones");
                
                if (assetBundle == null || !assetBundle.IsValid)
                {
                    NewLoggingSystem.Error("Failed to load headphone asset bundle", "HeadphoneManager");
                    yield break;
                }

                NewLoggingSystem.Debug("Successfully loaded headphone asset bundle", "HeadphoneManager");

                // Load all GameObjects from the bundle to find the headphone prefab
                var gameObjects = assetBundle.LoadAllAssets<GameObject>();
                NewLoggingSystem.Debug($"Found {gameObjects.Length} GameObjects in asset bundle", "HeadphoneManager");

                GameObject? headphonePrefab = null;
                foreach (var go in gameObjects)
                {
                    if (go != null)
                    {
                        NewLoggingSystem.Debug($"Found GameObject: {go.name}", "HeadphoneManager");
                        if (go.name.ToLower().Contains("headphone"))
                        {
                            headphonePrefab = go;
                            NewLoggingSystem.Debug($"Selected headphone prefab: {go.name}", "HeadphoneManager");
                            break;
                        }
                    }
                }

                if (headphonePrefab == null)
                {
                    NewLoggingSystem.Error("No headphone prefab found in asset bundle", "HeadphoneManager");
                    
                    // List all available assets for debugging
                    var allAssetNames = assetBundle.GetAllAssetNames();
                    NewLoggingSystem.Debug($"Available assets: [{string.Join(", ", allAssetNames)}]", "HeadphoneManager");
                    yield break;
                }

                // Instantiate the headphone object
                NewLoggingSystem.Debug("Instantiating headphone object", "HeadphoneManager");
                _headphoneObject = UnityEngine.Object.Instantiate(headphonePrefab);
                
                if (_headphoneObject != null)
                {
                    NewLoggingSystem.Debug($"Successfully instantiated headphone object: {_headphoneObject.name}", "HeadphoneManager");
                    
                    // Attach to player if available
                    if (_player?.Transform != null)
                    {
                        AttachHeadphonesToPlayer();
                    }
                    else
                    {
                        NewLoggingSystem.Debug("Player transform not available, headphones will be positioned later", "HeadphoneManager");
                    }
                }
                else
                {
                    NewLoggingSystem.Error("Failed to instantiate headphone object", "HeadphoneManager");
                }

                // // Clean up asset bundle
                // assetBundle.Unload(false);
                // NewLoggingSystem.Debug("Asset bundle unloaded", "HeadphoneManager");
            }
            catch (System.Exception ex)
            {
                NewLoggingSystem.Error($"Exception in HeadphoneManager.Initialize: {ex.Message}", "HeadphoneManager");
                NewLoggingSystem.Debug($"Exception details: {ex}", "HeadphoneManager");
            }

            NewLoggingSystem.Info("✓ HeadphoneManager initialized", "HeadphoneManager");
        }
        
        private void AttachHeadphonesToPlayer()
        {
            if (_player?.Transform == null)
            {
                NewLoggingSystem.Error("Player transform is null", "HeadphoneManager");
                return;
            }
            
            // Find head attachment point
            // Transform headTransform = FindHeadAttachmentPoint(_player.GameObject);
            Transform headTransform = FindHeadAttachmentPoint.FindEarAttachmentPoint(_player.Avatar?.HeadBone);
            if (headTransform == null)
            {
                NewLoggingSystem.Error("Could not find head attachment point", "HeadphoneManager");
                return;
            }
            
            // Attach headphones to player
            _headphoneObject.transform.SetParent(headTransform);
            _headphoneObject.transform.localPosition = new Vector3(0f, -0.0011f, -0.0005f);
            _headphoneObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            _headphoneObject.transform.localScale = Vector3.one * 0.2f;
            _headphoneObject.name = "HeadphoneInstance_Attached";
            _headphoneObject.SetActive(true);
            var renderers = _headphoneObject.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    renderer.enabled = true;
                    NewLoggingSystem.Debug($"Renderer: {renderer.name}, enabled: {renderer.enabled}, visible: {renderer.isVisible}", "HeadphoneManager");
                }
            // Set Headphone name in config to the new name
            HeadphoneConfig.Name = _headphoneObject.name;
            
            // Apply URP materials at runtime
            NewLoggingSystem.Debug("Applying shader and materials to headphone object", "HeadphoneManager");
            FixShaderAndMaterial.ApplyShaderAndMaterials(_headphoneObject);
            NewLoggingSystem.Debug("✓ Shader and materials applied to headphone object", "HeadphoneManager");
            
            UnityEngine.Object.DontDestroyOnLoad(_headphoneObject);
            
            _headphonesAttached = true;
            OnHeadphonesStateChanged?.Invoke(true);
            
            NewLoggingSystem.Info("✓ Headphones attached to player", "HeadphoneManager");
        }

        public void EnableHeadphones()
        {
            if (_headphoneObject != null)
            {
                _headphoneObject.SetActive(true);
                _headphonesAttached = true;
                OnHeadphonesStateChanged?.Invoke(true);
                NewLoggingSystem.Info("✓ Headphones enabled", "HeadphoneManager");
            }
        }

        public void DisableHeadphones()
        {
            if (_headphoneObject != null)
            {
                // _headphoneObject.transform.SetParent(null);
                _headphoneObject.SetActive(false);
                _headphonesAttached = false;
                OnHeadphonesStateChanged?.Invoke(false);
                NewLoggingSystem.Info("✓ Headphones detached from player", "HeadphoneManager");
            }
        }

        public void ToggleHeadphones()
        {
            if (_headphonesAttached)
            {
                DisableHeadphones();
            }
            else
            {
                EnableHeadphones();
            }
        }
        
        public void Shutdown()
        {
            NewLoggingSystem.Info("Shutting down HeadphoneManager", "HeadphoneManager");
            
            // Detach headphones
            if (_headphoneObject != null)
            {
                // GameObject.Destroy(_currentHeadphoneInstance!);
                GameObject.Destroy(_headphoneObject!);
                _headphonesAttached = false;
                OnHeadphonesStateChanged?.Invoke(false);
            }
            
            // Unload asset bundle
            S1AssetBundleLoader.UnloadAssetBundle("scheduleoneheadphones");
            
            _player = null;
            _headphoneObject = null;
            _headphonesAttached = false;
            
            NewLoggingSystem.Info("✓ HeadphoneManager shutdown complete", "HeadphoneManager");
        }
    }
} 