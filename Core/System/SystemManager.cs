using System;
using System.Collections.Generic;
using UnityEngine;
using BackSpeakerMod.Core.Features.Player.Attachment;
using BackSpeakerMod.Core.Features.Headphones.Managers;
using BackSpeakerMod.Core.Features.Audio.Managers;
using BackSpeakerMod.Core.Common.Managers;
using BackSpeakerMod.Core.Modules;
using BackSpeakerMod.Configuration;

namespace BackSpeakerMod.Core.System
{
    /// <summary>
    /// Consolidated system manager that replaces the previous fragmented system files
    /// Handles initialization, configuration, API, and component management in one place
    /// </summary>
    public class SystemManager
    {
        // Core components
        private PlayerAttachment? playerAttachment;
        private HeadphoneManager? headphoneManager;
        private AudioManager? audioManager;
        
        // State tracking
        private bool isInitialized = false;
        
        // Events
        public Action? OnTracksReloaded;
        public Action? OnInitializationComplete;
        public Action<AudioSource>? OnSpeakerAttached;

        // Feature toggles (consolidated from FeatureToggleSystem)
        public static class Features
        {
            public static bool HeadphonesEnabled = true;
            public static bool AudioEnabled = true;
            public static bool ShowDebugInfo = false;
            public static bool AutoLoadTracks = true;
        }

        // Configuration (consolidated from ConfigurationManager)
        private readonly Dictionary<string, object> settings = new Dictionary<string, object>();

        #region Initialization & Lifecycle

        public SystemManager()
        {
            LoadDefaultSettings();
            CreateComponents();
        }

        private void LoadDefaultSettings()
        {
            SetSetting("Audio.Volume", 0.5f);
            SetSetting("Audio.Enabled", true);
            SetSetting("Headphones.Enabled", true);
        }

        private void CreateComponents()
        {
            try
            {
                LoggingSystem.Info("Creating system components", "SystemManager");
                
                headphoneManager = new HeadphoneManager();
                audioManager = new AudioManager();
                playerAttachment = new PlayerAttachment();
                
                LoggingSystem.Info("System components created successfully", "SystemManager");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to create system components: {ex.Message}", "SystemManager");
                throw;
            }
        }

        public bool Initialize()
        {
            if (isInitialized) return true;

            try
            {
                LoggingSystem.Info("Initializing system manager", "SystemManager");

                // Step 1: Initialize core managers
                PlayerManager.Initialize();

                // Step 2: Initialize headphones first
                if (!Features.HeadphonesEnabled)
                {
                    LoggingSystem.Info("Headphones feature disabled", "SystemManager");
                    return false;
                }

                bool headphonesReady = headphoneManager?.Initialize() ?? false;
                if (!headphonesReady)
                {
                    LoggingSystem.Warning("Headphones initialization failed", "SystemManager");
                    return false;
                }

                // Step 3: Initialize player attachment
                playerAttachment?.Initialize();

                // Step 4: Wire up cross-component dependencies
                if (headphoneManager != null && playerAttachment != null)
                {
                    headphoneManager.SetPlayerAttachment(playerAttachment);
                }

                // Step 5: Set up event handlers
                SetupEventHandlers();

                isInitialized = true;
                OnInitializationComplete?.Invoke();
                
                LoggingSystem.Info("System manager initialized successfully", "SystemManager");
                return true;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"System initialization failed: {ex}", "SystemManager");
                return false;
            }
        }

        private void SetupEventHandlers()
        {
            if (playerAttachment != null)
            {
                playerAttachment.OnSpeakerAttached += HandleSpeakerAttached;
                playerAttachment.OnSpeakerDetached += HandleSpeakerDetached;
            }

            if (audioManager != null)
            {
                audioManager.OnTracksReloaded += () => OnTracksReloaded?.Invoke();
            }
        }

        private void HandleSpeakerAttached(AudioSource audioSource)
        {
            if (Features.AudioEnabled)
            {
                audioManager?.Initialize(audioSource);
                if (Features.AutoLoadTracks)
                {
                    audioManager?.LoadTracks();
                }
            }
            OnSpeakerAttached?.Invoke(audioSource);
        }

        private void HandleSpeakerDetached()
        {
            LoggingSystem.Debug("Speaker detached", "SystemManager");
            audioManager?.Reset();
        }

        public void Update()
        {
            if (!isInitialized || !Features.AudioEnabled) return;
            audioManager?.Update();
        }

        public void Shutdown()
        {
            try
            {
                LoggingSystem.Info("Shutting down system manager", "SystemManager");

                // Cleanup event handlers
                if (playerAttachment != null)
                {
                    playerAttachment.OnSpeakerAttached -= HandleSpeakerAttached;
                    playerAttachment.OnSpeakerDetached -= HandleSpeakerDetached;
                }

                // Shutdown components
                headphoneManager?.Shutdown();
                audioManager?.Shutdown();
                PlayerManager.Shutdown();

                isInitialized = false;
                LoggingSystem.Info("System manager shutdown completed", "SystemManager");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"System shutdown failed: {ex.Message}", "SystemManager");
            }
        }

        #endregion

        #region Audio API

        public void Play()
        {
            if (!AreHeadphonesAttached())
            {
                LoggingSystem.Warning("Cannot play - headphones not attached", "SystemManager");
                return;
            }
            audioManager?.Play();
        }

        public void Pause() => audioManager?.Pause();

        public void TogglePlayPause()
        {
            if (!AreHeadphonesAttached())
            {
                LoggingSystem.Warning("Cannot toggle playback - headphones not attached", "SystemManager");
                return;
            }
            audioManager?.TogglePlayPause();
        }

        public void SetVolume(float volume) => audioManager?.SetVolume(volume);

        public void NextTrack()
        {
            if (!AreHeadphonesAttached()) return;
            audioManager?.NextTrack();
        }

        public void PreviousTrack()
        {
            if (!AreHeadphonesAttached()) return;
            audioManager?.PreviousTrack();
        }

        public void PlayTrack(int index)
        {
            if (!AreHeadphonesAttached()) return;
            audioManager?.PlayTrack(index);
        }

        public void SeekToTime(float time) => audioManager?.SeekToTime(time);
        public void SeekToProgress(float progress) => audioManager?.SeekToProgress(progress);
        public void ReloadTracks() => audioManager?.LoadTracks();

        #endregion

        #region Audio Properties

        public bool IsPlaying => audioManager?.IsPlaying ?? false;
        public float CurrentVolume => audioManager?.CurrentVolume ?? 0.5f;
        public int GetTrackCount() => audioManager?.GetTrackCount() ?? 0;
        public bool IsAudioReady() => audioManager?.IsAudioReady() ?? false;
        public float CurrentTime => audioManager?.CurrentTime ?? 0f;
        public float TotalTime => audioManager?.TotalTime ?? 0f;
        public float Progress => audioManager?.Progress ?? 0f;
        public int CurrentTrackIndex => audioManager?.CurrentTrackIndex ?? 0;
        public string GetCurrentTrackInfo() => audioManager?.GetCurrentTrackInfo() ?? "No Track";
        public string GetCurrentArtistInfo() => audioManager?.GetCurrentArtistInfo() ?? "Unknown Artist";
        public List<(string title, string artist)> GetAllTracks() => audioManager?.GetAllTracks() ?? new List<(string, string)>();

        public RepeatMode RepeatMode
        {
            get => audioManager?.RepeatMode ?? RepeatMode.None;
            set { if (audioManager != null) audioManager.RepeatMode = value; }
        }

        #endregion

        #region Headphone API

        public bool AttachHeadphones()
        {
            bool success = headphoneManager?.AttachHeadphones() ?? false;
            if (success)
            {
                playerAttachment?.TriggerManualAttachment();
                if (Features.AutoLoadTracks)
                {
                    audioManager?.LoadTracks();
                }
            }
            return success;
        }

        public bool RemoveHeadphones()
        {
            LoggingSystem.Debug("Removing headphones", "SystemManager");
            // Stop music first
            LoggingSystem.Debug($"Checking if music is playing: {IsPlaying}", "SystemManager");
            if (IsPlaying) audioManager?.Pause();
            
            // Reset audio system
            LoggingSystem.Debug("Resetting audio system", "SystemManager");
            audioManager?.Reset();
            
            // Detach speaker
            try {
                LoggingSystem.Debug("Detaching speaker", "SystemManager");
                playerAttachment?.DetachSpeaker();
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to detach speaker: {ex.Message}", "SystemManager");
                return false;
            }
            
            // Remove headphones
            LoggingSystem.Debug("Removing headphones", "SystemManager");
            bool removed = headphoneManager?.RemoveHeadphones() ?? false;
            LoggingSystem.Debug($"Headphone removal result: {removed}", "SystemManager");
            return removed;
        }

        public bool ToggleHeadphones()
        {
            LoggingSystem.Debug("Toggling headphones", "SystemManager");
            bool wasAttached = AreHeadphonesAttached();
            bool success = headphoneManager?.ToggleHeadphones() ?? false;
            LoggingSystem.Debug($"Toggle headphones result: {success}", "SystemManager");
            
            if (success)
            {
                LoggingSystem.Debug("Headphones toggled", "SystemManager");
                bool nowAttached = AreHeadphonesAttached();
                if (nowAttached && !wasAttached)
                {
                    LoggingSystem.Debug("Attaching headphones", "SystemManager");
                    // Attaching headphones
                    playerAttachment?.TriggerManualAttachment();
                    if (Features.AutoLoadTracks)
                    {
                        audioManager?.LoadTracks();
                    }
                }
                else if (!nowAttached && wasAttached)
                {
                    LoggingSystem.Debug("Detaching headphones", "SystemManager");
                    // Detaching headphones
                    bool removed = RemoveHeadphones();
                    LoggingSystem.Debug($"Remove headphones result: {removed}", "SystemManager");
                    return removed;
                }
            }
            
            return success;
        }

        public bool AreHeadphonesAttached() => headphoneManager?.AreHeadphonesAttached ?? false;
        public string GetHeadphoneStatus() => headphoneManager?.GetStatus() ?? "Headphone system not initialized";

        #endregion

        #region Player Attachment API

        public void TriggerManualAttachment() => playerAttachment?.TriggerManualAttachment();
        public string GetAttachmentStatus() => playerAttachment?.GetAttachmentStatus() ?? "Initializing...";

        #endregion

        #region Configuration API

        public T? GetSetting<T>(string key, T? defaultValue = default(T))
        {
            if (settings.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)value;
                }
                catch (Exception ex)
                {
                    LoggingSystem.Warning($"Failed to cast setting '{key}': {ex.Message}", "SystemManager");
                }
            }
            return defaultValue;
        }

        public void SetSetting<T>(string key, T value)
        {
            try
            {
                if (value == null) throw new ArgumentNullException(nameof(value));
                settings[key] = value;
                LoggingSystem.Debug($"Setting '{key}' set to '{value}'", "SystemManager");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to set setting '{key}': {ex.Message}", "SystemManager");
            }
        }

        #endregion

        #region Status & Properties

        public bool IsInitialized => isInitialized;

        public string GetSystemStatus()
        {
            if (!isInitialized) return "System not initialized";

            var statuses = new List<string>();

            if (!Features.HeadphonesEnabled && !Features.AudioEnabled)
            {
                statuses.Add("All features disabled");
            }
            else
            {
                if (Features.AudioEnabled)
                    statuses.Add($"Audio: {(audioManager?.IsAudioReady() == true ? "Ready" : "Not Ready")}");

                if (Features.HeadphonesEnabled)
                    statuses.Add($"Headphones: {GetHeadphoneStatus()}");
            }

            statuses.Add($"Player: {GetAttachmentStatus()}");
            return string.Join(" | ", statuses);
        }

        #endregion
    }
} 