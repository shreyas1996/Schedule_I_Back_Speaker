using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Features.Headphones.Data;
using BackSpeakerMod.Core.Features.Headphones.Loading;
using BackSpeakerMod.Core.Features.Headphones.Attachment;
using Il2CppScheduleOne.PlayerScripts;
using PlayerManager = BackSpeakerMod.Core.Common.Managers.PlayerManager;
using System;
using System.Threading.Tasks;

namespace BackSpeakerMod.Core.Features.Headphones.Managers
{
    /// <summary>
    /// Simple headphone manager with streaming asset support
    /// </summary>
    public class HeadphoneManager
    {
        private readonly HeadphoneConfig config;
        private readonly HeadphoneAssetLoader loader;
        private readonly HeadphoneAttachment attachment;
        private bool isInitialized = false;

        /// <summary>
        /// Initialize headphone manager
        /// </summary>
        public HeadphoneManager(HeadphoneConfig headphoneConfig = null)
        {
            config = headphoneConfig ?? new HeadphoneConfig();
            loader = new HeadphoneAssetLoader(config);
            attachment = new HeadphoneAttachment(config);
            
            // Subscribe to player events
            PlayerManager.OnPlayerReady += OnPlayerReady;
            PlayerManager.OnPlayerLost += OnPlayerLost;
        }

        /// <summary>
        /// Initialize with async loading
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            if (!FeatureFlags.Headphones.Enabled || isInitialized)
                return isInitialized;

            try
            {
                // Try streaming assets first, fallback to embedded
                // bool loaded = await loader.LoadAsync();
                LoggingSystem.Info("Loading cleanheadphones from streaming assets", "CleanHeadphones");
                bool loaded = loader.LoadFromEmbeddedResource();
                // if (!loaded)
                // {
                //     LoggingSystem.Info("Streaming assets failed, trying embedded resources", "Headphones");
                //     loaded = loader.LoadFromEmbeddedResource();
                // }

                if (!loaded)
                {
                    LoggingSystem.Error("Failed to load headphone assets", "Headphones");
                    return false;
                }

                isInitialized = true;
                LoggingSystem.Info("Headphone system initialized", "Headphones");

                // Auto-attach if enabled
                if (config.AutoAttachOnSpawn && PlayerManager.CurrentPlayer != null)
                    AttachHeadphones();

                return true;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Headphone initialization failed: {ex.Message}", "Headphones");
                return false;
            }
        }

        /// <summary>
        /// Initialize synchronously (fallback)
        /// </summary>
        public bool Initialize()
        {
            if (!FeatureFlags.Headphones.Enabled || isInitialized)
                return isInitialized;

            bool loaded = loader.LoadFromEmbeddedResource();
            if (!loaded)
            {
                LoggingSystem.Error("Failed to load headphone assets", "Headphones");
                return false;
            }

            isInitialized = true;
            LoggingSystem.Info("Headphone system initialized", "Headphones");

            if (config.AutoAttachOnSpawn && PlayerManager.CurrentPlayer != null)
                AttachHeadphones();

            return true;
        }

        /// <summary>
        /// Attach headphones to player
        /// </summary>
        public bool AttachHeadphones(Il2CppScheduleOne.PlayerScripts.Player player = null)
        {
            if (!isInitialized || !loader.IsLoaded)
                return false;

            return attachment.AttachToPlayer(loader.HeadphonePrefab, player);
        }

        /// <summary>
        /// Remove headphones from player
        /// </summary>
        public void RemoveHeadphones()
        {
            attachment.DetachFromPlayer();
        }

        /// <summary>
        /// Toggle headphones on/off
        /// </summary>
        public bool ToggleHeadphones(Il2CppScheduleOne.PlayerScripts.Player player = null)
        {
            if (attachment.IsAttached)
            {
                RemoveHeadphones();
                return false;
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
        /// Get simple status
        /// </summary>
        public string GetStatus()
        {
            if (!FeatureFlags.Headphones.Enabled)
                return "Disabled";
            if (!isInitialized)
                return "Not initialized";
            if (!loader.IsLoaded)
                return "Assets not loaded";
            return attachment.GetStatus();
        }

        /// <summary>
        /// Reload assets
        /// </summary>
        public async Task<bool> ReloadAssetsAsync()
        {
            if (!FeatureFlags.Headphones.Enabled)
                return false;

            attachment.DetachFromPlayer();
            loader.Unload();
            isInitialized = false;

            return await InitializeAsync();
        }

        /// <summary>
        /// Player ready event handler
        /// </summary>
        private void OnPlayerReady(Il2CppScheduleOne.PlayerScripts.Player player)
        {
            if (config.AutoAttachOnSpawn && isInitialized && loader.IsLoaded)
                AttachHeadphones(player);
        }

        /// <summary>
        /// Player lost event handler
        /// </summary>
        private void OnPlayerLost()
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
    }
} 