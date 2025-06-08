using System;
using System.Collections.Generic;
using System.Linq;
using BackSpeakerMod.Utils;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.Core.Modules
{
    /// <summary>
    /// Manages a playlist of YouTube songs using SongDetails for streaming
    /// </summary>
    public class YouTubePlaylist
    {
        private readonly List<SongDetails> playlist = new List<SongDetails>();
        private int currentTrackIndex = 0;
        
        // Events
        public event Action? OnPlaylistChanged;
        
        // Properties
        public int Count => playlist.Count;
        public bool HasTracks => playlist.Count > 0;
        public int CurrentTrackIndex => currentTrackIndex;
        
        /// <summary>
        /// Add a song to the playlist
        /// </summary>
        public bool AddSong(SongDetails songDetails)
        {
            if (songDetails == null || string.IsNullOrEmpty(songDetails.url))
            {
                LoggingSystem.Warning("Cannot add null or invalid song to YouTube playlist", "YouTubePlaylist");
                return false;
            }
            
            // Check if song already exists (by URL)
            if (playlist.Any(s => s.url == songDetails.url))
            {
                LoggingSystem.Info($"Song '{songDetails.title}' already exists in playlist", "YouTubePlaylist");
                return false;
            }
            
            playlist.Add(songDetails);
            LoggingSystem.Info($"Added '{songDetails.title}' by '{songDetails.GetArtist()}' to YouTube playlist - Total: {playlist.Count}", "YouTubePlaylist");
            
            OnPlaylistChanged?.Invoke();
            return true;
        }
        
        /// <summary>
        /// Remove a song from the playlist by URL
        /// </summary>
        public bool RemoveSong(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            
            var songToRemove = playlist.FirstOrDefault(s => s.url == url);
            if (songToRemove != null)
            {
                int removeIndex = playlist.IndexOf(songToRemove);
                playlist.Remove(songToRemove);
                
                // Adjust current track index if needed
                if (removeIndex < currentTrackIndex)
                {
                    currentTrackIndex--;
                }
                else if (removeIndex == currentTrackIndex && currentTrackIndex >= playlist.Count)
                {
                    currentTrackIndex = Math.Max(0, playlist.Count - 1);
                }
                
                LoggingSystem.Info($"Removed '{songToRemove.title}' from YouTube playlist - Total: {playlist.Count}", "YouTubePlaylist");
                OnPlaylistChanged?.Invoke();
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if a song exists in the playlist
        /// </summary>
        public bool ContainsSong(string url)
        {
            return !string.IsNullOrEmpty(url) && playlist.Any(s => s.url == url);
        }
        
        /// <summary>
        /// Get current song details
        /// </summary>
        public SongDetails? GetCurrentSong()
        {
            if (!HasTracks || currentTrackIndex >= playlist.Count) return null;
            return playlist[currentTrackIndex];
        }
        
        /// <summary>
        /// Get song at specific index
        /// </summary>
        public SongDetails? GetSong(int index)
        {
            if (index < 0 || index >= playlist.Count) return null;
            return playlist[index];
        }
        
        /// <summary>
        /// Get all songs as track info for UI display
        /// </summary>
        public List<(string title, string artist)> GetAllTracksInfo()
        {
            return playlist.Select(song => (song.title ?? "Unknown Title", song.GetArtist())).ToList();
        }
        
        /// <summary>
        /// Get all song details
        /// </summary>
        public List<SongDetails> GetAllSongs()
        {
            return new List<SongDetails>(playlist);
        }
        
        /// <summary>
        /// Set current track index
        /// </summary>
        public bool SetCurrentTrack(int index)
        {
            if (index < 0 || index >= playlist.Count) return false;
            
            currentTrackIndex = index;
            LoggingSystem.Debug($"YouTube playlist current track set to {index + 1}/{playlist.Count}: {GetCurrentSong()?.title}", "YouTubePlaylist");
            return true;
        }
        
        /// <summary>
        /// Go to next track
        /// </summary>
        public bool NextTrack()
        {
            if (!HasTracks) return false;
            
            int nextIndex = (currentTrackIndex + 1) % playlist.Count;
            return SetCurrentTrack(nextIndex);
        }
        
        /// <summary>
        /// Go to previous track
        /// </summary>
        public bool PreviousTrack()
        {
            if (!HasTracks) return false;
            
            int prevIndex = currentTrackIndex - 1;
            if (prevIndex < 0) prevIndex = playlist.Count - 1;
            return SetCurrentTrack(prevIndex);
        }
        
        /// <summary>
        /// Clear the entire playlist
        /// </summary>
        public void Clear()
        {
            playlist.Clear();
            currentTrackIndex = 0;
            LoggingSystem.Info("YouTube playlist cleared", "YouTubePlaylist");
            OnPlaylistChanged?.Invoke();
        }
        
        /// <summary>
        /// Get playlist status for debugging
        /// </summary>
        public string GetStatus()
        {
            if (!HasTracks) return "YouTube Playlist: Empty";
            
            var currentSong = GetCurrentSong();
            return $"YouTube Playlist: {currentTrackIndex + 1}/{playlist.Count} - {currentSong?.title ?? "Unknown"}";
        }
    }
} 