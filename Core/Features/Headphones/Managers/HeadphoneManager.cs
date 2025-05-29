using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Features.Headphones.Data;
using BackSpeakerMod.Core.Features.Headphones.Loading;
using BackSpeakerMod.Core.Features.Headphones.Attachment;
using Il2CppScheduleOne.PlayerScripts;
using PlayerManager = BackSpeakerMod.Core.Common.Managers.PlayerManager;
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
            
            LoggingSystem.Info("HeadphoneManager created", "Headphones");
        }

        /// <summary>
        /// Initialize headphone system (simplified)
        /// </summary>
        public bool Initialize()
        {
            if (!FeatureFlags.Headphones.Enabled)
            {
                LoggingSystem.Info("Headphones feature is disabled", "Headphones");
                return false;
            }

            if (isInitialized)
            {
                LoggingSystem.Info("HeadphoneManager already initialized", "Headphones");
                return true;
            }

            try
            {
                LoggingSystem.Info("Initializing headphone system...", "Headphones");

                // Load assets using simple approach
                LoggingSystem.Debug($"Before loading - Loader status: {loader.GetDetailedStatus()}", "Headphones");
                bool loaded = loader.LoadFromEmbeddedResource();
                LoggingSystem.Debug($"After loading - Success: {loaded}, Loader status: {loader.GetDetailedStatus()}", "Headphones");
                
                if (!loaded)
                {
                    LoggingSystem.Error($"Failed to load headphone assets. Loader status: {loader.GetDetailedStatus()}", "Headphones");
                    return false;
                }

                isInitialized = true;
                LoggingSystem.Info("✓ Headphone system initialized successfully", "Headphones");

                // Auto-attach if enabled and player is ready
                if (config.AutoAttachOnSpawn && PlayerManager.CurrentPlayer != null)
                {
                    LoggingSystem.Info("Auto-attaching headphones to current player", "Headphones");
                    AttachHeadphones();
                }

                return true;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Headphone initialization failed: {ex.Message}", "Headphones");
                return false;
            }
        }

        /// <summary>
        /// Attach headphones to player
        /// </summary>
        public bool AttachHeadphones(Il2CppScheduleOne.PlayerScripts.Player player = null)
        {
            if (!isInitialized)
            {
                LoggingSystem.Warning("Cannot attach headphones - system not initialized", "Headphones");
                return false;
            }

            LoggingSystem.Debug($"AttachHeadphones - Loader detailed status: {loader.GetDetailedStatus()}", "Headphones");

            if (!loader.IsLoaded)
            {
                LoggingSystem.Warning($"Cannot attach headphones - assets not loaded. Details: {loader.GetDetailedStatus()}", "Headphones");
                return false;
            }

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
        /// Get simple, clear status
        /// </summary>
        public string GetStatus()
        {
            if (!FeatureFlags.Headphones.Enabled)
                return "Disabled";
            
            if (!isInitialized)
                return "Not initialized";
            
            // Debug the loader state
            // LoggingSystem.Debug($"GetStatus - Loader detailed status: {loader.GetDetailedStatus()}", "Headphones");
            
            if (!loader.IsLoaded)
                return $"Assets not loaded ({loader.GetDetailedStatus()})";
            
            return attachment.GetStatus();
        }

        /// <summary>
        /// Reload assets (simplified)
        /// </summary>
        public bool ReloadAssets()
        {
            if (!FeatureFlags.Headphones.Enabled)
                return false;

            LoggingSystem.Info("Reloading headphone assets", "Headphones");

            // Detach and cleanup
            attachment.DetachFromPlayer();
            loader.Unload();
            isInitialized = false;

            // Reinitialize
            return Initialize();
        }

        /// <summary>
        /// Player ready event handler
        /// </summary>
        private void OnPlayerReady(Il2CppScheduleOne.PlayerScripts.Player player)
        {
            if (config.AutoAttachOnSpawn && isInitialized && loader.IsLoaded)
            {
                LoggingSystem.Info($"Player ready, auto-attaching headphones to {player.name}", "Headphones");
                AttachHeadphones(player);
            }
        }

        /// <summary>
        /// Player lost event handler
        /// </summary>
        private void OnPlayerLost()
        {
            LoggingSystem.Info("Player lost, detaching headphones", "Headphones");
            attachment.DetachFromPlayer();
        }

        /// <summary>
        /// Shutdown system
        /// </summary>
        public void Shutdown()
        {
            LoggingSystem.Info("Shutting down HeadphoneManager", "Headphones");

            // Unsubscribe from events
            PlayerManager.OnPlayerReady -= OnPlayerReady;
            PlayerManager.OnPlayerLost -= OnPlayerLost;
            
            // Cleanup
            attachment.DetachFromPlayer();
            loader.Unload();
            isInitialized = false;
            
            LoggingSystem.Info("HeadphoneManager shutdown complete", "Headphones");
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