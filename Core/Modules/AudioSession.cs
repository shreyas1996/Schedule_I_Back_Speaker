using System;
using System.Collections.Generic;
using UnityEngine;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.Core.Modules
{
    /// <summary>
    /// Represents an individual audio session for a specific music source
    /// Each tab has its own session with independent track list, progress, and state
    /// </summary>
    public class AudioSession
    {
        public MusicSourceType SourceType { get; }
        public string DisplayName { get; }
        
        // Track data
        private List<AudioClip> tracks = new List<AudioClip>();
        private List<(string title, string artist)> trackInfo = new List<(string, string)>();
        
        // Playback state
        private int currentTrackIndex = 0;
        private float savedProgress = 0f; // Progress when paused/switched away
        private bool isPaused = false;
        private bool hasBeenPlayed = false; // Whether this session has ever been active
        
        // Session-specific settings
        private float volume = 0.75f; // Default 75% volume
        private RepeatMode repeatMode = RepeatMode.None; // Default no repeat
        
        // Properties
        public int TrackCount => tracks.Count;
        public int CurrentTrackIndex => currentTrackIndex;
        public bool HasTracks => tracks.Count > 0;
        public float SavedProgress => savedProgress;
        public bool IsPaused => isPaused;
        public bool HasBeenPlayed => hasBeenPlayed;
        public float Volume => volume;
        public RepeatMode RepeatMode => repeatMode;
        
        public AudioSession(MusicSourceType sourceType)
        {
            SourceType = sourceType;
            DisplayName = sourceType switch
            {
                MusicSourceType.Jukebox => "In-Game Jukebox",
                MusicSourceType.LocalFolder => "Local Music",
                MusicSourceType.YouTube => "YouTube Music",
                _ => sourceType.ToString()
            };
            
            LoggingSystem.Info($"Created audio session for {DisplayName}", "AudioSession");
        }
        
        /// <summary>
        /// Load tracks into this session
        /// </summary>
        public void LoadTracks(List<AudioClip> newTracks, List<(string title, string artist)> newTrackInfo)
        {
            tracks.Clear();
            trackInfo.Clear();
            
            if (newTracks != null && newTrackInfo != null)
            {
                tracks.AddRange(newTracks);
                trackInfo.AddRange(newTrackInfo);
            }
            
            // Reset playback state when new tracks are loaded
            currentTrackIndex = 0;
            savedProgress = 0f;
            isPaused = false;
            
            LoggingSystem.Info($"Session {DisplayName}: Loaded {tracks.Count} tracks", "AudioSession");
        }
        
        /// <summary>
        /// Get track info for display
        /// </summary>
        public string GetCurrentTrackInfo()
        {
            if (!HasTracks || currentTrackIndex >= trackInfo.Count)
                return "No Track";
                
            return trackInfo[currentTrackIndex].title;
        }
        
        /// <summary>
        /// Get artist info for display
        /// </summary>
        public string GetCurrentArtistInfo()
        {
            if (!HasTracks || currentTrackIndex >= trackInfo.Count)
                return "Unknown Artist";
                
            return trackInfo[currentTrackIndex].artist;
        }
        
        /// <summary>
        /// Get all tracks for playlist display
        /// </summary>
        public List<(string title, string artist)> GetAllTracks()
        {
            return new List<(string, string)>(trackInfo);
        }
        
        /// <summary>
        /// Get current audio clip
        /// </summary>
        public AudioClip? GetCurrentClip()
        {
            if (!HasTracks || currentTrackIndex >= tracks.Count)
                return null;
                
            return tracks[currentTrackIndex];
        }
        
        /// <summary>
        /// Get all audio clips for playlist functionality
        /// </summary>
        public List<AudioClip> GetAllClips()
        {
            return new List<AudioClip>(tracks);
        }
        
        /// <summary>
        /// Play a specific track
        /// </summary>
        public bool PlayTrack(int index)
        {
            if (index < 0 || index >= tracks.Count)
            {
                LoggingSystem.Warning($"Session {DisplayName}: Invalid track index {index}", "AudioSession");
                return false;
            }
            
            currentTrackIndex = index;
            savedProgress = 0f;
            isPaused = false;
            hasBeenPlayed = true;
            
            LoggingSystem.Info($"Session {DisplayName}: Playing track {index + 1}/{tracks.Count}: {GetCurrentTrackInfo()}", "AudioSession");
            return true;
        }
        
        /// <summary>
        /// Go to next track
        /// </summary>
        public bool NextTrack()
        {
            if (!HasTracks) return false;
            
            int nextIndex = (currentTrackIndex + 1) % tracks.Count;
            return PlayTrack(nextIndex);
        }
        
        /// <summary>
        /// Go to previous track
        /// </summary>
        public bool PreviousTrack()
        {
            if (!HasTracks) return false;
            
            int prevIndex = currentTrackIndex - 1;
            if (prevIndex < 0) prevIndex = tracks.Count - 1;
            return PlayTrack(prevIndex);
        }
        
        /// <summary>
        /// Pause this session and save current progress
        /// </summary>
        public void Pause(float currentProgress)
        {
            savedProgress = currentProgress;
            isPaused = true;
            LoggingSystem.Debug($"Session {DisplayName}: Paused at {savedProgress:F1}s", "AudioSession");
        }
        
        /// <summary>
        /// Resume this session
        /// </summary>
        public void Resume()
        {
            isPaused = false;
            hasBeenPlayed = true;
            LoggingSystem.Debug($"Session {DisplayName}: Resumed from {savedProgress:F1}s", "AudioSession");
        }
        
        /// <summary>
        /// Stop this session
        /// </summary>
        public void Stop()
        {
            isPaused = true;
            savedProgress = 0f;
            LoggingSystem.Debug($"Session {DisplayName}: Stopped", "AudioSession");
        }
        
        /// <summary>
        /// Set volume for this session
        /// </summary>
        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            LoggingSystem.Debug($"Session {DisplayName}: Volume set to {volume:P0}", "AudioSession");
        }
        
        /// <summary>
        /// Set repeat mode for this session
        /// </summary>
        public void SetRepeatMode(RepeatMode newRepeatMode)
        {
            repeatMode = newRepeatMode;
            LoggingSystem.Debug($"Session {DisplayName}: Repeat mode set to {repeatMode}", "AudioSession");
        }
        
        /// <summary>
        /// Get session status for debugging
        /// </summary>
        public string GetStatus()
        {
            if (!HasTracks)
                return $"{DisplayName}: No tracks";
                
            var status = isPaused ? "Paused" : "Ready";
            return $"{DisplayName}: {status} - Track {currentTrackIndex + 1}/{tracks.Count} - {GetCurrentTrackInfo()}";
        }
    }
} 