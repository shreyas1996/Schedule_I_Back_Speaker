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
    /// Orchestrates audio controller and track loading
    /// </summary>
    public class AudioManager
    {
        private readonly AudioController audioController;
        private readonly TrackLoader trackLoader;
        private bool isInitialized = false;
        
        /// <summary>
        /// Event fired when tracks are reloaded
        /// </summary>
        public event Action? OnTracksReloaded;

        /// <summary>
        /// Initialize audio manager
        /// </summary>
        public AudioManager()
        {
            audioController = new AudioController();
            trackLoader = new TrackLoader();
            
            // Wire up events
            trackLoader.OnTracksLoaded += OnTracksLoaded;
            audioController.OnTracksChanged += () => OnTracksReloaded?.Invoke();
            
            LoggingSystem.Info("AudioManager initialized", "Audio");
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

            LoggingSystem.Info("Initializing audio system", "Audio");

            try
            {
                audioController.Initialize(audioSource);
                isInitialized = true;
                
                LoggingSystem.Info("Audio system initialized successfully", "Audio");
                return true;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Exception during audio initialization: {ex.Message}", "Audio");
                return false;
            }
        }

        /// <summary>
        /// Load tracks from jukebox
        /// </summary>
        public void LoadTracks()
        {
            if (!isInitialized)
            {
                LoggingSystem.Warning("Audio system not initialized", "Audio");
                return;
            }

            LoggingSystem.Info("Loading tracks", "Audio");
            trackLoader.LoadJukeboxTracks();
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
                audioController.Update();
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Exception during audio update: {ex.Message}", "Audio");
            }
        }

        /// <summary>
        /// Handle tracks loaded event
        /// </summary>
        private void OnTracksLoaded(List<AudioClip> tracks, List<(string title, string artist)> trackInfo)
        {
            audioController.SetTracks(tracks, trackInfo);
            LoggingSystem.Info($"Audio manager loaded {tracks.Count} tracks", "Audio");
        }

        // Audio Control API
        public void Play() => audioController?.Play();
        public void Pause() => audioController?.Pause();
        public void TogglePlayPause() => audioController?.TogglePlayPause();
        public void SetVolume(float volume) => audioController?.SetVolume(volume);
        public void NextTrack() => audioController?.NextTrack();
        public void PreviousTrack() => audioController?.PreviousTrack();
        public void PlayTrack(int index) => audioController?.PlayTrack(index);
        public void SeekToTime(float time) => audioController?.SeekToTime(time);
        public void SeekToProgress(float progress) => audioController?.SeekToProgress(progress);

        // Audio State Properties
        public bool IsPlaying => audioController?.IsPlaying ?? false;
        public float CurrentVolume => audioController?.CurrentVolume ?? 0.5f;
        public int GetTrackCount() => audioController?.GetTrackCount() ?? 0;
        public float CurrentTime => audioController?.CurrentTime ?? 0f;
        public float TotalTime => audioController?.TotalTime ?? 0f;
        public float Progress => audioController?.Progress ?? 0f;
        public int CurrentTrackIndex => audioController?.CurrentTrackIndex ?? 0;
        public bool IsAudioReady() => audioController?.IsAudioReady() ?? false;
        
        public RepeatMode RepeatMode 
        { 
            get => audioController?.RepeatMode ?? RepeatMode.None;
            set { if (audioController != null) audioController.RepeatMode = value; }
        }

        public string GetCurrentTrackInfo() => audioController?.GetCurrentTrackInfo() ?? "No Track";
        public string GetCurrentArtistInfo() => audioController?.GetCurrentArtistInfo() ?? "Unknown Artist";
        public List<(string title, string artist)> GetAllTracks() => audioController?.GetAllTracks() ?? new List<(string, string)>();

        /// <summary>
        /// Get system status
        /// </summary>
        public string GetStatus()
        {
            if (!FeatureFlags.Audio.Enabled)
                return "Audio feature disabled";
                
            if (!isInitialized)
                return "Audio system not initialized";
                
            var trackCount = GetTrackCount();
            if (trackCount == 0)
                return "No tracks loaded";
                
            var currentTrack = GetCurrentTrackInfo();
            var status = IsPlaying ? "Playing" : "Paused";
            return $"{status}: {currentTrack} ({CurrentTrackIndex + 1}/{trackCount})";
        }

        /// <summary>
        /// Shutdown audio manager
        /// </summary>
        public void Shutdown()
        {
            LoggingSystem.Info("Shutting down audio manager", "Audio");
            
            try
            {
                // Pause any playing audio
                Pause();
                
                // Unsubscribe from events
                if (trackLoader != null)
                    trackLoader.OnTracksLoaded -= OnTracksLoaded;
                
                isInitialized = false;
                LoggingSystem.Info("Audio manager shutdown completed", "Audio");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Exception during audio shutdown: {ex.Message}", "Audio");
            }
        }

        /// <summary>
        /// Get audio controller for direct access (if needed)
        /// </summary>
        public AudioController GetAudioController() => audioController;

        /// <summary>
        /// Get track loader for direct access (if needed)
        /// </summary>
        public TrackLoader GetTrackLoader() => trackLoader;
    }
} 