using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.Utils
{
    /// <summary>
    /// Manages persistent JSON metadata for YouTube cached songs
    /// Stores song details, download info, and file mappings
    /// </summary>
    public static class YouTubeMetadataManager
    {
        private static readonly string MetadataFileName = "youtube_metadata.json";
        private static string? _metadataPath;
        
        /// <summary>
        /// Cached metadata to avoid frequent file reads
        /// </summary>
        private static Dictionary<string, SongDetails>? _cachedMetadata;
        private static DateTime _lastCacheUpdate = DateTime.MinValue;
        private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5);
        
        /// <summary>
        /// Get the path to the metadata file
        /// </summary>
        private static string GetMetadataPath()
        {
            if (_metadataPath == null)
            {
                var cacheDirectory = YoutubeHelper.GetYouTubeCacheDirectory();
                _metadataPath = Path.Combine(cacheDirectory, MetadataFileName);
            }
            return _metadataPath;
        }
        
        /// <summary>
        /// Load all metadata from JSON file
        /// </summary>
        public static Dictionary<string, SongDetails> LoadMetadata()
        {
            // Return cached data if it's still valid
            if (_cachedMetadata != null && DateTime.Now - _lastCacheUpdate < CacheExpiry)
            {
                return new Dictionary<string, SongDetails>(_cachedMetadata);
            }
            
            try
            {
                var metadataPath = GetMetadataPath();
                
                if (!File.Exists(metadataPath))
                {
                    LoggingSystem.Debug("No metadata file found, returning empty metadata", "YouTubeMetadata");
                    _cachedMetadata = new Dictionary<string, SongDetails>();
                    _lastCacheUpdate = DateTime.Now;
                    return new Dictionary<string, SongDetails>(_cachedMetadata);
                }
                
                var jsonContent = File.ReadAllText(metadataPath);
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    LoggingSystem.Debug("Metadata file is empty, returning empty metadata", "YouTubeMetadata");
                    _cachedMetadata = new Dictionary<string, SongDetails>();
                    _lastCacheUpdate = DateTime.Now;
                    return new Dictionary<string, SongDetails>(_cachedMetadata);
                }
                
                var metadata = JsonConvert.DeserializeObject<Dictionary<string, SongDetails>>(jsonContent);
                if (metadata == null)
                {
                    LoggingSystem.Warning("Failed to deserialize metadata, returning empty metadata", "YouTubeMetadata");
                    _cachedMetadata = new Dictionary<string, SongDetails>();
                }
                else
                {
                    LoggingSystem.Info($"Loaded metadata for {metadata.Count} YouTube songs", "YouTubeMetadata");
                    _cachedMetadata = metadata;
                }
                
                _lastCacheUpdate = DateTime.Now;
                return new Dictionary<string, SongDetails>(_cachedMetadata);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error loading YouTube metadata: {ex.Message}", "YouTubeMetadata");
                _cachedMetadata = new Dictionary<string, SongDetails>();
                _lastCacheUpdate = DateTime.Now;
                return new Dictionary<string, SongDetails>(_cachedMetadata);
            }
        }
        
        /// <summary>
        /// Save metadata to JSON file
        /// </summary>
        public static bool SaveMetadata(Dictionary<string, SongDetails> metadata)
        {
            try
            {
                var metadataPath = GetMetadataPath();
                var cacheDirectory = Path.GetDirectoryName(metadataPath);
                
                // Ensure directory exists
                if (!string.IsNullOrEmpty(cacheDirectory) && !Directory.Exists(cacheDirectory))
                {
                    Directory.CreateDirectory(cacheDirectory);
                }
                
                // Serialize with nice formatting
                var jsonContent = JsonConvert.SerializeObject(metadata, Formatting.Indented);
                File.WriteAllText(metadataPath, jsonContent);
                
                // Update cache
                _cachedMetadata = new Dictionary<string, SongDetails>(metadata);
                _lastCacheUpdate = DateTime.Now;
                
                LoggingSystem.Info($"Saved metadata for {metadata.Count} YouTube songs", "YouTubeMetadata");
                return true;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error saving YouTube metadata: {ex.Message}", "YouTubeMetadata");
                return false;
            }
        }
        
        /// <summary>
        /// Add or update a song's metadata
        /// </summary>
        public static bool AddOrUpdateSong(SongDetails songDetails)
        {
            try
            {
                if (songDetails == null || string.IsNullOrEmpty(songDetails.GetVideoId()))
                {
                    LoggingSystem.Warning("Cannot add song with null details or missing video ID", "YouTubeMetadata");
                    return false;
                }
                
                var metadata = LoadMetadata();
                var videoId = songDetails.GetVideoId();
                
                // Update timestamp
                songDetails.downloadTimestamp = DateTime.Now;
                
                metadata[videoId] = songDetails;
                
                bool success = SaveMetadata(metadata);
                if (success)
                {
                    LoggingSystem.Debug($"Added/updated metadata for: {songDetails.title} ({videoId})", "YouTubeMetadata");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error adding/updating song metadata: {ex.Message}", "YouTubeMetadata");
                return false;
            }
        }
        
        /// <summary>
        /// Get metadata for a specific song by video ID
        /// </summary>
        public static SongDetails? GetSongMetadata(string videoId)
        {
            try
            {
                if (string.IsNullOrEmpty(videoId))
                {
                    return null;
                }
                
                var metadata = LoadMetadata();
                return metadata.ContainsKey(videoId) ? metadata[videoId] : null;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error getting song metadata for {videoId}: {ex.Message}", "YouTubeMetadata");
                return null;
            }
        }
        
        /// <summary>
        /// Remove metadata for a song
        /// </summary>
        public static bool RemoveSong(string videoId)
        {
            try
            {
                if (string.IsNullOrEmpty(videoId))
                {
                    return false;
                }
                
                var metadata = LoadMetadata();
                if (!metadata.ContainsKey(videoId))
                {
                    return true; // Already removed
                }
                
                metadata.Remove(videoId);
                bool success = SaveMetadata(metadata);
                
                if (success)
                {
                    LoggingSystem.Debug($"Removed metadata for video ID: {videoId}", "YouTubeMetadata");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error removing song metadata for {videoId}: {ex.Message}", "YouTubeMetadata");
                return false;
            }
        }
        
        /// <summary>
        /// Get all cached songs that have corresponding audio files
        /// </summary>
        public static List<SongDetails> GetCachedSongsWithFiles()
        {
            try
            {
                var metadata = LoadMetadata();
                var validSongs = new List<SongDetails>();
                var cacheDirectory = YoutubeHelper.GetYouTubeCacheDirectory();
                
                foreach (var kvp in metadata)
                {
                    var songDetails = kvp.Value;
                    
                    // Check if the cached file still exists
                    if (!string.IsNullOrEmpty(songDetails.cachedFilePath) && File.Exists(songDetails.cachedFilePath))
                    {
                        validSongs.Add(songDetails);
                    }
                    else
                    {
                        // Try to find the file by video ID (in case path changed)
                        var expectedFile = Path.Combine(cacheDirectory, $"{kvp.Key}.mp3");
                        if (File.Exists(expectedFile))
                        {
                            // Update the cached file path
                            songDetails.cachedFilePath = expectedFile;
                            songDetails.isDownloaded = true;
                            validSongs.Add(songDetails);
                        }
                        else
                        {
                            LoggingSystem.Debug($"Cached file not found for: {songDetails.title} ({kvp.Key})", "YouTubeMetadata");
                        }
                    }
                }
                
                LoggingSystem.Info($"Found {validSongs.Count} cached YouTube songs with valid files", "YouTubeMetadata");
                return validSongs;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error getting cached songs with files: {ex.Message}", "YouTubeMetadata");
                return new List<SongDetails>();
            }
        }
        
        /// <summary>
        /// Clean up metadata for files that no longer exist
        /// </summary>
        public static int CleanupMetadata()
        {
            try
            {
                var metadata = LoadMetadata();
                var toRemove = new List<string>();
                var cacheDirectory = YoutubeHelper.GetYouTubeCacheDirectory();
                
                foreach (var kvp in metadata)
                {
                    var songDetails = kvp.Value;
                    var videoId = kvp.Key;
                    
                    bool fileExists = false;
                    
                    // Check if the recorded cached file exists
                    if (!string.IsNullOrEmpty(songDetails.cachedFilePath) && File.Exists(songDetails.cachedFilePath))
                    {
                        fileExists = true;
                    }
                    else
                    {
                        // Check if the file exists with the expected naming
                        var expectedFile = Path.Combine(cacheDirectory, $"{videoId}.mp3");
                        if (File.Exists(expectedFile))
                        {
                            fileExists = true;
                            // Update the path in metadata
                            songDetails.cachedFilePath = expectedFile;
                        }
                    }
                    
                    if (!fileExists)
                    {
                        toRemove.Add(videoId);
                    }
                }
                
                // Remove orphaned metadata
                foreach (var videoId in toRemove)
                {
                    metadata.Remove(videoId);
                }
                
                if (toRemove.Count > 0)
                {
                    SaveMetadata(metadata);
                    LoggingSystem.Info($"Cleaned up {toRemove.Count} orphaned metadata entries", "YouTubeMetadata");
                }
                
                return toRemove.Count;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error cleaning up metadata: {ex.Message}", "YouTubeMetadata");
                return 0;
            }
        }
        
        /// <summary>
        /// Force reload metadata from file (invalidate cache)
        /// </summary>
        public static void InvalidateCache()
        {
            _cachedMetadata = null;
            _lastCacheUpdate = DateTime.MinValue;
            LoggingSystem.Debug("YouTube metadata cache invalidated", "YouTubeMetadata");
        }
    }
} 