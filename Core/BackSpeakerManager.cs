using System;
using System.Collections.Generic;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Modules;

namespace BackSpeakerMod.Core
{
    /// <summary>
    /// Backward compatibility wrapper for BackSpeaker mod
    /// Delegates all functionality to the new granular SystemCoordinator
    /// </summary>
    public class BackSpeakerManager
    {
        private static BackSpeakerManager _instance;
        public static BackSpeakerManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new BackSpeakerManager();
                return _instance;
            }
        }
        
        private readonly SystemCoordinator coordinator;

        /// <summary>
        /// Event to notify UI when tracks are reloaded
        /// </summary>
        public Action OnTracksReloaded
        {
            get => coordinator.OnTracksReloaded;
            set => coordinator.OnTracksReloaded = value;
        }

        /// <summary>
        /// Initialize BackSpeaker manager using new granular architecture
        /// </summary>
        public BackSpeakerManager()
        {
            LoggingSystem.Info("BackSpeakerManager: Initializing with SystemCoordinator", "Core");
            coordinator = new SystemCoordinator();
        }

        /// <summary>
        /// Update all systems
        /// </summary>
        public void Update() => coordinator.Update();

        // Public API - Audio Control
        public void Play() => coordinator.Play();
        public void Pause() => coordinator.Pause();
        public void TogglePlayPause() => coordinator.TogglePlayPause();
        public void SetVolume(float volume) => coordinator.SetVolume(volume);
        public void NextTrack() => coordinator.NextTrack();
        public void PreviousTrack() => coordinator.PreviousTrack();
        public void PlayTrack(int index) => coordinator.PlayTrack(index);
        public void SeekToTime(float time) => coordinator.SeekToTime(time);
        public void SeekToProgress(float progress) => coordinator.SeekToProgress(progress);
        public void ReloadTracks() => coordinator.ReloadTracks();

        // Public API - Audio Properties
        public bool IsPlaying => coordinator.IsPlaying;
        public float CurrentVolume => coordinator.CurrentVolume;
        public int GetTrackCount() => coordinator.GetTrackCount();
        public bool IsAudioReady() => coordinator.IsAudioReady();
        public float CurrentTime => coordinator.CurrentTime;
        public float TotalTime => coordinator.TotalTime;
        public float Progress => coordinator.Progress;
        public int CurrentTrackIndex => coordinator.CurrentTrackIndex;
        public string GetCurrentTrackInfo() => coordinator.GetCurrentTrackInfo();
        public string GetCurrentArtistInfo() => coordinator.GetCurrentArtistInfo();
        public List<(string title, string artist)> GetAllTracks() => coordinator.GetAllTracks();

        public RepeatMode RepeatMode
        {
            get => coordinator.RepeatMode;
            set => coordinator.RepeatMode = value;
        }

        // Public API - Player Attachment
        public void TriggerManualAttachment() => coordinator.TriggerManualAttachment();
        public string GetAttachmentStatus() => coordinator.GetAttachmentStatus();

        // Public API - Headphones
        public bool AttachHeadphones() => coordinator.AttachHeadphones();
        public void RemoveHeadphones() => coordinator.RemoveHeadphones();
        public bool ToggleHeadphones() => coordinator.ToggleHeadphones();
        public bool AreHeadphonesAttached() => coordinator.AreHeadphonesAttached();
        public string GetHeadphoneStatus() => coordinator.GetHeadphoneStatus();

        // Public API - Testing
        public bool AttachTestCube() => coordinator.AttachTestCube();
        public bool AttachGlowingSphere() => coordinator.AttachGlowingSphere();
        public bool ToggleGlowingSphere() => coordinator.ToggleGlowingSphere();
        public bool ToggleTestCube() => coordinator.ToggleTestCube();
        public void DestroyAllTestObjects() => coordinator.DestroyAllTestObjects();
        public string GetTestingStatus() => coordinator.GetTestingStatus();

        // Public API - Placement
        public void TogglePlacementMode() => coordinator.TogglePlacementMode();
        public bool IsInPlacementMode() => coordinator.IsInPlacementMode();
        public string GetPlacementStatus() => coordinator.GetPlacementStatus();
        
        /// <summary>
        /// Get overall system status
        /// </summary>
        public string GetSystemStatus() => coordinator.GetSystemStatus();
        
        /// <summary>
        /// Shutdown all systems
        /// </summary>
        public void Shutdown() => coordinator.Shutdown();
    }
}