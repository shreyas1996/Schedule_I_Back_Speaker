using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Utils;
using MelonLoader;
using System.Threading.Tasks;
using System.Linq;

namespace BackSpeakerMod.Core.Modules
{
    /// <summary>
    /// Provides music from local folder files using NAudio for reliable audio loading
    /// </summary>
    public class LocalFolderMusicProvider : MonoBehaviour, IMusicSourceProvider
    {
        public MusicSourceType SourceType => MusicSourceType.LocalFolder;
        public string DisplayName => "Local Music Folder";
        public bool IsAvailable 
        { 
            get 
            {
                // Ensure folder exists before checking availability
                EnsureMusicFolderExists();
                return Directory.Exists(GetMusicFolderPath());
            }
        }

        private Dictionary<string, object> configuration = new Dictionary<string, object>();
        private bool isLoading = false;
        private readonly Dictionary<string, AudioClip> cachedClips = new Dictionary<string, AudioClip>();
        private const int maxCacheSize = 10; // Keep fewer clips in memory compared to YouTube due to larger file sizes
        private bool isDestroyed = false;

        public LocalFolderMusicProvider() : base() { }

        public void LoadTracks(Action<List<AudioClip>, List<(string title, string artist)>> onComplete)
        {
            if (isLoading)
            {
                LoggingSystem.Warning("Track loading already in progress", "LocalFolder");
                return;
            }

            isLoading = true;
            var coroutine = LoadLocalTracksCoroutine(onComplete);
            MelonCoroutines.Start(coroutine);
        }

        private IEnumerator LoadLocalTracksCoroutine(Action<List<AudioClip>, List<(string title, string artist)>> onComplete)
        {
            var tracks = new List<AudioClip>();
            var trackInfo = new List<(string title, string artist)>();

            string musicPath = GetMusicFolderPath();
            EnsureMusicFolderExists();
            
            LoggingSystem.Info($"Loading local tracks from: {musicPath}", "LocalFolder");

            if (!Directory.Exists(musicPath))
            {
                LoggingSystem.Warning($"Music directory does not exist: {musicPath}", "LocalFolder");
                isLoading = false;
                onComplete?.Invoke(tracks, trackInfo);
                yield break;
            }

            // Get all supported audio files
            var supportedExtensions = AudioHelper.GetSupportedExtensions();
            var allFiles = new List<string>();
            
            foreach (string extension in supportedExtensions)
            {
                var files = Directory.GetFiles(musicPath, "*" + extension, SearchOption.TopDirectoryOnly);
                allFiles.AddRange(files);
            }

            LoggingSystem.Info($"Found {allFiles.Count} supported audio files", "LocalFolder");

            // If we have too many cached clips, remove oldest ones
            while (cachedClips.Count > maxCacheSize)
            {
                var oldestKey = cachedClips.Keys.First();
                var oldClip = cachedClips[oldestKey];
                if (oldClip != null)
                {
                    Destroy(oldClip);
                }
                cachedClips.Remove(oldestKey);
                LoggingSystem.Debug($"Removed old cached clip: {oldestKey}", "LocalFolder");
            }

            // Load files sequentially to avoid memory issues
            foreach (var filePath in allFiles)
            {
                if (isDestroyed) break; // Stop if component is being destroyed

                string fileName = Path.GetFileNameWithoutExtension(filePath);
                LoggingSystem.Debug($"Loading: {fileName}", "LocalFolder");

                // First verify the source file still exists
                if (!File.Exists(filePath))
                {
                    LoggingSystem.Warning($"Source file no longer exists: {filePath}", "LocalFolder");
                    continue;
                }

                // Check if we have a cached clip and verify its validity
                if (cachedClips.TryGetValue(fileName, out AudioClip? cachedClip))
                {
                    if (cachedClip != null && cachedClip.loadState == AudioDataLoadState.Loaded && cachedClip.length > 0)
                    {
                        // Verify the cache is still valid by comparing file modify times
                        try
                        {
                            var fileInfo = new FileInfo(filePath);
                            if (fileInfo.Length > 0)
                            {
                                LoggingSystem.Debug($"Using cached clip for: {fileName}", "LocalFolder");
                                tracks.Add(cachedClip);
                                trackInfo.Add((FormatTrackTitle(fileName), "Local File"));
                                continue;
                            }
                            else
                            {
                                LoggingSystem.Warning($"Source file is empty: {fileName}", "LocalFolder");
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggingSystem.Warning($"Error checking source file {fileName}: {ex.Message}", "LocalFolder");
                        }
                    }

                    // If we get here, the cached clip is invalid
                    if (cachedClip != null)
                    {
                        Destroy(cachedClip);
                    }
                    cachedClips.Remove(fileName);
                    LoggingSystem.Debug($"Removed invalid cached clip for: {fileName}", "LocalFolder");
                }

                // Add a delay before each load to let the audio system stabilize
                yield return new WaitForSeconds(0.25f);

                // Declare variables outside try block to maintain scope
                bool loadingComplete = false;
                AudioClip? loadedClip = null;
                Exception? loadError = null;
                var loadTask = Task.Run(async () => {
                    try 
                    {
                        loadedClip = await AudioHelper.LoadAudioFileAsync(filePath);
                    }
                    catch (Exception ex)
                    {
                        loadError = ex;
                    }
                    finally
                    {
                        loadingComplete = true;
                    }
                });

                // Wait for loading to complete
                while (!loadingComplete)
                {
                    yield return null;
                }

                // Handle loading results
                if (loadError != null)
                {
                    LoggingSystem.Error($"Failed to load {fileName}: {loadError.Message}", "LocalFolder");
                    continue;
                }

                if (loadedClip != null && !isDestroyed)
                {
                    try
                    {
                        // If we're at cache limit, remove oldest clip first
                        if (cachedClips.Count >= maxCacheSize)
                        {
                            var oldestKey = cachedClips.Keys.First();
                            var oldClip = cachedClips[oldestKey];
                            if (oldClip != null)
                            {
                                Destroy(oldClip);
                            }
                            cachedClips.Remove(oldestKey);
                            LoggingSystem.Debug($"Cache full - removed old clip: {oldestKey}", "LocalFolder");
                        }

                        // Add to cache and track list
                        cachedClips[fileName] = loadedClip;
                        tracks.Add(loadedClip);
                        trackInfo.Add((FormatTrackTitle(fileName), "Local File"));
                        LoggingSystem.Debug($"Successfully loaded and cached: {fileName} ({loadedClip.length:F1}s)", "LocalFolder");
                    }
                    catch (Exception ex)
                    {
                        LoggingSystem.Error($"Error processing {fileName}: {ex.Message}", "LocalFolder");
                        
                        // Cleanup on error
                        if (loadedClip != null)
                        {
                            Destroy(loadedClip);
                        }
                        if (cachedClips.ContainsKey(fileName))
                        {
                            cachedClips.Remove(fileName);
                        }
                    }
                }
            }

            LoggingSystem.Info($"Successfully loaded {tracks.Count} local music tracks", "LocalFolder");
            isLoading = false;
            onComplete?.Invoke(tracks, trackInfo);
        }

        private string FormatTrackTitle(string fileName)
        {
            return fileName
                .Replace("_", " ")
                .Replace("-", " - ")
                .Trim();
        }

        private string GetMusicFolderPath()
        {
            string gameDirectory = Directory.GetCurrentDirectory();
            return Path.Combine(gameDirectory, "Mods", "BackSpeaker", "Music");
        }

        private void EnsureMusicFolderExists()
        {
            string musicPath = GetMusicFolderPath();
            
            if (!Directory.Exists(musicPath))
            {
                Directory.CreateDirectory(musicPath);
                
                // Create README file with instructions
                string readmePath = Path.Combine(musicPath, "README.txt");
                string readmeContent = @"BackSpeaker Local Music Folder
=============================

This folder is for your personal music collection.

Supported formats (via NAudio):
- MP3 (recommended)
- WAV
- AIFF/AIF
- WMA
- M4A

Instructions:
1. Copy your music files to this folder
2. Go back to BackSpeaker and click 'Refresh Tracks'
3. Your music will appear in the playlist

Tips:
- Use descriptive filenames for better organization
- Avoid special characters in filenames
- Consider using MP3 format for best compatibility
- Keep file sizes reasonable for better performance
- Clean naming format: 'Artist - Title.mp3'";

                File.WriteAllText(readmePath, readmeContent);
            }
        }

        public void OpenMusicFolder()
        {
            string musicPath = GetMusicFolderPath();
            EnsureMusicFolderExists();
            global::System.Diagnostics.Process.Start(musicPath);
        }

        public void RefreshTracks(Action<List<AudioClip>, List<(string title, string artist)>> onComplete)
        {
            LoadTracks(onComplete);
        }

        public Dictionary<string, object> GetConfiguration()
        {
            configuration["MusicFolderPath"] = GetMusicFolderPath();
            configuration["IsAvailable"] = IsAvailable;
            configuration["SupportedFormats"] = AudioHelper.GetSupportedExtensions();
            configuration["MaxCacheSize"] = maxCacheSize;
            
            try
            {
                string musicPath = GetMusicFolderPath();
                if (Directory.Exists(musicPath))
                {
                    var supportedExtensions = AudioHelper.GetSupportedExtensions();
                    int fileCount = 0;
                    foreach (string ext in supportedExtensions)
                    {
                        fileCount += Directory.GetFiles(musicPath, "*" + ext, SearchOption.TopDirectoryOnly).Length;
                    }
                    configuration["FileCount"] = fileCount;
                }
                else
                {
                    configuration["FileCount"] = 0;
                }
            }
            catch
            {
                configuration["FileCount"] = "Unknown";
            }
            
            return new Dictionary<string, object>(configuration);
        }

        public void ApplyConfiguration(Dictionary<string, object> config)
        {
            // Configuration could be applied here in the future
            // For example: maxCacheSize, supported formats, etc.
        }

        public void Cleanup()
        {
            // Mark as destroyed so coroutines can stop
            isDestroyed = true;

            // Clear cached clips to free memory
            foreach (var clip in cachedClips.Values)
            {
                if (clip != null)
                {
                    Destroy(clip);
                }
            }
            cachedClips.Clear();
            LoggingSystem.Info("Cleaned up cached audio clips", "LocalFolder");
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}