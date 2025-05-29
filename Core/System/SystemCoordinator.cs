using System;
using System.Collections.Generic;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Modules;
using BackSpeakerMod.Core.System;
using System.Threading.Tasks;

namespace BackSpeakerMod.Core.System
{
    /// <summary>
    /// Main system coordinator using granular architecture
    /// Replaces the monolithic BackSpeakerManager
    /// </summary>
    public class SystemCoordinator
    {
        private readonly SystemComponents components;
        private readonly SystemInitializer initializer;
        private readonly APIManager apiManager;
        private bool useAsyncInitialization = true;

        /// <summary>
        /// Event to notify UI when tracks are reloaded
        /// </summary>
        public Action OnTracksReloaded
        {
            get => components.OnTracksReloaded;
            set => components.OnTracksReloaded = value;
        }

        /// <summary>
        /// Whether system is initialized
        /// </summary>
        public bool IsInitialized => initializer.IsInitialized;

        /// <summary>
        /// Initialize system coordinator with async support
        /// </summary>
        public SystemCoordinator()
        {
            LoggingSystem.Info("SystemCoordinator: Initializing with granular architecture", "System");

            // Create system components
            components = SystemComponents.Create();

            // Create initializer and API manager
            initializer = new SystemInitializer(components);
            apiManager = new APIManager(components);

            // Start async initialization
            _ = InitializeAsync();
        }

        /// <summary>
        /// Async initialization for better asset loading
        /// </summary>
        private async Task InitializeAsync()
        {
            if (useAsyncInitialization)
            {
                LoggingSystem.Info("Starting async initialization", "System");
                await initializer.InitializeAsync();
            }
            else
            {
                LoggingSystem.Info("Falling back to sync initialization", "System");
                initializer.Initialize();
            }
        }

        /// <summary>
        /// Update all systems
        /// </summary>
        public void Update()
        {
            if (!initializer.IsInitialized)
                return;

            try
            {
                // Update feature managers
                components.AudioManager?.Update();
                components.PlacementManager?.Update();
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Exception during system update: {ex.Message}", "System");
            }
        }

        // Public API - Audio Control
        public void Play() => apiManager.Play();
        public void Pause() => apiManager.Pause();
        public void TogglePlayPause() => apiManager.TogglePlayPause();
        public void SetVolume(float volume) => apiManager.SetVolume(volume);
        public void NextTrack() => apiManager.NextTrack();
        public void PreviousTrack() => apiManager.PreviousTrack();
        public void PlayTrack(int index) => apiManager.PlayTrack(index);
        public void SeekToTime(float time) => apiManager.SeekToTime(time);
        public void SeekToProgress(float progress) => apiManager.SeekToProgress(progress);
        public void ReloadTracks() => apiManager.ReloadTracks();

        // Public API - Audio Properties
        public bool IsPlaying => apiManager.IsPlaying;
        public float CurrentVolume => apiManager.CurrentVolume;
        public int GetTrackCount() => apiManager.GetTrackCount();
        public bool IsAudioReady() => apiManager.IsAudioReady();
        public float CurrentTime => apiManager.CurrentTime;
        public float TotalTime => apiManager.TotalTime;
        public float Progress => apiManager.Progress;
        public int CurrentTrackIndex => apiManager.CurrentTrackIndex;
        public string GetCurrentTrackInfo() => apiManager.GetCurrentTrackInfo();
        public string GetCurrentArtistInfo() => apiManager.GetCurrentArtistInfo();
        public List<(string title, string artist)> GetAllTracks() => apiManager.GetAllTracks();

        public RepeatMode RepeatMode
        {
            get => apiManager.RepeatMode;
            set => apiManager.RepeatMode = value;
        }

        // Public API - Player Attachment
        public void TriggerManualAttachment() => apiManager.TriggerManualAttachment();
        public string GetAttachmentStatus() => apiManager.GetAttachmentStatus();

        // Public API - Headphones
        public bool AttachHeadphones() => apiManager.AttachHeadphones();
        public void RemoveHeadphones() => apiManager.RemoveHeadphones();
        public bool ToggleHeadphones() => apiManager.ToggleHeadphones();
        public bool AreHeadphonesAttached() => apiManager.AreHeadphonesAttached();
        public string GetHeadphoneStatus() => apiManager.GetHeadphoneStatus();

        // Public API - Spheres
        public bool AttachSphere() => apiManager.AttachSphere();
        public bool DetachSphere() => apiManager.DetachSphere();
        public bool ToggleSphere() => apiManager.ToggleSphere();
        public bool IsSphereAttached() => apiManager.IsSphereAttached();
        public string GetSphereStatus() => apiManager.GetSphereStatus();

        // Public API - Testing
        public bool AttachTestCube() => apiManager.AttachTestCube();
        public bool AttachGlowingSphere() => apiManager.AttachGlowingSphere();
        public bool ToggleGlowingSphere() => apiManager.ToggleGlowingSphere();
        public bool ToggleTestCube() => apiManager.ToggleTestCube();
        public void DestroyAllTestObjects() => apiManager.DestroyAllTestObjects();
        public string GetTestingStatus() => apiManager.GetTestingStatus();

        // Public API - Placement
        public void TogglePlacementMode() => apiManager.TogglePlacementMode();
        public void ToggleSpherePlacementMode() => apiManager.ToggleSpherePlacementMode();
        public bool IsInPlacementMode() => apiManager.IsInPlacementMode();
        public string GetPlacementStatus() => apiManager.GetPlacementStatus();

        /// <summary>
        /// Get comprehensive system status
        /// </summary>
        public string GetSystemStatus()
        {
            if (!initializer.IsInitialized)
                return "System not initialized";

            var statuses = new List<string>();

            if (!FeatureFlags.Headphones.Enabled && !FeatureFlags.Spheres.Enabled && !FeatureFlags.Testing.Enabled && 
                !FeatureFlags.Placement.Enabled && !FeatureFlags.Audio.Enabled)
            {
                statuses.Add("All features disabled");
            }
            else
            {
                if (FeatureFlags.Audio.Enabled)
                    statuses.Add($"Audio: {components.AudioManager?.GetStatus() ?? "Not initialized"}");

                if (FeatureFlags.Headphones.Enabled)
                    statuses.Add($"Headphones: {GetHeadphoneStatus()}");

                if (FeatureFlags.Spheres.Enabled)
                    statuses.Add($"Spheres: {GetSphereStatus()}");

                if (FeatureFlags.Testing.Enabled)
                    statuses.Add($"Testing: {GetTestingStatus()}");

                if (FeatureFlags.Placement.Enabled)
                    statuses.Add($"Placement: {GetPlacementStatus()}");
            }

            statuses.Add($"Player: {GetAttachmentStatus()}");

            return string.Join(" | ", statuses);
        }

        /// <summary>
        /// Shutdown all systems
        /// </summary>
        public void Shutdown()
        {
            LoggingSystem.Info("Shutting down SystemCoordinator", "System");

            try
            {
                initializer.Shutdown();
                components.Shutdown();

                LoggingSystem.Info("SystemCoordinator shutdown completed", "System");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Exception during shutdown: {ex.Message}", "System");
            }
        }

        /// <summary>
        /// Get sphere manager instance
        /// </summary>
        public Core.Features.Spheres.Managers.SphereManager? GetSphereManager() => components.SphereManager;
    }
} 