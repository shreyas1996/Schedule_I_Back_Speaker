using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using BackSpeakerMod.S1Wrapper;
using BackSpeakerMod.S1Wrapper.Interfaces;
using BackSpeakerMod.NewBackend.Utils;

namespace BackSpeakerMod.NewBackend
{
    /// <summary>
    /// Manages music sources and playlist operations
    /// </summary>
    public class PlaylistManager
    {
        // Source track caches
        private Dictionary<string, List<NewSongDetails>>? _sourceTracks;
        
        public IEnumerator Initialize()
        {
            NewLoggingSystem.Info("Initializing PlaylistManager", "PlaylistManager");
            
            // Initialize source collections
            _sourceTracks = new Dictionary<string, List<NewSongDetails>>();
            _sourceTracks["Jukebox"] = new List<NewSongDetails>();
            _sourceTracks["LocalFolder"] = new List<NewSongDetails>();
            _sourceTracks["YouTube"] = new List<NewSongDetails>();
            
            // Load tracks from all sources
            yield return LoadJukeboxTracks();
            yield return LoadLocalFolderTracks();
            yield return LoadYouTubeTracks();
            
            NewLoggingSystem.Info("✓ PlaylistManager initialized", "PlaylistManager");
        }
        
        private IEnumerator LoadJukeboxTracks()
        {
            try
            {
                // Use S1Factory to find jukeboxes
                var jukeboxes = S1Factory.FindJukeboxes();
                if (jukeboxes != null && jukeboxes.Length > 0)
                {
                    foreach (var jukebox in jukeboxes)
                    {
                        var tracks = jukebox.GetTracks();
                        foreach (var track in tracks)
                        {
                            var songDetails = new NewSongDetails
                            {
                                title = track.name ?? "Unknown Track",
                                artist = "Jukebox",
                                cachedFilePath = "",
                                duration = (int)track.length
                            };
                            _sourceTracks["Jukebox"].Add(songDetails);
                        }
                    }
                    
                    NewLoggingSystem.Info($"✓ Loaded {_sourceTracks["Jukebox"].Count} jukebox tracks", "PlaylistManager");
                }
                else
                {
                    NewLoggingSystem.Warning("No jukeboxes found", "PlaylistManager");
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to load jukebox tracks: {ex}", "PlaylistManager");
            }
            
            yield break;
        }
        
        private IEnumerator LoadLocalFolderTracks()
        {
            try
            {
                string[] audioPaths = {
                    Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads")
                };
                
                // Use AudioHelper supported extensions
                string[] audioExtensions = AudioLoaderHelper.GetSupportedExtensions();
                
                foreach (string audioPath in audioPaths)
                {
                    if (Directory.Exists(audioPath))
                    {
                        foreach (string extension in audioExtensions)
                        {
                            var files = Directory.GetFiles(audioPath, $"*{extension}", SearchOption.TopDirectoryOnly);
                            foreach (string file in files)
                            {
                                var songDetails = new NewSongDetails
                                {
                                    title = Path.GetFileNameWithoutExtension(file),
                                    artist = "Local File",
                                    cachedFilePath = file,
                                    isDownloaded = true
                                };
                                _sourceTracks["LocalFolder"].Add(songDetails);
                            }
                        }
                    }
                }
                
                NewLoggingSystem.Info($"✓ Loaded {_sourceTracks["LocalFolder"].Count} local folder tracks", "PlaylistManager");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to load local folder tracks: {ex}", "PlaylistManager");
            }
            
            yield break;
        }
        
        private IEnumerator LoadYouTubeTracks()
        {
            try
            {
                var playlists = NewYouTubePlaylistManager.GetAllPlaylists();
                
                foreach (var playlistInfo in playlists)
                {
                    var playlist = NewYouTubePlaylistManager.LoadPlaylist(playlistInfo.id);
                    if (playlist != null && playlist.songs != null)
                    {
                        // No conversion needed - already NewSongDetails
                        foreach (var song in playlist.songs)
                        {
                            _sourceTracks["YouTube"].Add(song);
                        }
                    }
                }
                
                NewLoggingSystem.Info($"✓ Loaded {_sourceTracks["YouTube"].Count} YouTube tracks", "PlaylistManager");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to load YouTube tracks: {ex}", "PlaylistManager");
            }
            
            yield break;
        }
        
        // Public API
        public List<NewSongDetails> GetJukeboxTracks() => _sourceTracks.ContainsKey("Jukebox") ? _sourceTracks["Jukebox"] : new List<NewSongDetails>();
        public List<NewSongDetails> GetLocalFolderTracks() => _sourceTracks.ContainsKey("LocalFolder") ? _sourceTracks["LocalFolder"] : new List<NewSongDetails>();
        public List<NewSongDetails> GetYouTubeTracks() => _sourceTracks.ContainsKey("YouTube") ? _sourceTracks["YouTube"] : new List<NewSongDetails>();
        
        public List<string> GetPlaylistNames(string source)
        {
            try
            {
                switch (source)
                {
                    case "YouTube":
                        var playlists = NewYouTubePlaylistManager.GetAllPlaylists();
                        var names = new List<string>();
                        foreach (var playlist in playlists)
                        {
                            names.Add(playlist.name);
                        }
                        return names;
                    
                    case "LocalFolder":
                        return new List<string> { "All Local Tracks" };
                    
                    case "Jukebox":
                        return new List<string> { "All Jukebox Tracks" };
                    
                    default:
                        return new List<string>();
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error getting playlist names for {source}: {ex}", "PlaylistManager");
                return new List<string>();
            }
        }
        
        public List<NewSongDetails> GetPlaylistTracks(string playlistName, string source)
        {
            try
            {
                switch (source)
                {
                    case "YouTube":
                        var playlists = NewYouTubePlaylistManager.GetAllPlaylists();
                        foreach (var playlistInfo in playlists)
                        {
                            if (playlistInfo.name == playlistName)
                            {
                                var playlist = NewYouTubePlaylistManager.LoadPlaylist(playlistInfo.id);
                                if (playlist?.songs != null)
                                {
                                    // No conversion needed - already NewSongDetails
                                    return playlist.songs;
                                }
                                return new List<NewSongDetails>();
                            }
                        }
                        return new List<NewSongDetails>();
                    
                    case "LocalFolder":
                        return GetLocalFolderTracks();
                    
                    case "Jukebox":
                        return GetJukeboxTracks();
                    
                    default:
                        return new List<NewSongDetails>();
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error getting playlist tracks for {playlistName}: {ex}", "PlaylistManager");
                return new List<NewSongDetails>();
            }
        }
        
        public bool CreatePlaylist(string playlistName, string source)
        {
            try
            {
                switch (source)
                {
                    case "YouTube":
                        var playlist = NewYouTubePlaylistManager.CreatePlaylist(playlistName);
                        return playlist != null;
                    
                    default:
                        NewLoggingSystem.Warning($"Creating playlists not supported for {source}", "PlaylistManager");
                        return false;
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error creating playlist {playlistName}: {ex}", "PlaylistManager");
                return false;
            }
        }
        
        public bool DeletePlaylist(string playlistName, string source)
        {
            try
            {
                switch (source)
                {
                    case "YouTube":
                        var playlists = NewYouTubePlaylistManager.GetAllPlaylists();
                        foreach (var playlistInfo in playlists)
                        {
                            if (playlistInfo.name == playlistName)
                            {
                                return NewYouTubePlaylistManager.DeletePlaylist(playlistInfo.id);
                            }
                        }
                        return false;
                    
                    default:
                        NewLoggingSystem.Warning($"Deleting playlists not supported for {source}", "PlaylistManager");
                        return false;
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error deleting playlist {playlistName}: {ex}", "PlaylistManager");
                return false;
            }
        }
        
        public void Shutdown()
        {
            NewLoggingSystem.Info("Shutting down PlaylistManager", "PlaylistManager");
            
            // Clear caches
            _sourceTracks?.Clear();
            _sourceTracks = null;
            
            NewLoggingSystem.Info("✓ PlaylistManager shutdown complete", "PlaylistManager");
        }
    }
} 