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
    /// High-level manager for headphone functionality
    /// Orchestrates loading and attachment components
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

            LoggingSystem.Info("HeadphoneManager initialized", "Headphones");
            
            // Subscribe to player events
            PlayerManager.OnPlayerReady += OnPlayerReady;
            PlayerManager.OnPlayerLost += OnPlayerLost;
        }

        /// <summary>
        /// Initialize headphone system
        /// </summary>
        public bool Initialize()
        {
            LoggingSystem.Debug("=== HeadphoneManager.Initialize() called ===", "Headphones");
            LoggingSystem.Debug($"FeatureFlags.Headphones.Enabled: {FeatureFlags.Headphones.Enabled}", "Headphones");
            LoggingSystem.Debug($"isInitialized: {isInitialized}", "Headphones");
            
            if (!FeatureFlags.Headphones.Enabled)
            {
                LoggingSystem.Warning("Headphones feature is disabled", "Headphones");
                return false;
            }

            if (isInitialized)
            {
                LoggingSystem.Info("Headphone system already initialized", "Headphones");
                LoggingSystem.Debug($"Current loader.IsLoaded status: {loader.IsLoaded}", "Headphones");
                return true;
            }

            try
            {
                LoggingSystem.Info("Initializing headphone system", "Headphones");
                
                // Subscribe to player events
                PlayerManager.OnPlayerReady += OnPlayerReady;
                PlayerManager.OnPlayerLost += OnPlayerLost;

                // Load assets
                LoggingSystem.Info("Loading headphones from embedded resource", "Headphones");
                LoggingSystem.Debug($"loader.IsLoaded before loading: {loader.IsLoaded}", "Headphones");
                
                bool loadSuccess = loader.LoadFromEmbeddedResource();
                
                LoggingSystem.Debug($"LoadFromEmbeddedResource returned: {loadSuccess}", "Headphones");
                LoggingSystem.Debug($"loader.IsLoaded after loading: {loader.IsLoaded}", "Headphones");
                LoggingSystem.Debug($"Immediate loader status: {loader.GetStatus()}", "Headphones");
                
                if (!loadSuccess)
                {
                    LoggingSystem.Error("Failed to load headphone assets from embedded resource", "Headphones");
                    LoggingSystem.Error("The headphone asset should be embedded in the mod. Check if EmbeddedResources/cleanheadphones exists.", "Headphones");
                    return false;
                }

                isInitialized = true;
                LoggingSystem.Info("Headphone system initialized successfully", "Headphones");
                LoggingSystem.Debug($"Final loader.IsLoaded status: {loader.IsLoaded}", "Headphones");
                LoggingSystem.Debug($"Final loader status: {loader.GetStatus()}", "Headphones");

                // Auto-attach if enabled and player is ready
                if (FeatureFlags.Headphones.AutoAttachOnSpawn && PlayerManager.CurrentPlayer != null)
                {
                    LoggingSystem.Info("Auto-attaching headphones to existing player", "Headphones");
                    AttachHeadphones();
                }

                LoggingSystem.Debug("=== HeadphoneManager.Initialize() completed successfully ===", "Headphones");
                return true;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Exception during headphone initialization: {ex.Message}", "Headphones");
                LoggingSystem.Error($"Exception stack trace: {ex.StackTrace}", "Headphones");
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
                LoggingSystem.Warning("Headphone system not initialized", "Headphones");
                return false;
            }

            if (!loader.IsLoaded)
            {
                LoggingSystem.Warning("Headphone assets not loaded", "Headphones");
                return false;
            }

            LoggingSystem.Info("Attempting to attach headphones", "Headphones");
            return attachment.AttachToPlayer(loader.HeadphonePrefab, player);
        }

        /// <summary>
        /// Remove headphones from player
        /// </summary>
        public void RemoveHeadphones()
        {
            LoggingSystem.Info("Removing headphones", "Headphones");
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
            else
            {
                return AttachHeadphones(player);
            }
        }

        /// <summary>
        /// Check if headphones are currently attached
        /// </summary>
        public bool AreHeadphonesAttached => attachment.IsAttached;

        /// <summary>
        /// Get current headphone state
        /// </summary>
        public HeadphoneState GetState() => attachment.GetState();

        /// <summary>
        /// Get system status
        /// </summary>
        public string GetStatus()
        {
            LoggingSystem.Debug($"HeadphoneManager.GetStatus() called - Enabled: {FeatureFlags.Headphones.Enabled}, Initialized: {isInitialized}, LoaderIsLoaded: {loader?.IsLoaded}", "Headphones");
            
            if (!FeatureFlags.Headphones.Enabled)
            {
                LoggingSystem.Debug("Returning 'Headphones feature disabled'", "Headphones");
                return "Headphones feature disabled";
            }
                
            if (!isInitialized)
            {
                LoggingSystem.Debug("Returning 'Headphone system not initialized'", "Headphones");
                return "Headphone system not initialized";
            }
                
            if (!loader.IsLoaded)
            {
                LoggingSystem.Debug("Returning 'Headphone assets not loaded'", "Headphones");
                return "Headphone assets not loaded";
            }
                
            var attachmentStatus = attachment.GetStatus();
            LoggingSystem.Debug($"Returning attachment status: {attachmentStatus}", "Headphones");
            return attachmentStatus;
        }

        /// <summary>
        /// Reload headphone assets
        /// </summary>
        public bool ReloadAssets()
        {
            LoggingSystem.Info("Reloading headphone assets", "Headphones");
            
            // Detach if currently attached
            if (attachment.IsAttached)
            {
                RemoveHeadphones();
            }

            // Force unload existing assets (including persistent objects for clean reload)
            loader.ForceUnloadAssets();

            // Reload assets from embedded resource
            bool success = loader.LoadFromEmbeddedResource();

            if (success)
            {
                LoggingSystem.Info("Headphone assets reloaded successfully", "Headphones");
            }
            else
            {
                LoggingSystem.Error("Failed to reload headphone assets from embedded resource", "Headphones");
            }

            return success;
        }

        /// <summary>
        /// Handle player ready event
        /// </summary>
        private void OnPlayerReady(Il2CppScheduleOne.PlayerScripts.Player player)
        {
            LoggingSystem.Info($"Player ready: {player.name}", "Headphones");
            
            if (FeatureFlags.Headphones.AutoAttachOnSpawn && isInitialized && loader.IsLoaded)
            {
                LoggingSystem.Info("Auto-attaching headphones to new player", "Headphones");
                AttachHeadphones(player);
            }
        }

        /// <summary>
        /// Handle player lost event
        /// </summary>
        private void OnPlayerLost()
        {
            LoggingSystem.Info("Player lost, detaching headphones", "Headphones");
            RemoveHeadphones();
        }

        /// <summary>
        /// Shutdown headphone manager
        /// </summary>
        public void Shutdown()
        {
            LoggingSystem.Info("Shutting down headphone manager", "Headphones");
            
            // Unsubscribe from events
            PlayerManager.OnPlayerReady -= OnPlayerReady;
            PlayerManager.OnPlayerLost -= OnPlayerLost;
            
            // Remove headphones
            RemoveHeadphones();
            
            // Force unload all assets for complete cleanup
            loader.ForceUnloadAssets();
            
            isInitialized = false;
        }

        /// <summary>
        /// Get loader for direct access (if needed)
        /// </summary>
        public HeadphoneAssetLoader GetLoader() => loader;

        /// <summary>
        /// Get attachment handler for direct access (if needed)
        /// </summary>
        public HeadphoneAttachment GetAttachment() => attachment;
    }
} 