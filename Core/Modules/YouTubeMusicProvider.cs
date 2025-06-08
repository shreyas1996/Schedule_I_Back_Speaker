using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Utils;
using System.Linq;
using System.Threading.Tasks;
using MelonLoader;
using BackSpeakerMod.Core.Features.Audio;

namespace BackSpeakerMod.Core.Modules
{
    /// <summary>
    /// YouTube Music Provider - integrates with YouTube playlist system
    /// Handles cached YouTube audio files and populates the YouTube playlist
    /// </summary>
    public class YouTubeMusicProvider : MonoBehaviour, IMusicSourceProvider
    {
        public MusicSourceType SourceType => MusicSourceType.YouTube;
        public string DisplayName => "YouTube Music";
        public bool IsAvailable => true;

        private Dictionary<string, object> configuration = new Dictionary<string, object>();
        private readonly Dictionary<string, AudioClip> cachedClips = new Dictionary<string, AudioClip>();
        
        private string? downloadPath;
        private const int maxCacheSize = 50;

        private void Awake()
        {
            InitializeYouTubeProvider();
        }

        private void InitializeYouTubeProvider()
        {
            // Set up YouTube download path (same as YoutubeHelper)
            var modDataPath = Path.Combine(Application.persistentDataPath, "Mods");
            downloadPath = Path.Combine(modDataPath, "BackSpeakerMod", "Cache", "YouTube");
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
            }
            
            LoggingSystem.Info($"YouTube cache directory: {downloadPath}", "YouTube");
            
            InitializeConfiguration();
            LoggingSystem.Info("YouTube music provider initialized", "YouTube");
        }

        /// <summary>
        /// Load tracks method for compatibility with IMusicSourceProvider interface
        /// For YouTube, this should populate the YouTube playlist instead of returning AudioClips
        /// </summary>
        public void LoadTracks(Action<List<AudioClip>, List<(string title, string artist)>>? onComplete)
        {
            // For YouTube sessions, we need to populate the playlist instead of returning AudioClips
            LoggingSystem.Info("Loading YouTube tracks - populating YouTube playlist", "YouTube");
            
            // Load cached songs into the YouTube playlist (async)
            LoadCachedSongsIntoPlaylistAsync(onComplete);
        }

        /// <summary>
        /// Load cached YouTube songs and populate the YouTube playlist (async wrapper)
        /// </summary>
        private async void LoadCachedSongsIntoPlaylistAsync(Action<List<AudioClip>, List<(string title, string artist)>>? onComplete)
        {
            await LoadCachedSongsIntoPlaylist();
            
            // Return empty lists since YouTube uses playlist system
            var emptyTracks = new List<AudioClip>();
            var emptyTrackInfo = new List<(string title, string artist)>();
            onComplete?.Invoke(emptyTracks, emptyTrackInfo);
        }

        /// <summary>
        /// Load cached YouTube songs and populate the YouTube playlist
        /// </summary>
        private async Task LoadCachedSongsIntoPlaylist()
        {
            try
            {
                LoggingSystem.Info("Loading cached YouTube songs with metadata", "YouTube");
                
                // Clean up orphaned metadata first
                YouTubeMetadataManager.CleanupMetadata();
                
                // Get songs that have both metadata and files
                var cachedSongs = YouTubeMetadataManager.GetCachedSongsWithFiles();
                
                if (cachedSongs.Count > 0)
                {
                    LoggingSystem.Info($"Found {cachedSongs.Count} cached YouTube songs with metadata", "YouTube");
                    _cachedSongDetails = cachedSongs;
                }
                else
                {
                    LoggingSystem.Info("No cached songs with metadata found, scanning for audio files", "YouTube");
                    
                    // Fallback: scan for audio files without metadata
                    if (downloadPath != null && Directory.Exists(downloadPath))
                    {
                        var audioFiles = Directory.GetFiles(downloadPath, "*.mp3", SearchOption.TopDirectoryOnly);
                        LoggingSystem.Info($"Found {audioFiles.Length} cached YouTube audio files", "YouTube");
                        
                        var fallbackSongs = new List<SongDetails>();
                        
                        foreach (string filePath in audioFiles)
                        {
                            try
                            {
                                var songDetails = await CreateSongDetailsFromCachedFile(filePath);
                                if (songDetails != null)
                                {
                                    fallbackSongs.Add(songDetails);
                                    
                                    // Save metadata for future use
                                    YouTubeMetadataManager.AddOrUpdateSong(songDetails);
                                    
                                    LoggingSystem.Debug($"Created and saved metadata for: {songDetails.title}", "YouTube");
                                }
                            }
                            catch (Exception ex)
                            {
                                LoggingSystem.Warning($"Error processing cached file {filePath}: {ex.Message}", "YouTube");
                            }
                        }
                        
                        _cachedSongDetails = fallbackSongs;
                        LoggingSystem.Info($"Created metadata for {fallbackSongs.Count} cached songs", "YouTube");
                    }
                    else
                    {
                        LoggingSystem.Warning("YouTube cache directory not found", "YouTube");
                        _cachedSongDetails = new List<SongDetails>();
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error loading cached songs into playlist: {ex.Message}", "YouTube");
                _cachedSongDetails = new List<SongDetails>();
            }
        }
        
        // Store cached song details for later playlist population
        private List<SongDetails> _cachedSongDetails = new List<SongDetails>();
        
        /// <summary>
        /// Add cached songs to a YouTube AudioSession (called by external systems)
        /// </summary>
        public void AddCachedSongsToSession(AudioSession audioSession)
        {
            try
            {
                // If we don't have cached song details yet, load them first
                if (_cachedSongDetails.Count == 0)
                {
                    LoggingSystem.Info("No cached songs loaded yet - loading them now", "YouTube");
                    var loadTask = LoadCachedSongsIntoPlaylist();
                    // Since this is called from UI thread, we need to wait synchronously
                    loadTask.Wait();
                }
                
                if (_cachedSongDetails.Count == 0)
                {
                    LoggingSystem.Debug("No cached songs available to add to session", "YouTube");
                    return;
                }
                
                LoggingSystem.Info($"Adding {_cachedSongDetails.Count} cached YouTube songs to session", "YouTube");
                
                int addedCount = 0;
                foreach (var songDetails in _cachedSongDetails)
                {
                    if (!audioSession.ContainsYouTubeSong(songDetails.url ?? ""))
                    {
                        bool added = audioSession.AddYouTubeSong(songDetails);
                        if (added)
                        {
                            addedCount++;
                            LoggingSystem.Debug($"Added cached song to session: {songDetails.title}", "YouTube");
                        }
                    }
                    else
                    {
                        LoggingSystem.Debug($"Song already in session: {songDetails.title}", "YouTube");
                        addedCount++;
                    }
                }
                
                LoggingSystem.Info($"Successfully added {addedCount} cached YouTube songs to AudioSession (total session tracks: {audioSession.TrackCount})", "YouTube");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error adding cached songs to session: {ex.Message}", "YouTube");
            }
        }

        /// <summary>
        /// Create SongDetails object from a cached audio file
        /// </summary>
        private async Task<SongDetails?> CreateSongDetailsFromCachedFile(string filePath)
        {
            try
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                var (title, artist) = ParseYouTubeFileName(fileName);
                
                // Extract video ID from filename (assumes filename format includes video ID)
                string videoId = ExtractVideoIdFromFileName(fileName);
                
                // Create SongDetails object
                var songDetails = new SongDetails
                {
                    title = title,
                    artist = artist,
                    id = videoId,
                    url = $"https://www.youtube.com/watch?v={videoId}",
                    isDownloaded = true,
                    cachedFilePath = filePath,
                    downloadTimestamp = File.GetCreationTime(filePath)
                };

                // Get file size
                try
                {
                    songDetails.fileSizeBytes = new FileInfo(filePath).Length;
                }
                catch (Exception ex)
                {
                    LoggingSystem.Warning($"Could not get file size for {filePath}: {ex.Message}", "YouTube");
                }

                // Try to get duration from audio file
                try
                {
                    var audioClip = await AudioHelper.LoadAudioFileAsync(filePath);
                    if (audioClip != null)
                    {
                        songDetails.duration = (int)audioClip.length;
                        
                        // Cache the clip for potential immediate playback
                        if (!cachedClips.ContainsKey(fileName))
                        {
                            cachedClips[fileName] = audioClip;
                        }
                        
                        LoggingSystem.Debug($"Created SongDetails for cached file: {title} ({audioClip.length:F1}s)", "YouTube");
                    }
                }
                catch (Exception ex)
                {
                    LoggingSystem.Warning($"Could not load audio to get duration for {filePath}: {ex.Message}", "YouTube");
                }

                return songDetails;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error creating SongDetails from cached file {filePath}: {ex.Message}", "YouTube");
                return null;
            }
        }

        /// <summary>
        /// Extract video ID from filename
        /// Assumes filename contains the YouTube video ID (11 characters)
        /// </summary>
        private string ExtractVideoIdFromFileName(string fileName)
        {
            try
            {
                // Look for 11-character sequences that could be video IDs
                // YouTube video IDs are exactly 11 characters long
                for (int i = 0; i <= fileName.Length - 11; i++)
                {
                    string candidate = fileName.Substring(i, 11);
                    
                    // Check if it looks like a video ID (alphanumeric, underscore, hyphen)
                    if (IsValidYouTubeVideoId(candidate))
                    {
                        return candidate;
                    }
                }
                
                // If no valid video ID found, use a hash of the filename
                return fileName.GetHashCode().ToString("X8");
            }
            catch
            {
                return fileName.GetHashCode().ToString("X8");
            }
        }

        /// <summary>
        /// Check if a string looks like a valid YouTube video ID
        /// </summary>
        private bool IsValidYouTubeVideoId(string candidate)
        {
            if (candidate.Length != 11) return false;
            
            foreach (char c in candidate)
            {
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
                {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// Parse YouTube filename to extract title and artist
        /// yt-dlp typically downloads files with format: "Title.mp3" or "Artist - Title.mp3"
        /// </summary>
        private (string title, string artist) ParseYouTubeFileName(string fileName)
        {
            try
            {
                // Clean up the filename
                string cleanName = fileName.Replace("_", " ").Trim();
                
                // Check if it contains " - " separator (common format: Artist - Title)
                if (cleanName.Contains(" - "))
                {
                    var parts = cleanName.Split(new string[] { " - " }, 2, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        return (parts[1].Trim(), parts[0].Trim());
                    }
                }
                
                // If no separator, treat entire name as title
                return (cleanName, "YouTube");
            }
            catch (Exception ex)
            {
                LoggingSystem.Warning($"Error parsing filename {fileName}: {ex.Message}", "YouTube");
                return (fileName, "YouTube");
            }
        }

        /// <summary>
        /// Download a song from YouTube URL
        /// </summary>
        public void DownloadFromYouTube(string url, Action<bool, string>? onComplete)
        {
            LoggingSystem.Info($"YouTube download requested: {url}", "YouTube");
            
            try
            {
                // First get song details, then download
                YoutubeHelper.GetSongDetails(url, (songDetailsList) =>
                {
                    if (songDetailsList != null && songDetailsList.Count > 0)
                    {
                        var songDetails = songDetailsList[0]; // Take the first song
                        
                        YoutubeHelper.DownloadSong(songDetails, (success) =>
                        {
                            string message = success ? "Download completed successfully" : "Download failed";
                            LoggingSystem.Info($"Download result: {message}", "YouTube");
                            onComplete?.Invoke(success, message);
                        });
                    }
                    else
                    {
                        LoggingSystem.Error("Could not get song details for download", "YouTube");
                        onComplete?.Invoke(false, "Could not get song details for download");
                    }
                });
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Download error: {ex.Message}", "YouTube");
                onComplete?.Invoke(false, $"Download error: {ex.Message}");
            }
        }

        /// <summary>
        /// Get song details from YouTube URL
        /// </summary>
        public void GetSongDetails(string url, Action<List<SongDetails>> onComplete)
        {
            LoggingSystem.Info($"Getting song details for: {url}", "YouTube");
            YoutubeHelper.GetSongDetails(url, onComplete);
        }

        private void InitializeConfiguration()
        {
            configuration["Library"] = "yt-dlp YouTube Streaming";
            configuration["DownloadPath"] = downloadPath ?? "";
            configuration["MaxCacheSize"] = maxCacheSize;
            configuration["ExternalDependencies"] = "yt-dlp.exe (embedded)";
            configuration["SupportedFormats"] = "MP3 (cached) + Streaming";
            configuration["AudioLoader"] = "YouTube Streaming System";
            configuration["Note"] = "Uses playlist-based streaming with smart caching";
        }

        public Dictionary<string, object> GetConfiguration()
        {
            return new Dictionary<string, object>(configuration);
        }

        public void ApplyConfiguration(Dictionary<string, object> config)
        {
            // Future configuration options could be added here
            if (config.ContainsKey("MaxCacheSize"))
            {
                // Could implement cache size limits
            }
        }

        public void Cleanup()
        {
            cachedClips.Clear();
            LoggingSystem.Info("YouTube provider cleaned up", "YouTube");
        }

        /// <summary>
        /// Clear all cached downloads
        /// </summary>
        public void ClearCache()
        {
            try
            {
                if (downloadPath != null && Directory.Exists(downloadPath))
                {
                    var files = Directory.GetFiles(downloadPath, "*.mp3");
                    foreach (var file in files)
                    {
                        File.Delete(file);
                    }
                    LoggingSystem.Info($"Cleared {files.Length} cached YouTube files", "YouTube");
                }
                
                cachedClips.Clear();
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error clearing cache: {ex.Message}", "YouTube");
            }
        }
    }
} 