using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Modules;

namespace BackSpeakerMod.Core.Features.Audio.Managers
{
    /// <summary>
    /// High-level manager for audio functionality
    /// Now uses AudioSessionManager for individualized tab sessions
    /// </summary>
    public class AudioManager
    {
        private readonly AudioSessionManager sessionManager;
        private readonly GameAudioManager gameAudioManager;
        private bool isInitialized = false;
        
        /// <summary>
        /// Event fired when tracks are reloaded
        /// </summary>
        public event Action? OnTracksReloaded;

        /// <summary>
        /// Initialize audio manager with session-based approach
        /// </summary>
        public AudioManager()
        {
            sessionManager = new AudioSessionManager();
            gameAudioManager = new GameAudioManager();
            
            // Wire up events
            sessionManager.OnTracksReloaded += () => OnTracksReloaded?.Invoke();
            
            LoggingSystem.Info("AudioManager initialized with session management", "Audio");
        }

        /// <summary>
        /// Initialize the audio system with an audio source
        /// </summary>
        public bool Initialize(AudioSource audioSource)
        {
            if (!FeatureFlags.Audio.Enabled)
            {
                LoggingSystem.Info("Audio feature is disabled", "Audio");
                return false;
            }

            LoggingSystem.Info("Initializing audio system with sessions", "Audio");

            try
            {
                isInitialized = sessionManager.Initialize(audioSource);
                
                if (isInitialized)
                {
                    // Initialize game audio manager
                    gameAudioManager.Initialize();
                    LoggingSystem.Info("Audio system with sessions initialized successfully", "Audio");
                }
                
                return isInitialized;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Exception during audio initialization: {ex.Message}", "Audio");
                return false;
            }
        }

        /// <summary>
        /// Reset audio system when speaker is detached
        /// </summary>
        public void Reset()
        {
            // Restore game audio before resetting
            gameAudioManager?.RestoreGameAudio();
            sessionManager?.Reset();
            isInitialized = false;
        }

        /// <summary>
        /// Load tracks for current active session
        /// </summary>
        public void LoadTracks()
        {
            if (!isInitialized)
            {
                LoggingSystem.Warning("Audio system not initialized", "Audio");
                return;
            }

            var activeSession = sessionManager.GetActiveSession();
            LoggingSystem.Info($"Loading tracks for active session: {activeSession.DisplayName}", "Audio");
            sessionManager.LoadTracksForSession(activeSession.SourceType);
        }

        /// <summary>
        /// Load tracks for the default session (Jukebox) when system initializes
        /// This ensures users have music available immediately when headphones are attached
        /// </summary>
        public void LoadDefaultSession()
        {
            if (!isInitialized)
            {
                LoggingSystem.Warning("Audio system not initialized", "Audio");
                return;
            }

            LoggingSystem.Info("Loading default session (Jukebox) for immediate music availability", "Audio");
            sessionManager.LoadTracksForSession(MusicSourceType.Jukebox);
        }

        /// <summary>
        /// Update audio system (call from main update loop)
        /// </summary>
        public void Update()
        {
            if (!isInitialized || !FeatureFlags.Audio.Enabled)
                return;

            try
            {
                sessionManager.Update();
                gameAudioManager.Update();
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Exception during audio update: {ex.Message}", "Audio");
            }
        }

        // Audio Control API - delegates to session manager
        public void Play()
        {
            if (sessionManager != null && !sessionManager.IsPlaying)
            {
                sessionManager.Play();
                gameAudioManager?.ReduceGameAudio();
                LoggingSystem.Info("Audio resumed", "Audio");
            }
        }
        
        public void Pause()
        {
            if (sessionManager != null && sessionManager.IsPlaying)
            {
                sessionManager.Pause();
                gameAudioManager?.RestoreGameAudio();
                LoggingSystem.Info("Audio paused", "Audio");
            }
        }
        
        public void TogglePlayPause()
        {
            if (sessionManager != null)
            {
                if (sessionManager.IsPlaying)
                {
                    Pause();
                    gameAudioManager?.RestoreGameAudio();
                }
                else
                {
                    Play();
                    gameAudioManager?.ReduceGameAudio();
                }
            }
        }
        public void SetVolume(float volume) => sessionManager?.SetVolume(volume);
        public void NextTrack()
        {
            if (sessionManager != null && sessionManager.IsPlaying)
            {
                sessionManager.NextTrack();
                gameAudioManager?.ReduceGameAudio();
            }
        }
        
        public void PreviousTrack()
        {
            if (sessionManager != null && sessionManager.IsPlaying)
            {
                sessionManager.PreviousTrack();
                gameAudioManager?.ReduceGameAudio();
            }
        }
        
        public void PlayTrack(int index)
        {
            if (sessionManager != null)
            {
                sessionManager.PlayTrack(sessionManager.GetActiveSession().SourceType, index);
                gameAudioManager?.ReduceGameAudio();
            }
        }
        public void SeekToTime(float time) => sessionManager?.SeekToTime(time);
        public void SeekToProgress(float progress) => sessionManager?.SeekToProgress(progress);

        // Audio State Properties - delegates to session manager
        public bool IsPlaying => sessionManager?.IsPlaying ?? false;
        public float CurrentVolume => sessionManager?.CurrentVolume ?? 0.5f;
        public int GetTrackCount() => sessionManager?.GetTrackCount() ?? 0;
        public float CurrentTime => sessionManager?.CurrentTime ?? 0f;
        public float TotalTime => sessionManager?.TotalTime ?? 0f;
        public float Progress => sessionManager?.Progress ?? 0f;
        public int CurrentTrackIndex => sessionManager?.CurrentTrackIndex ?? 0;
        public bool IsAudioReady() => sessionManager?.IsAudioReady() ?? false;
        public YouTubeDownloadManager GetDownloadManager() => sessionManager?.GetDownloadManager();
        
        public RepeatMode RepeatMode
        {
            get => sessionManager?.RepeatMode ?? RepeatMode.None;
            set { if (sessionManager != null) sessionManager.RepeatMode = value; }
        }

        public string GetCurrentTrackInfo() => sessionManager?.GetCurrentTrackInfo() ?? "No Track";
        public string GetCurrentArtistInfo() => sessionManager?.GetCurrentArtistInfo() ?? "Unknown Artist";
        public List<(string title, string artist)> GetAllTracks() => sessionManager?.GetAllTracks() ?? new List<(string, string)>();

        // Session Management API
        public void SetActiveSession(MusicSourceType sessionType) => sessionManager?.SetActiveSession(sessionType);
        public void LoadTracksForSession(MusicSourceType sessionType) => sessionManager?.LoadTracksForSession(sessionType);
        public MusicSourceType GetCurrentMusicSource() => sessionManager?.GetCurrentMusicSource() ?? MusicSourceType.Jukebox;

        /// <summary>
        /// Get system status
        /// </summary>
        public string GetStatus()
        {
            if (!FeatureFlags.Audio.Enabled)
                return "Audio feature disabled";
                
            if (!isInitialized)
                return "Audio system not initialized";
                
            var sessionStatus = sessionManager?.GetStatus() ?? "Session manager not available";
            var gameAudioStatus = gameAudioManager?.GetStatus() ?? "Game audio manager not available";
            
            return $"{sessionStatus}\n{gameAudioStatus}";
        }

        /// <summary>
        /// Shutdown audio manager
        /// </summary>
        public void Shutdown()
        {
            LoggingSystem.Info("Shutting down audio manager", "Audio");
            
            try
            {
                gameAudioManager?.Shutdown();
                sessionManager?.Shutdown();
                isInitialized = false;
                LoggingSystem.Info("Audio manager shutdown completed", "Audio");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Exception during audio shutdown: {ex.Message}", "Audio");
            }
        }

        /// <summary>
        /// Get session manager for direct access (if needed)
        /// </summary>
        public AudioSessionManager GetSessionManager() => sessionManager;
    }
} 