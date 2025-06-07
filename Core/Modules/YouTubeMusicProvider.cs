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

namespace BackSpeakerMod.Core.Modules
{
    /// <summary>
    /// Provides music from YouTube downloads using yt-dlp
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
            // Use the same cache directory as YoutubeHelper
            var gameDirectory = Directory.GetCurrentDirectory();
            downloadPath = Path.Combine(gameDirectory, "Mods", "BackSpeaker", "Cache", "YouTube");
            
            // Ensure directory exists
            if (!Directory.Exists(downloadPath))
            {
                Directory.CreateDirectory(downloadPath);
                LoggingSystem.Info($"Created YouTube cache directory: {downloadPath}", "YouTube");
            }
            
            // Ensure yt-dlp is available
            EmbeddedYtDlpLoader.EnsureYtDlpPresent();
            EmbeddedYtDlpLoader.EnsureFFMPEGPresent();
            
            InitializeConfiguration();
            LoggingSystem.Info("YouTube music provider initialized", "YouTube");
        }

        public void LoadTracks(Action<List<AudioClip>, List<(string title, string artist)>>? onComplete)
        {
            var tracks = new List<AudioClip>();
            var trackInfo = new List<(string title, string artist)>();
            
            // Use async loading instead of coroutines
            LoadCachedTracksAsync(tracks, trackInfo, onComplete);
        }

        private async void LoadCachedTracksAsync(List<AudioClip> tracks, List<(string title, string artist)> trackInfo, Action<List<AudioClip>, List<(string title, string artist)>>? onComplete)
        {
            try
            {
                if (downloadPath != null && Directory.Exists(downloadPath))
                {
                    var audioFiles = Directory.GetFiles(downloadPath, "*.mp3", SearchOption.TopDirectoryOnly);
                    LoggingSystem.Info($"Found {audioFiles.Length} YouTube audio files", "YouTube");
                    
                    foreach (string filePath in audioFiles)
                    {
                        await LoadCachedAudioFileAsync(filePath, tracks, trackInfo);
                    }
                }
                else
                {
                    LoggingSystem.Warning("YouTube cache directory not found", "YouTube");
                }
                
                LoggingSystem.Info($"Loaded {tracks.Count} cached YouTube tracks", "YouTube");
                onComplete?.Invoke(tracks, trackInfo);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error loading cached tracks: {ex.Message}", "YouTube");
                onComplete?.Invoke(tracks, trackInfo); // Return what we have
            }
        }

        private async Task LoadCachedAudioFileAsync(string filePath, List<AudioClip> tracks, List<(string title, string artist)> trackInfo)
        {
            try
            {
                LoggingSystem.Debug($"Loading YouTube audio file: {filePath}", "YouTube");
                
                // Use the existing AudioHelper system
                var clip = await AudioHelper.LoadAudioFileAsync(filePath);
                
                if (clip != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    clip.name = fileName;
                    
                    tracks.Add(clip);
                    var (title, artist) = ParseYouTubeFileName(fileName);
                    trackInfo.Add((title, artist));
                    
                    if (!cachedClips.ContainsKey(fileName))
                    {
                        cachedClips[fileName] = clip;
                    }
                    
                    LoggingSystem.Info($"Loaded YouTube track: {title} by {artist}", "YouTube");
                }
                else
                {
                    LoggingSystem.Warning($"AudioHelper returned null for: {filePath}", "YouTube");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error loading audio file {filePath}: {ex.Message}", "YouTube");
            }
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
                YoutubeHelper.DownloadSong(url, (output) =>
                {
                    bool success = !string.IsNullOrEmpty(output);
                    string message = success ? "Download completed successfully" : "Download failed";
                    
                    LoggingSystem.Info($"Download result: {message}", "YouTube");
                    onComplete?.Invoke(success, message);
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
            configuration["Library"] = "yt-dlp YouTube Downloader";
            configuration["DownloadPath"] = downloadPath ?? "";
            configuration["MaxCacheSize"] = maxCacheSize;
            configuration["ExternalDependencies"] = "yt-dlp.exe (embedded)";
            configuration["SupportedFormats"] = "MP3 (extracted from best audio)";
            configuration["AudioLoader"] = "AudioHelper (same as local music)";
            configuration["Note"] = "Downloads YouTube audio using yt-dlp, loads using existing audio system";
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