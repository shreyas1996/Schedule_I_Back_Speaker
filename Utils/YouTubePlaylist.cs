using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace BackSpeakerMod.Utils
{
    /// <summary>
    /// Represents a YouTube playlist with songs and metadata
    /// </summary>
    [Serializable]
    public class YouTubePlaylist
    {
        [JsonProperty("id")]
        public string id { get; set; } = "";
        
        [JsonProperty("name")]
        public string name { get; set; } = "";
        
        [JsonProperty("createdDate")]
        public DateTime createdDate { get; set; } = DateTime.Now;
        
        [JsonProperty("lastModified")]
        public DateTime lastModified { get; set; } = DateTime.Now;
        
        [JsonProperty("songs")]
        public List<SongDetails> songs { get; set; } = new List<SongDetails>();
        
        [JsonProperty("description")]
        public string? description { get; set; }
        
        /// <summary>
        /// Get count of downloaded songs in this playlist
        /// </summary>
        [JsonIgnore]
        public int DownloadedCount => songs.Count(s => s.isDownloaded);
        
        /// <summary>
        /// Get count of total songs in this playlist
        /// </summary>
        [JsonIgnore]
        public int TotalCount => songs.Count;
        
        /// <summary>
        /// Check if playlist is empty
        /// </summary>
        [JsonIgnore]
        public bool IsEmpty => songs.Count == 0;
        
        /// <summary>
        /// Get formatted display name with download status
        /// </summary>
        [JsonIgnore]
        public string DisplayName => $"{name} ({DownloadedCount}/{TotalCount})";
        
        public YouTubePlaylist()
        {
            id = Guid.NewGuid().ToString("N")[..8]; // Short 8-character ID
        }
        
        public YouTubePlaylist(string name) : this()
        {
            this.name = name;
        }
        
        /// <summary>
        /// Add a song to the playlist if it doesn't already exist
        /// </summary>
        public bool AddSong(SongDetails song)
        {
            if (song == null) return false;
            
            var videoId = song.GetVideoId();
            if (songs.Any(s => s.GetVideoId() == videoId))
            {
                return false; // Song already exists
            }
            
            songs.Add(song);
            lastModified = DateTime.Now;
            return true;
        }
        
        /// <summary>
        /// Remove a song from the playlist
        /// </summary>
        public bool RemoveSong(string videoId)
        {
            if (string.IsNullOrEmpty(videoId)) return false;
            
            var songToRemove = songs.FirstOrDefault(s => s.GetVideoId() == videoId);
            if (songToRemove != null)
            {
                songs.Remove(songToRemove);
                lastModified = DateTime.Now;
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if playlist contains a song
        /// </summary>
        public bool ContainsSong(string videoId)
        {
            if (string.IsNullOrEmpty(videoId)) return false;
            return songs.Any(s => s.GetVideoId() == videoId);
        }
        
        /// <summary>
        /// Get a song from the playlist
        /// </summary>
        public SongDetails? GetSong(string videoId)
        {
            if (string.IsNullOrEmpty(videoId)) return null;
            return songs.FirstOrDefault(s => s.GetVideoId() == videoId);
        }
        
        /// <summary>
        /// Update download status for a song in the playlist
        /// </summary>
        public bool UpdateSongDownloadStatus(string videoId, bool isDownloaded, string? cachedFilePath = null)
        {
            var song = GetSong(videoId);
            if (song != null)
            {
                song.isDownloaded = isDownloaded;
                if (!string.IsNullOrEmpty(cachedFilePath))
                {
                    song.cachedFilePath = cachedFilePath;
                }
                lastModified = DateTime.Now;
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Get all downloaded songs that are ready to play
        /// </summary>
        public List<SongDetails> GetPlayableSongs()
        {
            return songs.Where(s => s.IsReadyToPlay()).ToList();
        }
        
        /// <summary>
        /// Clear all songs from playlist
        /// </summary>
        public void Clear()
        {
            songs.Clear();
            lastModified = DateTime.Now;
        }
    }
    
    /// <summary>
    /// Summary information about a playlist for the global index
    /// </summary>
    [Serializable]
    public class YouTubePlaylistInfo
    {
        [JsonProperty("id")]
        public string id { get; set; } = "";
        
        [JsonProperty("name")]
        public string name { get; set; } = "";
        
        [JsonProperty("createdDate")]
        public DateTime createdDate { get; set; } = DateTime.Now;
        
        [JsonProperty("lastModified")]
        public DateTime lastModified { get; set; } = DateTime.Now;
        
        [JsonProperty("songCount")]
        public int songCount { get; set; } = 0;
        
        [JsonProperty("downloadedCount")]
        public int downloadedCount { get; set; } = 0;
        
        [JsonProperty("description")]
        public string? description { get; set; }
        
        /// <summary>
        /// Convert from full playlist
        /// </summary>
        public static YouTubePlaylistInfo FromPlaylist(YouTubePlaylist playlist)
        {
            return new YouTubePlaylistInfo
            {
                id = playlist.id,
                name = playlist.name,
                createdDate = playlist.createdDate,
                lastModified = playlist.lastModified,
                songCount = playlist.TotalCount,
                downloadedCount = playlist.DownloadedCount,
                description = playlist.description
            };
        }
    }
} 