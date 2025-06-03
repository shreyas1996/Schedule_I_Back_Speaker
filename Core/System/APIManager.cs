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

        // Audio Control API - with headphone dependency
        public void Play()
        {
            if (!AreHeadphonesAttached())
            {
                return;
            }
            components.AudioManager?.Play();
        }
        
        public void Pause() => components.AudioManager?.Pause();
        
        public void TogglePlayPause()
        {
            if (!AreHeadphonesAttached())
            {
                return;
            }
            components.AudioManager?.TogglePlayPause();
        }
        
        public void SetVolume(float volume) => components.AudioManager?.SetVolume(volume);
        
        public void NextTrack()
        {
            if (!AreHeadphonesAttached())
            {
                return;
            }
            components.AudioManager?.NextTrack();
        }
        
        public void PreviousTrack()
        {
            if (!AreHeadphonesAttached())
            {
                return;
            }
            components.AudioManager?.PreviousTrack();
        }
        
        public void PlayTrack(int index)
        {
            if (!AreHeadphonesAttached())
            {
                return;
            }
            components.AudioManager?.PlayTrack(index);
        }
        
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
        public bool AttachHeadphones()
        {
            bool success = components.HeadphoneManager?.AttachHeadphones() ?? false;
            if (success)
            {
                components.PlayerAttachment?.TriggerManualAttachment();
                components.AudioManager?.LoadTracks();
            }
            return success;
        }
        
        public void RemoveHeadphones()
        {
            // Step 1: Stop any playing music first
            if (IsPlaying)
            {
                components.AudioManager?.Pause();
            }
            
            // Step 2: Reset audio manager to clear audio source reference
            components.AudioManager?.Reset();
            
            // Step 3: Detach and destroy the speaker
            components.PlayerAttachment?.DetachSpeaker();
            
            // Step 4: Remove headphones last
            components.HeadphoneManager?.RemoveHeadphones();
            
            LoggingSystem.Info("Complete headphone removal sequence executed", "System");
        }
        
        public bool ToggleHeadphones()
        {
            bool wasAttached = AreHeadphonesAttached();
            bool success = components.HeadphoneManager?.ToggleHeadphones() ?? false;
            
            if (success)
            {
                bool nowAttached = AreHeadphonesAttached();
                if (nowAttached && !wasAttached)
                {
                    // Attaching: Speaker -> Load tracks
                    components.PlayerAttachment?.TriggerManualAttachment();
                    components.AudioManager?.LoadTracks();
                }
                else if (!nowAttached && wasAttached)
                {
                    // Detaching: Stop music -> Reset audio -> Detach speaker (headphones already detached by ToggleHeadphones)
                    if (IsPlaying)
                    {
                        components.AudioManager?.Pause();
                    }
                    components.AudioManager?.Reset();
                    components.PlayerAttachment?.DetachSpeaker();
                }
            }
            
            return success;
        }
        
        public bool AreHeadphonesAttached() => components.HeadphoneManager?.AreHeadphonesAttached ?? false;
        public string GetHeadphoneStatus() => components.HeadphoneManager?.GetStatus() ?? "Headphone system not initialized";

    }
} 