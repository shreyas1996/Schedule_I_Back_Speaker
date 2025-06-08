using System;
using System.Collections.Generic;
using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Utils;

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
        
        // Track data (for non-YouTube sources)
        private List<AudioClip> tracks = new List<AudioClip>();
        private List<(string title, string artist)> trackInfo = new List<(string, string)>();
        
        // YouTube playlist (for YouTube source)
        private YouTubePlaylist? youtubePlaylist;
        
        // Playback state
        private int currentTrackIndex = 0;
        private float savedProgress = 0f; // Progress when paused/switched away
        private bool isPaused = false;
        private bool hasBeenPlayed = false; // Whether this session has ever been active
        
        // Session-specific settings
        private float volume = 0.75f; // Default 75% volume
        private RepeatMode repeatMode = RepeatMode.None; // Default no repeat
        
        // Properties
        public int TrackCount => IsYouTubeSession ? (youtubePlaylist?.Count ?? 0) : tracks.Count;
        public int CurrentTrackIndex => IsYouTubeSession ? (youtubePlaylist?.CurrentTrackIndex ?? 0) : currentTrackIndex;
        public bool HasTracks => IsYouTubeSession ? (youtubePlaylist?.HasTracks ?? false) : tracks.Count > 0;
        public float SavedProgress => savedProgress;
        public bool IsPaused => isPaused;
        public bool HasBeenPlayed => hasBeenPlayed;
        public float Volume => volume;
        public RepeatMode RepeatMode => repeatMode;
        public bool IsYouTubeSession => SourceType == MusicSourceType.YouTube;
        
        // YouTube-specific properties
        public YouTubePlaylist? YouTubePlaylist => youtubePlaylist;
        
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
            
            // Create YouTube playlist for YouTube sessions
            if (IsYouTubeSession)
            {
                youtubePlaylist = new YouTubePlaylist();
                youtubePlaylist.OnPlaylistChanged += () => LoggingSystem.Debug($"YouTube playlist changed - Total: {youtubePlaylist.Count}", "AudioSession");
            }
            
            LoggingSystem.Info($"Created audio session for {DisplayName}", "AudioSession");
        }
        
        /// <summary>
        /// Load tracks into this session (for non-YouTube sources)
        /// </summary>
        public void LoadTracks(List<AudioClip> newTracks, List<(string title, string artist)> newTrackInfo)
        {
            if (IsYouTubeSession)
            {
                LoggingSystem.Warning("Cannot load AudioClip tracks into YouTube session - use YouTube playlist methods instead", "AudioSession");
                return;
            }
            
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
        /// Add a single track to this session (for non-YouTube sources)
        /// </summary>
        public void AddTrack(AudioClip audioClip, string title, string artist)
        {
            if (IsYouTubeSession)
            {
                LoggingSystem.Warning("Cannot add AudioClip to YouTube session - use AddYouTubeSong instead", "AudioSession");
                return;
            }
            
            if (audioClip == null)
            {
                LoggingSystem.Warning($"Session {DisplayName}: Cannot add null audio clip", "AudioSession");
                return;
            }
            
            tracks.Add(audioClip);
            trackInfo.Add((title, artist));
            
            LoggingSystem.Info($"Session {DisplayName}: Added track '{title}' by '{artist}' - Total tracks: {tracks.Count}", "AudioSession");
        }
        
        /// <summary>
        /// Add multiple tracks to this session (for non-YouTube sources)
        /// </summary>
        public void AddTracks(List<AudioClip> newTracks, List<(string title, string artist)> newTrackInfo)
        {
            if (IsYouTubeSession)
            {
                LoggingSystem.Warning("Cannot add AudioClip tracks to YouTube session - use YouTube playlist methods instead", "AudioSession");
                return;
            }
            
            if (newTracks == null || newTrackInfo == null || newTracks.Count != newTrackInfo.Count)
            {
                LoggingSystem.Warning($"Session {DisplayName}: Invalid tracks data for AddTracks", "AudioSession");
                return;
            }
            
            int initialCount = tracks.Count;
            
            for (int i = 0; i < newTracks.Count; i++)
            {
                if (newTracks[i] != null)
                {
                    tracks.Add(newTracks[i]);
                    trackInfo.Add(newTrackInfo[i]);
                }
            }
            
            int addedCount = tracks.Count - initialCount;
            LoggingSystem.Info($"Session {DisplayName}: Added {addedCount} tracks - Total tracks: {tracks.Count}", "AudioSession");
        }
        
        /// <summary>
        /// Add a YouTube song to the YouTube playlist
        /// </summary>
        public bool AddYouTubeSong(SongDetails songDetails)
        {
            if (!IsYouTubeSession)
            {
                LoggingSystem.Warning("Cannot add YouTube song to non-YouTube session", "AudioSession");
                return false;
            }
            
            return youtubePlaylist?.AddSong(songDetails) ?? false;
        }
        
        /// <summary>
        /// Remove a YouTube song from the YouTube playlist
        /// </summary>
        public bool RemoveYouTubeSong(string url)
        {
            if (!IsYouTubeSession)
            {
                LoggingSystem.Warning("Cannot remove YouTube song from non-YouTube session", "AudioSession");
                return false;
            }
            
            return youtubePlaylist?.RemoveSong(url) ?? false;
        }
        
        /// <summary>
        /// Check if YouTube song exists in playlist
        /// </summary>
        public bool ContainsYouTubeSong(string url)
        {
            if (!IsYouTubeSession) return false;
            return youtubePlaylist?.ContainsSong(url) ?? false;
        }
        
        /// <summary>
        /// Get track info for display
        /// </summary>
        public string GetCurrentTrackInfo()
        {
            if (IsYouTubeSession)
            {
                var currentSong = youtubePlaylist?.GetCurrentSong();
                return currentSong?.title ?? "No Track";
            }
            
            if (!HasTracks || currentTrackIndex >= trackInfo.Count)
                return "No Track";
                
            return trackInfo[currentTrackIndex].title;
        }
        
        /// <summary>
        /// Get artist info for display
        /// </summary>
        public string GetCurrentArtistInfo()
        {
            if (IsYouTubeSession)
            {
                var currentSong = youtubePlaylist?.GetCurrentSong();
                return currentSong?.GetArtist() ?? "Unknown Artist";
            }
            
            if (!HasTracks || currentTrackIndex >= trackInfo.Count)
                return "Unknown Artist";
                
            return trackInfo[currentTrackIndex].artist;
        }
        
        /// <summary>
        /// Get all tracks for playlist display
        /// </summary>
        public List<(string title, string artist)> GetAllTracks()
        {
            if (IsYouTubeSession)
            {
                return youtubePlaylist?.GetAllTracksInfo() ?? new List<(string, string)>();
            }
            
            return new List<(string, string)>(trackInfo);
        }
        
        /// <summary>
        /// Get current audio clip (not applicable for YouTube sessions)
        /// </summary>
        public AudioClip? GetCurrentClip()
        {
            if (IsYouTubeSession)
            {
                LoggingSystem.Warning("GetCurrentClip not applicable for YouTube sessions - use streaming instead", "AudioSession");
                return null;
            }
            
            if (!HasTracks || currentTrackIndex >= tracks.Count)
                return null;
                
            return tracks[currentTrackIndex];
        }
        
        /// <summary>
        /// Get all audio clips for playlist functionality (not applicable for YouTube sessions)
        /// </summary>
        public List<AudioClip> GetAllClips()
        {
            if (IsYouTubeSession)
            {
                LoggingSystem.Warning("GetAllClips not applicable for YouTube sessions - use streaming instead", "AudioSession");
                return new List<AudioClip>();
            }
            
            return new List<AudioClip>(tracks);
        }
        
        /// <summary>
        /// Play a specific track
        /// </summary>
        public bool PlayTrack(int index)
        {
            if (IsYouTubeSession)
            {
                if (youtubePlaylist?.SetCurrentTrack(index) == true)
                {
                    savedProgress = 0f;
                    isPaused = false;
                    hasBeenPlayed = true;
                    
                    var currentSong = youtubePlaylist.GetCurrentSong();
                    LoggingSystem.Info($"Session {DisplayName}: Playing YouTube track {index + 1}/{youtubePlaylist.Count}: {currentSong?.title}", "AudioSession");
                    return true;
                }
                return false;
            }
            
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
            if (IsYouTubeSession)
            {
                return youtubePlaylist?.NextTrack() ?? false;
            }
            
            if (!HasTracks) return false;
            
            int nextIndex = (currentTrackIndex + 1) % tracks.Count;
            return PlayTrack(nextIndex);
        }
        
        /// <summary>
        /// Go to previous track
        /// </summary>
        public bool PreviousTrack()
        {
            if (IsYouTubeSession)
            {
                return youtubePlaylist?.PreviousTrack() ?? false;
            }
            
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
        
        /// <summary>
        /// Initialize cached songs for YouTube session (called after system is ready)
        /// </summary>
        public void InitializeCachedSongs(YouTubeMusicProvider? youtubeProvider)
        {
            if (!IsYouTubeSession)
            {
                LoggingSystem.Warning("InitializeCachedSongs called on non-YouTube session", "AudioSession");
                return;
            }
            
            if (youtubePlaylist == null)
            {
                LoggingSystem.Warning("YouTube playlist not available for cached song initialization", "AudioSession");
                return;
            }
            
            if (youtubeProvider != null)
            {
                youtubeProvider.AddCachedSongsToPlaylist(youtubePlaylist);
                LoggingSystem.Info($"Initialized YouTube session with cached songs - Total: {youtubePlaylist.Count}", "AudioSession");
            }
            else
            {
                LoggingSystem.Warning("YouTube provider not available for cached song initialization", "AudioSession");
            }
        }
    }
} 