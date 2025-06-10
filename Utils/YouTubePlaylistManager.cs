using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.Utils
{
    /// <summary>
    /// Manages YouTube playlists with persistent storage
    /// Handles global playlist index and individual playlist files
    /// </summary>
    public static class YouTubePlaylistManager
    {
        private static readonly string PlaylistsIndexFileName = "youtube_playlists.json";
        private static readonly string PlaylistFilePrefix = "playlist_";
        private static readonly string PlaylistFileExtension = ".json";
        
        private static string? _playlistsDirectory;
        private static Dictionary<string, YouTubePlaylistInfo>? _cachedIndex;
        private static DateTime _lastIndexUpdate = DateTime.MinValue;
        private static readonly TimeSpan IndexCacheExpiry = TimeSpan.FromMinutes(2);
        
        // Events
        public static event Action<YouTubePlaylist>? OnPlaylistCreated;
        public static event Action<YouTubePlaylist>? OnPlaylistUpdated;
        public static event Action<string>? OnPlaylistDeleted;
        public static event Action? OnPlaylistIndexChanged;
        
        /// <summary>
        /// Get the playlists directory path
        /// </summary>
        private static string GetPlaylistsDirectory()
        {
            if (_playlistsDirectory == null)
            {
                var cacheDirectory = YoutubeHelper.GetYouTubeCacheDirectory();
                _playlistsDirectory = Path.Combine(cacheDirectory, "Playlists");
            }
            
            // Ensure directory exists
            if (!Directory.Exists(_playlistsDirectory))
            {
                Directory.CreateDirectory(_playlistsDirectory);
                LoggingSystem.Info($"Created YouTube playlists directory: {_playlistsDirectory}", "YouTubePlaylist");
            }
            
            return _playlistsDirectory;
        }
        
        /// <summary>
        /// Get path to the global playlists index file
        /// </summary>
        private static string GetIndexFilePath()
        {
            return Path.Combine(GetPlaylistsDirectory(), PlaylistsIndexFileName);
        }
        
        /// <summary>
        /// Get path to a specific playlist file
        /// </summary>
        private static string GetPlaylistFilePath(string playlistId)
        {
            return Path.Combine(GetPlaylistsDirectory(), $"{PlaylistFilePrefix}{playlistId}{PlaylistFileExtension}");
        }
        
        /// <summary>
        /// Load the global playlists index
        /// </summary>
        public static Dictionary<string, YouTubePlaylistInfo> LoadPlaylistIndex()
        {
            // Return cached data if still valid
            if (_cachedIndex != null && DateTime.Now - _lastIndexUpdate < IndexCacheExpiry)
            {
                return new Dictionary<string, YouTubePlaylistInfo>(_cachedIndex);
            }
            
            try
            {
                var indexPath = GetIndexFilePath();
                
                if (!File.Exists(indexPath))
                {
                    LoggingSystem.Debug("No playlist index found, returning empty index", "YouTubePlaylist");
                    _cachedIndex = new Dictionary<string, YouTubePlaylistInfo>();
                    _lastIndexUpdate = DateTime.Now;
                    return new Dictionary<string, YouTubePlaylistInfo>(_cachedIndex);
                }
                
                var jsonContent = File.ReadAllText(indexPath);
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    LoggingSystem.Debug("Playlist index file is empty, returning empty index", "YouTubePlaylist");
                    _cachedIndex = new Dictionary<string, YouTubePlaylistInfo>();
                    _lastIndexUpdate = DateTime.Now;
                    return new Dictionary<string, YouTubePlaylistInfo>(_cachedIndex);
                }
                
                var index = JsonConvert.DeserializeObject<Dictionary<string, YouTubePlaylistInfo>>(jsonContent);
                if (index == null)
                {
                    LoggingSystem.Warning("Failed to deserialize playlist index, returning empty index", "YouTubePlaylist");
                    _cachedIndex = new Dictionary<string, YouTubePlaylistInfo>();
                }
                else
                {
                    LoggingSystem.Info($"Loaded playlist index with {index.Count} playlists", "YouTubePlaylist");
                    _cachedIndex = index;
                }
                
                _lastIndexUpdate = DateTime.Now;
                return new Dictionary<string, YouTubePlaylistInfo>(_cachedIndex);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error loading playlist index: {ex.Message}", "YouTubePlaylist");
                _cachedIndex = new Dictionary<string, YouTubePlaylistInfo>();
                _lastIndexUpdate = DateTime.Now;
                return new Dictionary<string, YouTubePlaylistInfo>(_cachedIndex);
            }
        }
        
        /// <summary>
        /// Save the global playlists index
        /// </summary>
        private static bool SavePlaylistIndex(Dictionary<string, YouTubePlaylistInfo> index)
        {
            try
            {
                var indexPath = GetIndexFilePath();
                var playlistDirectory = Path.GetDirectoryName(indexPath);
                
                // Ensure directory exists
                if (!string.IsNullOrEmpty(playlistDirectory) && !Directory.Exists(playlistDirectory))
                {
                    Directory.CreateDirectory(playlistDirectory);
                }
                
                // Serialize with nice formatting
                var jsonContent = JsonConvert.SerializeObject(index, Formatting.Indented);
                File.WriteAllText(indexPath, jsonContent);
                
                // Update cache
                _cachedIndex = new Dictionary<string, YouTubePlaylistInfo>(index);
                _lastIndexUpdate = DateTime.Now;
                
                LoggingSystem.Info($"Saved playlist index with {index.Count} playlists", "YouTubePlaylist");
                
                // Fire event
                OnPlaylistIndexChanged?.Invoke();
                
                return true;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error saving playlist index: {ex.Message}", "YouTubePlaylist");
                return false;
            }
        }
        
        /// <summary>
        /// Create a new playlist
        /// </summary>
        public static YouTubePlaylist? CreatePlaylist(string name, string? description = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    LoggingSystem.Warning("Cannot create playlist with empty name", "YouTubePlaylist");
                    return null;
                }
                
                var playlist = new YouTubePlaylist(name.Trim())
                {
                    description = description?.Trim()
                };
                
                // Save the playlist
                if (SavePlaylist(playlist))
                {
                    LoggingSystem.Info($"Created new playlist: {playlist.name} (ID: {playlist.id})", "YouTubePlaylist");
                    
                    // Fire event
                    OnPlaylistCreated?.Invoke(playlist);
                    
                    return playlist;
                }
                else
                {
                    LoggingSystem.Error($"Failed to save new playlist: {playlist.name}", "YouTubePlaylist");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error creating playlist '{name}': {ex.Message}", "YouTubePlaylist");
                return null;
            }
        }
        
        /// <summary>
        /// Save a playlist to storage
        /// </summary>
        public static bool SavePlaylist(YouTubePlaylist playlist)
        {
            try
            {
                if (playlist == null || string.IsNullOrEmpty(playlist.id))
                {
                    LoggingSystem.Warning("Cannot save playlist with null data or empty ID", "YouTubePlaylist");
                    return false;
                }
                
                // Save individual playlist file  
                var playlistPath = GetPlaylistFilePath(playlist.id);
                var playlistJson = JsonConvert.SerializeObject(playlist, Formatting.Indented);
                File.WriteAllText(playlistPath, playlistJson);
                
                // Update global index
                var index = LoadPlaylistIndex();
                var playlistInfo = YouTubePlaylistInfo.FromPlaylist(playlist);
                index[playlist.id] = playlistInfo;
                
                if (SavePlaylistIndex(index))
                {
                    LoggingSystem.Debug($"Saved playlist: {playlist.name} (ID: {playlist.id})", "YouTubePlaylist");
                    
                    // Fire event
                    OnPlaylistUpdated?.Invoke(playlist);
                    
                    return true;
                }
                else
                {
                    LoggingSystem.Error($"Failed to update index after saving playlist: {playlist.name}", "YouTubePlaylist");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error saving playlist '{playlist?.name}': {ex.Message}", "YouTubePlaylist");
                return false;
            }
        }
        
        /// <summary>
        /// Load a specific playlist by ID
        /// </summary>
        public static YouTubePlaylist? LoadPlaylist(string playlistId)
        {
            try
            {
                if (string.IsNullOrEmpty(playlistId))
                {
                    return null;
                }
                
                var playlistPath = GetPlaylistFilePath(playlistId);
                if (!File.Exists(playlistPath))
                {
                    LoggingSystem.Warning($"Playlist file not found: {playlistId}", "YouTubePlaylist");
                    return null;
                }
                
                var jsonContent = File.ReadAllText(playlistPath);
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    LoggingSystem.Warning($"Playlist file is empty: {playlistId}", "YouTubePlaylist");
                    return null;
                }
                
                var playlist = JsonConvert.DeserializeObject<YouTubePlaylist>(jsonContent);
                if (playlist == null)
                {
                    LoggingSystem.Error($"Failed to deserialize playlist: {playlistId}", "YouTubePlaylist");
                    return null;
                }
                
                LoggingSystem.Debug($"Loaded playlist: {playlist.name} with {playlist.songs.Count} songs", "YouTubePlaylist");
                return playlist;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error loading playlist '{playlistId}': {ex.Message}", "YouTubePlaylist");
                return null;
            }
        }
        
        /// <summary>
        /// Delete a playlist
        /// </summary>
        public static bool DeletePlaylist(string playlistId)
        {
            try
            {
                if (string.IsNullOrEmpty(playlistId))
                {
                    return false;
                }
                
                // Delete playlist file
                var playlistPath = GetPlaylistFilePath(playlistId);
                if (File.Exists(playlistPath))
                {
                    File.Delete(playlistPath);
                }
                
                // Remove from index
                var index = LoadPlaylistIndex();
                if (index.ContainsKey(playlistId))
                {
                    var playlistName = index[playlistId].name;
                    index.Remove(playlistId);
                    
                    if (SavePlaylistIndex(index))
                    {
                        LoggingSystem.Info($"Deleted playlist: {playlistName} (ID: {playlistId})", "YouTubePlaylist");
                        
                        // Fire event
                        OnPlaylistDeleted?.Invoke(playlistId);
                        
                        return true;
                    }
                }
                
                return true; // Already removed
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error deleting playlist '{playlistId}': {ex.Message}", "YouTubePlaylist");
                return false;
            }
        }
        
        /// <summary>
        /// Get all playlists (summary info only)
        /// </summary>
        public static List<YouTubePlaylistInfo> GetAllPlaylists()
        {
            try
            {
                var index = LoadPlaylistIndex();
                return index.Values.OrderBy(p => p.name).ToList();
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error getting all playlists: {ex.Message}", "YouTubePlaylist");
                return new List<YouTubePlaylistInfo>();
            }
        }
        
        /// <summary>
        /// Update download status for a song across all playlists
        /// </summary>
        public static void UpdateSongDownloadStatus(string videoId, bool isDownloaded, string? cachedFilePath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(videoId))
                {
                    return;
                }
                
                var index = LoadPlaylistIndex();
                bool anyPlaylistUpdated = false;
                
                foreach (var playlistInfo in index.Values)
                {
                    var playlist = LoadPlaylist(playlistInfo.id);
                    if (playlist != null && playlist.ContainsSong(videoId))
                    {
                        if (playlist.UpdateSongDownloadStatus(videoId, isDownloaded, cachedFilePath))
                        {
                            SavePlaylist(playlist);
                            anyPlaylistUpdated = true;
                            LoggingSystem.Debug($"Updated download status for song {videoId} in playlist {playlist.name}", "YouTubePlaylist");
                        }
                    }
                }
                
                if (anyPlaylistUpdated)
                {
                    LoggingSystem.Info($"Updated download status for song {videoId} across playlists", "YouTubePlaylist");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error updating song download status for {videoId}: {ex.Message}", "YouTubePlaylist");
            }
        }
        
        /// <summary>
        /// Create a default playlist from existing cached songs
        /// </summary>
        public static YouTubePlaylist? CreateDefaultPlaylistFromCache()
        {
            try
            {
                LoggingSystem.Info("Creating default playlist from existing cached songs", "YouTubePlaylist");
                
                // Get all cached songs with metadata
                var cachedSongs = YouTubeMetadataManager.GetCachedSongsWithFiles();
                
                if (cachedSongs.Count == 0)
                {
                    LoggingSystem.Info("No cached songs found, not creating default playlist", "YouTubePlaylist");
                    return null;
                }
                
                // Create default playlist
                var defaultPlaylist = CreatePlaylist("My Downloaded Music", "Auto-created from existing downloads");
                if (defaultPlaylist == null)
                {
                    LoggingSystem.Error("Failed to create default playlist", "YouTubePlaylist");
                    return null;
                }
                
                // Add all cached songs to the playlist
                int addedCount = 0;
                foreach (var song in cachedSongs)
                {
                    if (defaultPlaylist.AddSong(song))
                    {
                        addedCount++;
                    }
                }
                
                // Save the updated playlist
                if (addedCount > 0)
                {
                    SavePlaylist(defaultPlaylist);
                    LoggingSystem.Info($"Created default playlist with {addedCount} cached songs", "YouTubePlaylist");
                    return defaultPlaylist;
                }
                else
                {
                    // Delete empty playlist
                    DeletePlaylist(defaultPlaylist.id);
                    LoggingSystem.Warning("No songs were added to default playlist, deleted it", "YouTubePlaylist");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error creating default playlist from cache: {ex.Message}", "YouTubePlaylist");
                return null;
            }
        }
        
        /// <summary>
        /// Get the first available playlist (for auto-loading)
        /// </summary>
        public static YouTubePlaylist? GetFirstPlaylist()
        {
            try
            {
                var playlists = GetAllPlaylists();
                if (playlists.Count == 0)
                {
                    return null;
                }
                
                var firstPlaylistInfo = playlists.OrderBy(p => p.createdDate).First();
                return LoadPlaylist(firstPlaylistInfo.id);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error getting first playlist: {ex.Message}", "YouTubePlaylist");
                return null;
            }
        }
        
        /// <summary>
        /// Clean up orphaned playlist files and invalid index entries
        /// </summary>
        public static int CleanupOrphanedData()
        {
            try
            {
                int cleanedCount = 0;
                var playlistDir = GetPlaylistsDirectory();
                
                if (!Directory.Exists(playlistDir))
                {
                    return 0;
                }
                
                var index = LoadPlaylistIndex();
                var indexChanged = false;
                
                // Check for orphaned playlist files (not in index)
                var playlistFiles = Directory.GetFiles(playlistDir, $"{PlaylistFilePrefix}*{PlaylistFileExtension}");
                
                foreach (var file in playlistFiles)
                {
                    var fileName = Path.GetFileNameWithoutExtension(file);
                    var playlistId = fileName.Substring(PlaylistFilePrefix.Length);
                    
                    if (!index.ContainsKey(playlistId))
                    {
                        File.Delete(file);
                        cleanedCount++;
                        LoggingSystem.Info($"Deleted orphaned playlist file: {fileName}", "YouTubePlaylist");
                    }
                }
                
                // Check for invalid index entries (missing files)
                var toRemove = new List<string>();
                
                foreach (var kvp in index)
                {
                    var playlistPath = GetPlaylistFilePath(kvp.Key);
                    if (!File.Exists(playlistPath))
                    {
                        toRemove.Add(kvp.Key);
                        cleanedCount++;
                        LoggingSystem.Info($"Removed invalid index entry: {kvp.Value.name}", "YouTubePlaylist");
                    }
                }
                
                foreach (var id in toRemove)
                {
                    index.Remove(id);
                    indexChanged = true;
                }
                
                if (indexChanged)
                {
                    SavePlaylistIndex(index);
                }
                
                LoggingSystem.Info($"Cleaned up {cleanedCount} orphaned playlist data entries", "YouTubePlaylist");
                return cleanedCount;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error cleaning up orphaned playlist data: {ex.Message}", "YouTubePlaylist");
                return 0;
            }
        }
    }
} 