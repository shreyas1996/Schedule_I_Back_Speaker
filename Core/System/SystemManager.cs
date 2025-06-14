using System;
using System.Collections.Generic;
using UnityEngine;
using BackSpeakerMod.Core.Features.Headphones.Managers;
using BackSpeakerMod.Core.Features.Audio;
using BackSpeakerMod.Core.Features.Player;
using BackSpeakerMod.Core.Modules;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Utils;

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
                    // Load default session (Jukebox) to ensure immediate music availability
                    // Other sessions will load on-demand when users switch tabs
                    audioManager?.LoadDefaultSession();
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
            if (!isInitialized) return;
            
            // Update headphone manager for camera tracking
            headphoneManager?.Update();
            
            // Update audio manager if audio is enabled
            if (Features.AudioEnabled)
            {
                audioManager?.Update();
            }
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
        public YouTubeDownloadManager GetDownloadManager() => audioManager?.GetDownloadManager();
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

        #region Music Source Management

        public void SetMusicSource(MusicSourceType sourceType)
        {
            try
            {
                audioManager?.SetActiveSession(sourceType);
                LoggingSystem.Info($"Active session changed to: {sourceType}", "SystemManager");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to set active session to {sourceType}: {ex.Message}", "SystemManager");
            }
        }

        public MusicSourceType GetCurrentMusicSource()
        {
            try
            {
                return audioManager?.GetCurrentMusicSource() ?? MusicSourceType.Jukebox;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to get current music source: {ex.Message}", "SystemManager");
                return MusicSourceType.Jukebox;
            }
        }

        public List<(MusicSourceType type, string name, bool available)> GetAvailableMusicSources()
        {
            try
            {
                // Return available music sources
                return new List<(MusicSourceType, string, bool)>
                {
                    (MusicSourceType.Jukebox, "In-Game Jukebox", true),
                    (MusicSourceType.LocalFolder, "Local Music", true),
                    (MusicSourceType.YouTube, "YouTube Music", true)
                };
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to get available music sources: {ex.Message}", "SystemManager");
                return new List<(MusicSourceType, string, bool)>();
            }
        }

        public void LoadTracksFromCurrentSource()
        {
            try
            {
                var sessionManager = audioManager?.GetSessionManager();
                if (sessionManager != null)
                {
                    var activeSession = sessionManager.GetActiveSession();
                    sessionManager.LoadTracksForSession(activeSession.SourceType);
                    LoggingSystem.Info($"Loading tracks for active session: {activeSession.DisplayName}", "SystemManager");
                }
                else
                {
                    LoggingSystem.Warning("Session manager not available", "SystemManager");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to load tracks from current source: {ex.Message}", "SystemManager");
            }
        }

        public void ForceLoadFromSource(MusicSourceType sourceType)
        {
            try
            {
                var sessionManager = audioManager?.GetSessionManager();
                if (sessionManager != null)
                {
                    // First set the active session to match the source type
                    sessionManager.SetActiveSession(sourceType);
                    
                    // Then force load tracks from that source (bypassing availability checks)
                    sessionManager.ForceLoadTracksForSession(sourceType);
                    
                    LoggingSystem.Info($"Force loading tracks from source: {sourceType}", "SystemManager");
                }
                else
                {
                    LoggingSystem.Warning("Session manager not available", "SystemManager");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to force load tracks from source {sourceType}: {ex.Message}", "SystemManager");
            }
        }

        public void AddTrackToSession(MusicSourceType sessionType, AudioClip audioClip, string title, string artist)
        {
            try
            {
                var sessionManager = audioManager?.GetSessionManager();
                if (sessionManager != null)
                {
                    sessionManager.AddTrackToSession(sessionType, audioClip, title, artist);
                    LoggingSystem.Info($"Added track '{title}' to {sessionType} session", "SystemManager");
                }
                else
                {
                    LoggingSystem.Warning("Session manager not available", "SystemManager");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to add track to {sessionType} session: {ex.Message}", "SystemManager");
            }
        }

        public void AddTracksToSession(MusicSourceType sessionType, List<AudioClip> audioClips, List<(string title, string artist)> trackInfo)
        {
            if (!Features.AudioEnabled)
            {
                LoggingSystem.Warning("Audio feature disabled - cannot add tracks to session", "SystemManager");
                return;
            }

            if (!isInitialized)
            {
                LoggingSystem.Warning("System not initialized - cannot add tracks to session", "SystemManager");
                return;
            }

            LoggingSystem.Info($"Adding {audioClips.Count} tracks to session: {sessionType}", "SystemManager");
            audioManager?.GetSessionManager()?.AddTracksToSession(sessionType, audioClips, trackInfo);
        }

        public bool AddYouTubeSong(SongDetails songDetails) => audioManager?.GetSessionManager()?.AddYouTubeSong(songDetails) ?? false;
        public bool RemoveYouTubeSong(string url) => audioManager?.GetSessionManager()?.RemoveYouTubeSong(url) ?? false;
        public bool ContainsYouTubeSong(string url) => audioManager?.GetSessionManager()?.ContainsYouTubeSong(url) ?? false;
        public void ClearYouTubePlaylist() => audioManager?.GetSessionManager()?.ClearYouTubePlaylist();
        public void LoadYouTubePlaylist(List<SongDetails> playlistSongs) => audioManager?.GetSessionManager()?.LoadYouTubePlaylist(playlistSongs);

        public AudioSession GetSession(MusicSourceType sessionType) => audioManager?.GetSessionManager()?.GetSession(sessionType);

        #endregion

        #region Headphone Management

        public bool AttachHeadphones()
        {
            try
            {
                if (!AreHeadphonesAttached())
                {
                    LoggingSystem.Info("Attempting to attach headphones", "SystemManager");
                    bool success = headphoneManager?.AttachHeadphones() ?? false;
                    if (success)
                    {
                        LoggingSystem.Info("✓ Headphones attached successfully", "SystemManager");
                    }
                    else
                    {
                        LoggingSystem.Warning("Failed to attach headphones", "SystemManager");
                    }
                    return success;
                }
                else
                {
                    LoggingSystem.Info("Headphones already attached", "SystemManager");
                    return true;
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"AttachHeadphones failed: {ex.Message}", "SystemManager");
                return false;
            }
        }

        public bool RemoveHeadphones()
        {
            try
            {
                if (AreHeadphonesAttached())
                {
                    LoggingSystem.Info("Removing headphones", "SystemManager");
                    bool removed = headphoneManager?.RemoveHeadphones() ?? false;
                    if (removed)
                    {
                        LoggingSystem.Info("✓ Headphones removed", "SystemManager");
                    }
                    return removed;
                }
                else
                {
                    LoggingSystem.Info("No headphones to remove", "SystemManager");
                    return true;
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"RemoveHeadphones failed: {ex.Message}", "SystemManager");
                return false;
            }
        }

        public bool ToggleHeadphones()
        {
            try
            {
                bool success = headphoneManager?.ToggleHeadphones() ?? false;
                string action = AreHeadphonesAttached() ? "attached" : "removed";
                LoggingSystem.Info($"Headphones {action}", "SystemManager");
                return success;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"ToggleHeadphones failed: {ex.Message}", "SystemManager");
                return false;
            }
        }

        public bool AreHeadphonesAttached() => headphoneManager?.AreHeadphonesAttached ?? false;
        public string GetHeadphoneStatus() => headphoneManager?.GetStatus() ?? "Headphone system not initialized";
        public string GetDetailedHeadphoneStatus() => headphoneManager?.GetDetailedStatus() ?? "Headphone system not initialized";
        public string GetHeadphoneCameraInfo() => headphoneManager?.GetCameraInfo() ?? "Camera info not available";
        public void ForceUpdateHeadphoneVisibility() => headphoneManager?.ForceUpdateVisibility();

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