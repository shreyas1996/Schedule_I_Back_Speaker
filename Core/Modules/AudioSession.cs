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
        
        // Unified track data (works for all sources including YouTube)
        private List<AudioClip?> tracks = new List<AudioClip?>(); // AudioClip can be null for YouTube songs
        private List<(string title, string artist)> trackInfo = new List<(string, string)>();
        private List<SongDetails?> songDetailsInfo = new List<SongDetails?>(); // YouTube song details when available
        
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
        public bool IsYouTubeSession => SourceType == MusicSourceType.YouTube;
        
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
            
            LoggingSystem.Info($"Created unified audio session for {DisplayName}", "AudioSession");
        }
        
        /// <summary>
        /// Load tracks into this session (for non-YouTube sources)
        /// </summary>
        public void LoadTracks(List<AudioClip> newTracks, List<(string title, string artist)> newTrackInfo)
        {
            if (IsYouTubeSession)
            {
                LoggingSystem.Warning("Cannot load AudioClip tracks into YouTube session - use YouTube song methods instead", "AudioSession");
                return;
            }
            
            tracks.Clear();
            trackInfo.Clear();
            songDetailsInfo.Clear();
            
            if (newTracks != null && newTrackInfo != null)
            {
                for (int i = 0; i < newTracks.Count && i < newTrackInfo.Count; i++)
                {
                    tracks.Add(newTracks[i]);
                    trackInfo.Add(newTrackInfo[i]);
                    songDetailsInfo.Add(null); // No YouTube details for regular tracks
                }
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
            songDetailsInfo.Add(null); // No YouTube details for regular tracks
            
            LoggingSystem.Info($"Session {DisplayName}: Added track '{title}' by '{artist}' - Total tracks: {tracks.Count}", "AudioSession");
        }
        
        /// <summary>
        /// Add multiple tracks to this session (for non-YouTube sources)
        /// </summary>
        public void AddTracks(List<AudioClip> newTracks, List<(string title, string artist)> newTrackInfo)
        {
            if (IsYouTubeSession)
            {
                LoggingSystem.Warning("Cannot add AudioClip tracks to YouTube session - use YouTube song methods instead", "AudioSession");
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
                    songDetailsInfo.Add(null); // No YouTube details for regular tracks
                }
            }
            
            int addedCount = tracks.Count - initialCount;
            LoggingSystem.Info($"Session {DisplayName}: Added {addedCount} tracks - Total tracks: {tracks.Count}", "AudioSession");
        }
        
        /// <summary>
        /// Add a YouTube song to the unified playlist
        /// </summary>
        public bool AddYouTubeSong(SongDetails songDetails)
        {
            if (!IsYouTubeSession)
            {
                LoggingSystem.Warning("Cannot add YouTube song to non-YouTube session", "AudioSession");
                return false;
            }
            
            if (songDetails == null)
            {
                LoggingSystem.Warning("Cannot add null YouTube song", "AudioSession");
                return false;
            }
            
            // Check if song already exists
            if (ContainsYouTubeSong(songDetails.url ?? ""))
            {
                LoggingSystem.Info($"YouTube song '{songDetails.title}' already exists in playlist", "AudioSession");
                return false;
            }
            
            tracks.Add(null); // No AudioClip for YouTube songs
            trackInfo.Add((songDetails.title ?? "Unknown Title", songDetails.GetArtist()));
            songDetailsInfo.Add(songDetails);
            
            LoggingSystem.Info($"Session {DisplayName}: Added YouTube song '{songDetails.title}' by '{songDetails.GetArtist()}' - Total tracks: {tracks.Count}", "AudioSession");
            return true;
        }
        
        /// <summary>
        /// Remove a YouTube song from the unified playlist
        /// </summary>
        public bool RemoveYouTubeSong(string url)
        {
            if (!IsYouTubeSession)
            {
                LoggingSystem.Warning("Cannot remove YouTube song from non-YouTube session", "AudioSession");
                return false;
            }
            
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }
            
            // Find the song by URL in songDetailsInfo
            for (int i = 0; i < songDetailsInfo.Count; i++)
            {
                var songDetail = songDetailsInfo[i];
                if (songDetail != null && songDetail.url == url)
                {
                    string title = trackInfo[i].title;
                    tracks.RemoveAt(i);
                    trackInfo.RemoveAt(i);
                    songDetailsInfo.RemoveAt(i);
                    
                    // Adjust current track index if needed
                    if (currentTrackIndex >= i && currentTrackIndex > 0)
                    {
                        currentTrackIndex--;
                    }
                    
                    LoggingSystem.Info($"Session {DisplayName}: Removed YouTube song '{title}' - Total tracks: {tracks.Count}", "AudioSession");
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if YouTube song exists in playlist
        /// </summary>
        public bool ContainsYouTubeSong(string url)
        {
            if (!IsYouTubeSession || string.IsNullOrEmpty(url)) return false;
            
            return songDetailsInfo.Any(sd => sd != null && sd.url == url);
        }
        
        /// <summary>
        /// Get current YouTube song details (null if not YouTube or no current song)
        /// </summary>
        public SongDetails? GetCurrentYouTubeSong()
        {
            if (!IsYouTubeSession || currentTrackIndex >= songDetailsInfo.Count)
            {
                return null;
            }
            
            return songDetailsInfo[currentTrackIndex];
        }
        
        /// <summary>
        /// Check if current track is a YouTube song
        /// </summary>
        public bool IsCurrentTrackYouTube()
        {
            return GetCurrentYouTubeSong() != null;
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
        /// Get current audio clip (not applicable for YouTube songs)
        /// </summary>
        public AudioClip? GetCurrentClip()
        {
            if (!HasTracks || currentTrackIndex >= tracks.Count)
                return null;
                
            return tracks[currentTrackIndex]; // Will be null for YouTube songs
        }
        
        /// <summary>
        /// Get all audio clips for playlist functionality (excludes YouTube songs)
        /// </summary>
        public List<AudioClip> GetAllClips()
        {
            return tracks.Where(t => t != null).Cast<AudioClip>().ToList();
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
            
            if (youtubeProvider != null)
            {
                youtubeProvider.AddCachedSongsToSession(this);
                LoggingSystem.Info($"Initialized YouTube session with cached songs - Total: {tracks.Count}", "AudioSession");
            }
            else
            {
                LoggingSystem.Warning("YouTube provider not available for cached song initialization", "AudioSession");
            }
        }
    }
} 