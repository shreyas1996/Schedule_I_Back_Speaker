using System;
using System.Collections.Generic;
using BackSpeakerMod.Core.Modules;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.Core.System
{
    /// <summary>
    /// Manages public API by delegating to appropriate system components
    /// </summary>
    public class APIManager
    {
        private readonly SystemComponents components;

        /// <summary>
        /// Initialize API manager with system components
        /// </summary>
        public APIManager(SystemComponents systemComponents)
        {
            components = systemComponents ?? throw new global::System.ArgumentNullException(nameof(systemComponents));
            LoggingSystem.Info("APIManager initialized", "System");
        }

        // Audio Control API
        public void Play() => components.AudioManager?.Play();
        public void Pause() => components.AudioManager?.Pause();
        public void TogglePlayPause() => components.AudioManager?.TogglePlayPause();
        public void SetVolume(float volume) => components.AudioManager?.SetVolume(volume);
        public void NextTrack() => components.AudioManager?.NextTrack();
        public void PreviousTrack() => components.AudioManager?.PreviousTrack();
        public void PlayTrack(int index) => components.AudioManager?.PlayTrack(index);
        public void SeekToTime(float time) => components.AudioManager?.SeekToTime(time);
        public void SeekToProgress(float progress) => components.AudioManager?.SeekToProgress(progress);
        public void ReloadTracks() => components.AudioManager?.LoadTracks();

        // Audio Properties API
        public bool IsPlaying => components.AudioManager?.IsPlaying ?? false;
        public float CurrentVolume => components.AudioManager?.CurrentVolume ?? 0.5f;
        public int GetTrackCount() => components.AudioManager?.GetTrackCount() ?? 0;
        public bool IsAudioReady() => components.AudioManager?.IsAudioReady() ?? false;
        public float CurrentTime => components.AudioManager?.CurrentTime ?? 0f;
        public float TotalTime => components.AudioManager?.TotalTime ?? 0f;
        public float Progress => components.AudioManager?.Progress ?? 0f;
        public int CurrentTrackIndex => components.AudioManager?.CurrentTrackIndex ?? 0;

        // Audio Track Info API
        public string GetCurrentTrackInfo() => components.AudioManager?.GetCurrentTrackInfo() ?? "No Track";
        public string GetCurrentArtistInfo() => components.AudioManager?.GetCurrentArtistInfo() ?? "Unknown Artist";
        public List<(string title, string artist)> GetAllTracks() => components.AudioManager?.GetAllTracks() ?? new List<(string, string)>();

        // Repeat Mode API
        public RepeatMode RepeatMode
        {
            get => components.AudioManager?.RepeatMode ?? RepeatMode.None;
            set { if (components.AudioManager != null) components.AudioManager.RepeatMode = value; }
        }

        // Player Attachment API
        public void TriggerManualAttachment() => components.PlayerAttachment?.TriggerManualAttachment();
        public string GetAttachmentStatus() => components.PlayerAttachment?.GetAttachmentStatus() ?? "Initializing...";

        // Headphone API
        public bool AttachHeadphones() => components.HeadphoneManager?.AttachHeadphones() ?? false;
        public void RemoveHeadphones() => components.HeadphoneManager?.RemoveHeadphones();
        public bool ToggleHeadphones() => components.HeadphoneManager?.ToggleHeadphones() ?? false;
        public bool AreHeadphonesAttached() => components.HeadphoneManager?.AreHeadphonesAttached ?? false;
        public string GetHeadphoneStatus() => components.HeadphoneManager?.GetStatus() ?? "Headphone system not initialized";

        // Testing API
        public bool AttachTestCube() => components.TestingManager?.CreateTestCube() ?? false;
        public bool AttachGlowingSphere() => components.TestingManager?.CreateGlowingSphere() ?? false;
        public bool ToggleGlowingSphere() => components.TestingManager?.ToggleGlowingSphere() ?? false;
        public bool ToggleTestCube() => components.TestingManager?.ToggleTestCube() ?? false;
        public void DestroyAllTestObjects() => components.TestingManager?.DestroyAllTestObjects();
        public string GetTestingStatus() => components.TestingManager?.GetStatus() ?? "Testing system not initialized";

        // Placement API
        public bool IsInPlacementMode() => components.PlacementManager?.IsInPlacementMode ?? false;
        public string GetPlacementStatus() => components.PlacementManager?.GetStatus() ?? "Placement system not initialized";

        /// <summary>
        /// Toggle placement mode specifically for headphones
        /// </summary>
        public void TogglePlacementMode()
        {
            if (components.PlacementManager == null || components.HeadphoneManager == null)
            {
                LoggingSystem.Warning("Placement or headphone manager not available", "API");
                return;
            }

            try
            {
                // Check if we have headphone prefab loaded
                var loader = components.HeadphoneManager.GetLoader();
                if (loader == null || !loader.IsLoaded || loader.HeadphonePrefab == null)
                {
                    LoggingSystem.Warning("No headphone prefab available for placement", "API");
                    return;
                }

                var headphonePrefab = loader.HeadphonePrefab;

                // Toggle placement mode with headphone prefab
                components.PlacementManager.TogglePlacement(
                    headphonePrefab,
                    (position, rotation) => 
                    {
                        // Place headphone instance at specified location
                        var instance = UnityEngine.Object.Instantiate(headphonePrefab, position, rotation);
                        LoggingSystem.Info($"Headphones placed at {position}", "API");
                        return instance;
                    }
                );
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Error during placement toggle: {ex.Message}", "API");
            }
        }
    }
} 