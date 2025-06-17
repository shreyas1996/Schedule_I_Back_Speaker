using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Features.Headphones.Loading;
using BackSpeakerMod.Core.Features.Headphones.Attachment;
using BackSpeakerMod.S1Wrapper.Interfaces;
using BackSpeakerMod.Core.Features.Player;
using PlayerManager = BackSpeakerMod.Core.Features.Player.PlayerManager;
using System;

namespace BackSpeakerMod.Core.Features.Headphones.Managers
{
    /// <summary>
    /// Simple headphone manager with reliable asset loading
    /// </summary>
    public class HeadphoneManager
    {
        private readonly HeadphoneConfig config;
        private readonly HeadphoneAssetLoader loader;
        private readonly HeadphoneAttachment attachment;
        private bool isInitialized = false;

        // Reference to PlayerAttachment for coordinated initialization
        private PlayerAttachment? playerAttachment = null;

        /// <summary>
        /// Initialize headphone manager
        /// </summary>
        public HeadphoneManager(HeadphoneConfig? headphoneConfig = null)
        {
            config = headphoneConfig ?? new HeadphoneConfig();
            loader = new HeadphoneAssetLoader(config);
            attachment = new HeadphoneAttachment(config);
            
            PlayerManager.OnPlayerReady += OnPlayerReady;
            PlayerManager.OnPlayerLost += OnPlayerLost;
        }

        /// <summary>
        /// Set PlayerAttachment reference for coordinated initialization
        /// </summary>
        public void SetPlayerAttachment(PlayerAttachment playerAttachment)
        {
            this.playerAttachment = playerAttachment;
        }

        /// <summary>
        /// Update headphone system - should be called regularly to monitor camera changes
        /// </summary>
        public void Update()
        {
            if (isInitialized)
            {
                attachment.Update();
            }
        }

        /// <summary>
        /// Initialize headphone system (simplified)
        /// </summary>
        public bool Initialize()
        {
            if (!FeatureFlags.Headphones.Enabled)
            {
                return false;
            }

            if (isInitialized)
            {
                return true;
            }

            try
            {
                bool loaded = loader.LoadFromEmbeddedResource();
                
                if (!loaded)
                {
                    return false;
                }

                isInitialized = true;

                if (config.AutoAttachOnSpawn && PlayerManager.CurrentPlayer != null)
                {
                    AttachHeadphones();
                }

                return true;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"HeadphoneManager: Initialize failed: {ex}", "HeadphoneManager");
                return false;
            }
        }

        /// <summary>
        /// Attach headphones to player
        /// </summary>
        public bool AttachHeadphones(IPlayer? player = null)
        {
            if (!isInitialized)
            {
                return false;
            }

            if (!loader.IsLoaded)
            {
                return false;
            }

            bool success = attachment.AttachToPlayer(loader.HeadphonePrefab!, player);
            
            if (success && playerAttachment != null)
            {
                playerAttachment.AttachSpeakerWithHeadphones();
            }
            
            return success;
        }

        /// <summary>
        /// Remove headphones from player
        /// </summary>
        public bool RemoveHeadphones()
        {
            attachment.DetachFromPlayer();
            return true;
        }

        /// <summary>
        /// Toggle headphones on/off
        /// </summary>
        public bool ToggleHeadphones(IPlayer? player = null)
        {
            if (attachment.IsAttached)
            {
                return RemoveHeadphones();
            }
            return AttachHeadphones(player);
        }

        /// <summary>
        /// Check if headphones are attached
        /// </summary>
        public bool AreHeadphonesAttached => attachment.IsAttached;

        /// <summary>
        /// Get current state
        /// </summary>
        public HeadphoneState GetState() => attachment.GetState();

        /// <summary>
        /// Get simple, clear status
        /// </summary>
        public string GetStatus()
        {
            if (!FeatureFlags.Headphones.Enabled)
                return "Disabled";
            
            if (!isInitialized)
                return "Not initialized";
            
            if (!loader.IsLoaded)
                return $"Assets not loaded ({loader.GetDetailedStatus()})";
            
            return attachment.GetStatus();
        }

        /// <summary>
        /// Get detailed status including camera information
        /// </summary>
        public string GetDetailedStatus()
        {
            if (!FeatureFlags.Headphones.Enabled)
                return "Disabled";
            
            if (!isInitialized)
                return "Not initialized";
            
            if (!loader.IsLoaded)
                return $"Assets not loaded ({loader.GetDetailedStatus()})";
            
            return attachment.GetDetailedStatus();
        }

        /// <summary>
        /// Get camera information for debugging
        /// </summary>
        public string GetCameraInfo()
        {
            if (!isInitialized)
                return "Not initialized";
            
            return attachment.GetCameraInfo();
        }

        /// <summary>
        /// Force update headphone visibility (useful for debugging)
        /// </summary>
        public void ForceUpdateVisibility()
        {
            if (isInitialized)
            {
                attachment.ForceUpdateVisibility();
            }
        }

        /// <summary>
        /// Reload assets (simplified)
        /// </summary>
        public bool ReloadAssets()
        {
            if (!FeatureFlags.Headphones.Enabled)
                return false;

            attachment.DetachFromPlayer();
            loader.Unload();
            isInitialized = false;

            return Initialize();
        }

        /// <summary>
        /// Player ready event handler
        /// </summary>
        private void OnPlayerReady(IPlayer? player)
        {
            if (config.AutoAttachOnSpawn && isInitialized && loader.IsLoaded)
            {
                AttachHeadphones(player);
            }
        }

        /// <summary>
        /// Player lost event handler
        /// </summary>
        private void OnPlayerLost(IPlayer? player)
        {
            attachment.DetachFromPlayer();
        }

        /// <summary>
        /// Shutdown system
        /// </summary>
        public void Shutdown()
        {
            PlayerManager.OnPlayerReady -= OnPlayerReady;
            PlayerManager.OnPlayerLost -= OnPlayerLost;
            
            attachment.DetachFromPlayer();
            loader.Unload();
            isInitialized = false;
        }

        /// <summary>
        /// Get loader reference
        /// </summary>
        public HeadphoneAssetLoader GetLoader() => loader;

        /// <summary>
        /// Get attachment reference
        /// </summary>
        public HeadphoneAttachment GetAttachment() => attachment;

        /// <summary>
        /// Load headphone assets at startup (called once during mod initialization)
        /// </summary>
        public bool LoadHeadphoneAssets()
        {
            try
            {
                LoggingSystem.Info("Loading headphone assets at startup", "HeadphoneManager");
                return loader.LoadFromEmbeddedResource();
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to load headphone assets: {ex}", "HeadphoneManager");
                return false;
            }
        }
    }
} 