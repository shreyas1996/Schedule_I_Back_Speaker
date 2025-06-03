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

        // Reference to PlayerAttachment for coordinated initialization
        private BackSpeakerMod.Core.Features.Player.Attachment.PlayerAttachment? playerAttachment = null;

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
        public void SetPlayerAttachment(BackSpeakerMod.Core.Features.Player.Attachment.PlayerAttachment playerAttachment)
        {
            this.playerAttachment = playerAttachment;
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
        public bool AttachHeadphones(Il2CppScheduleOne.PlayerScripts.Player? player = null)
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
        public void RemoveHeadphones()
        {
            attachment.DetachFromPlayer();
        }

        /// <summary>
        /// Toggle headphones on/off
        /// </summary>
        public bool ToggleHeadphones(Il2CppScheduleOne.PlayerScripts.Player? player = null)
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

            attachment.DetachFromPlayer();
            loader.Unload();
            isInitialized = false;

            return Initialize();
        }

        /// <summary>
        /// Player ready event handler
        /// </summary>
        private void OnPlayerReady(Il2CppScheduleOne.PlayerScripts.Player? player)
        {
            if (config.AutoAttachOnSpawn && isInitialized && loader.IsLoaded)
            {
                AttachHeadphones(player);
            }
        }

        /// <summary>
        /// Player lost event handler
        /// </summary>
        private void OnPlayerLost(Il2CppScheduleOne.PlayerScripts.Player? player)
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