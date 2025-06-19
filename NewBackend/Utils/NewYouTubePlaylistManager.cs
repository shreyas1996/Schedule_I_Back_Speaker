using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace BackSpeakerMod.NewBackend.Utils
{
    /// <summary>
    /// New YouTube playlist info for NewBackend
    /// </summary>
    [Serializable]
    public class NewYouTubePlaylistInfo
    {
        public string id = "";
        public string name = "";
        public string description = "";
        public DateTime created = DateTime.Now;
        public DateTime lastModified = DateTime.Now;
        public int songCount = 0;
    }

    /// <summary>
    /// New YouTube playlist for NewBackend
    /// </summary>
    [Serializable]
    public class NewYouTubePlaylist
    {
        public string id = "";
        public string name = "";
        public string description = "";
        public DateTime created = DateTime.Now;
        public DateTime lastModified = DateTime.Now;
        public List<NewSongDetails> songs = new List<NewSongDetails>();

        public NewYouTubePlaylist() { }

        public NewYouTubePlaylist(string name)
        {
            this.name = name;
            this.id = Guid.NewGuid().ToString();
            this.created = DateTime.Now;
            this.lastModified = DateTime.Now;
        }

        public bool AddSong(NewSongDetails song)
        {
            if (song == null || string.IsNullOrEmpty(song.url)) return false;
            
            // Check if song already exists
            if (songs.Any(s => s.url == song.url)) return false;
            
            songs.Add(song);
            lastModified = DateTime.Now;
            return true;
        }

        public bool RemoveSong(string url)
        {
            if (string.IsNullOrEmpty(url)) return false;
            
            var songToRemove = songs.FirstOrDefault(s => s.url == url);
            if (songToRemove != null)
            {
                songs.Remove(songToRemove);
                lastModified = DateTime.Now;
                return true;
            }
            return false;
        }

        public bool ContainsSong(string url)
        {
            return !string.IsNullOrEmpty(url) && songs.Any(s => s.url == url);
        }

        public NewSongDetails GetSong(string url)
        {
            return songs.FirstOrDefault(s => s.url == url);
        }

        public void Clear()
        {
            songs.Clear();
            lastModified = DateTime.Now;
        }
    }

    /// <summary>
    /// New YouTube playlist manager for NewBackend
    /// </summary>
    public static class NewYouTubePlaylistManager
    {
        private static readonly string PlaylistsIndexFileName = "new_youtube_playlists.json";
        private static readonly string PlaylistFilePrefix = "new_playlist_";
        private static readonly string PlaylistFileExtension = ".json";
        
        private static string _playlistsDirectory;
        private static Dictionary<string, NewYouTubePlaylistInfo> _cachedIndex;
        private static DateTime _lastIndexUpdate = DateTime.MinValue;
        private static readonly TimeSpan IndexCacheExpiry = TimeSpan.FromMinutes(2);

        /// <summary>
        /// Get the playlists directory path
        /// </summary>
        private static string GetPlaylistsDirectory()
        {
            if (_playlistsDirectory == null)
            {
                var gameDirectory = Directory.GetCurrentDirectory();
                var cacheDirectory = Path.Combine(gameDirectory, "Mods", "BackSpeaker", "Cache", "YouTube");
                _playlistsDirectory = Path.Combine(cacheDirectory, "NewPlaylists");
            }
            
            // Ensure directory exists
            if (!Directory.Exists(_playlistsDirectory))
            {
                Directory.CreateDirectory(_playlistsDirectory);
                NewLoggingSystem.Info($"Created NewBackend YouTube playlists directory: {_playlistsDirectory}", "NewYouTubePlaylistManager");
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
        public static Dictionary<string, NewYouTubePlaylistInfo> LoadPlaylistIndex()
        {
            // Return cached data if still valid
            if (_cachedIndex != null && DateTime.Now - _lastIndexUpdate < IndexCacheExpiry)
            {
                return new Dictionary<string, NewYouTubePlaylistInfo>(_cachedIndex);
            }
            
            try
            {
                var indexPath = GetIndexFilePath();
                
                if (!File.Exists(indexPath))
                {
                    NewLoggingSystem.Debug("No NewBackend playlist index found, returning empty index", "NewYouTubePlaylistManager");
                    _cachedIndex = new Dictionary<string, NewYouTubePlaylistInfo>();
                    _lastIndexUpdate = DateTime.Now;
                    return new Dictionary<string, NewYouTubePlaylistInfo>(_cachedIndex);
                }
                
                var jsonContent = File.ReadAllText(indexPath);
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    NewLoggingSystem.Debug("NewBackend playlist index file is empty, returning empty index", "NewYouTubePlaylistManager");
                    _cachedIndex = new Dictionary<string, NewYouTubePlaylistInfo>();
                    _lastIndexUpdate = DateTime.Now;
                    return new Dictionary<string, NewYouTubePlaylistInfo>(_cachedIndex);
                }
                
                var index = JsonConvert.DeserializeObject<Dictionary<string, NewYouTubePlaylistInfo>>(jsonContent);
                if (index == null)
                {
                    NewLoggingSystem.Warning("Failed to deserialize NewBackend playlist index, returning empty index", "NewYouTubePlaylistManager");
                    _cachedIndex = new Dictionary<string, NewYouTubePlaylistInfo>();
                }
                else
                {
                    NewLoggingSystem.Info($"Loaded NewBackend playlist index with {index.Count} playlists", "NewYouTubePlaylistManager");
                    _cachedIndex = index;
                }
                
                _lastIndexUpdate = DateTime.Now;
                return new Dictionary<string, NewYouTubePlaylistInfo>(_cachedIndex);
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error loading NewBackend playlist index: {ex.Message}", "NewYouTubePlaylistManager");
                _cachedIndex = new Dictionary<string, NewYouTubePlaylistInfo>();
                _lastIndexUpdate = DateTime.Now;
                return new Dictionary<string, NewYouTubePlaylistInfo>(_cachedIndex);
            }
        }
        
        /// <summary>
        /// Save the global playlists index
        /// </summary>
        private static bool SavePlaylistIndex(Dictionary<string, NewYouTubePlaylistInfo> index)
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
                _cachedIndex = new Dictionary<string, NewYouTubePlaylistInfo>(index);
                _lastIndexUpdate = DateTime.Now;
                
                NewLoggingSystem.Info($"Saved NewBackend playlist index with {index.Count} playlists", "NewYouTubePlaylistManager");
                
                return true;
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error saving NewBackend playlist index: {ex.Message}", "NewYouTubePlaylistManager");
                return false;
            }
        }
        
        /// <summary>
        /// Create a new playlist
        /// </summary>
        public static NewYouTubePlaylist CreatePlaylist(string name, string description = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    NewLoggingSystem.Warning("Cannot create playlist with empty name", "NewYouTubePlaylistManager");
                    return null;
                }
                
                var playlist = new NewYouTubePlaylist(name.Trim())
                {
                    description = description?.Trim() ?? ""
                };
                
                // Save the playlist
                if (SavePlaylist(playlist))
                {
                    NewLoggingSystem.Info($"Created new NewBackend playlist: {playlist.name} (ID: {playlist.id})", "NewYouTubePlaylistManager");
                    return playlist;
                }
                else
                {
                    NewLoggingSystem.Error($"Failed to save new NewBackend playlist: {playlist.name}", "NewYouTubePlaylistManager");
                    return null;
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error creating NewBackend playlist: {ex.Message}", "NewYouTubePlaylistManager");
                return null;
            }
        }
        
        /// <summary>
        /// Save a playlist to disk
        /// </summary>
        public static bool SavePlaylist(NewYouTubePlaylist playlist)
        {
            try
            {
                if (playlist == null || string.IsNullOrEmpty(playlist.id))
                {
                    NewLoggingSystem.Warning("Cannot save playlist with null or empty ID", "NewYouTubePlaylistManager");
                    return false;
                }
                
                // Save playlist file
                var playlistPath = GetPlaylistFilePath(playlist.id);
                var jsonContent = JsonConvert.SerializeObject(playlist, Formatting.Indented);
                File.WriteAllText(playlistPath, jsonContent);
                
                // Update index
                var index = LoadPlaylistIndex();
                var playlistInfo = new NewYouTubePlaylistInfo
                {
                    id = playlist.id,
                    name = playlist.name,
                    description = playlist.description,
                    created = playlist.created,
                    lastModified = playlist.lastModified,
                    songCount = playlist.songs.Count
                };
                
                index[playlist.id] = playlistInfo;
                
                bool success = SavePlaylistIndex(index);
                if (success)
                {
                    NewLoggingSystem.Debug($"Saved NewBackend playlist: {playlist.name} with {playlist.songs.Count} songs", "NewYouTubePlaylistManager");
                }
                
                return success;
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error saving NewBackend playlist: {ex.Message}", "NewYouTubePlaylistManager");
                return false;
            }
        }
        
        /// <summary>
        /// Load a playlist from disk
        /// </summary>
        public static NewYouTubePlaylist LoadPlaylist(string playlistId)
        {
            try
            {
                if (string.IsNullOrEmpty(playlistId))
                {
                    NewLoggingSystem.Warning("Cannot load playlist with empty ID", "NewYouTubePlaylistManager");
                    return null;
                }
                
                var playlistPath = GetPlaylistFilePath(playlistId);
                
                if (!File.Exists(playlistPath))
                {
                    NewLoggingSystem.Warning($"NewBackend playlist file not found: {playlistPath}", "NewYouTubePlaylistManager");
                    return null;
                }
                
                var jsonContent = File.ReadAllText(playlistPath);
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    NewLoggingSystem.Warning($"NewBackend playlist file is empty: {playlistPath}", "NewYouTubePlaylistManager");
                    return null;
                }
                
                var playlist = JsonConvert.DeserializeObject<NewYouTubePlaylist>(jsonContent);
                if (playlist != null)
                {
                    NewLoggingSystem.Debug($"Loaded NewBackend playlist: {playlist.name} with {playlist.songs.Count} songs", "NewYouTubePlaylistManager");
                }
                
                return playlist;
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error loading NewBackend playlist {playlistId}: {ex.Message}", "NewYouTubePlaylistManager");
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
                    NewLoggingSystem.Warning("Cannot delete playlist with empty ID", "NewYouTubePlaylistManager");
                    return false;
                }
                
                // Delete playlist file
                var playlistPath = GetPlaylistFilePath(playlistId);
                if (File.Exists(playlistPath))
                {
                    File.Delete(playlistPath);
                    NewLoggingSystem.Debug($"Deleted NewBackend playlist file: {playlistPath}", "NewYouTubePlaylistManager");
                }
                
                // Update index
                var index = LoadPlaylistIndex();
                if (index.ContainsKey(playlistId))
                {
                    var playlistName = index[playlistId].name;
                    index.Remove(playlistId);
                    
                    bool success = SavePlaylistIndex(index);
                    if (success)
                    {
                        NewLoggingSystem.Info($"Deleted NewBackend playlist: {playlistName} (ID: {playlistId})", "NewYouTubePlaylistManager");
                    }
                    
                    return success;
                }
                
                NewLoggingSystem.Warning($"NewBackend playlist not found in index: {playlistId}", "NewYouTubePlaylistManager");
                return true; // Consider it successful if it doesn't exist
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error deleting NewBackend playlist {playlistId}: {ex.Message}", "NewYouTubePlaylistManager");
                return false;
            }
        }
        
        /// <summary>
        /// Get all playlists
        /// </summary>
        public static List<NewYouTubePlaylistInfo> GetAllPlaylists()
        {
            try
            {
                var index = LoadPlaylistIndex();
                return index.Values.ToList();
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error getting all NewBackend playlists: {ex.Message}", "NewYouTubePlaylistManager");
                return new List<NewYouTubePlaylistInfo>();
            }
        }
        
        /// <summary>
        /// Create a default playlist if none exist
        /// </summary>
        public static NewYouTubePlaylist CreateDefaultPlaylist()
        {
            try
            {
                var playlists = GetAllPlaylists();
                if (playlists.Count == 0)
                {
                    NewLoggingSystem.Info("No NewBackend playlists found, creating default playlist", "NewYouTubePlaylistManager");
                    return CreatePlaylist("My Music", "Default playlist for YouTube tracks");
                }
                
                return LoadPlaylist(playlists[0].id);
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error creating default NewBackend playlist: {ex.Message}", "NewYouTubePlaylistManager");
                return null;
            }
        }
    }
} 