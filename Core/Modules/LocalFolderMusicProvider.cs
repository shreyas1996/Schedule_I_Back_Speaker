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

            // Load files sequentially to avoid memory issues
            foreach (var filePath in allFiles)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                LoggingSystem.Debug($"Loading: {fileName}", "LocalFolder");

                // Check if we have a cached clip
                if (cachedClips.TryGetValue(fileName, out AudioClip? cachedClip) && cachedClip != null)
                {
                    LoggingSystem.Debug($"Using cached clip for: {fileName}", "LocalFolder");
                    tracks.Add(cachedClip);
                    trackInfo.Add((FormatTrackTitle(fileName), "Local File"));
                    continue;
                }

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

                if (loadedClip != null)
                {
                    try
                    {
                        // Cache the clip
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

                // Add a small delay between files to prevent audio system overload
                yield return new WaitForSeconds(0.1f);
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
- MP3 format provides best compatibility and file size
- NAudio handles most common audio formats reliably

Enjoy your music!
";
                
                try
                {
                    File.WriteAllText(readmePath, readmeContent);
                    LoggingSystem.Info("Created music folder with README", "LocalFolder");
                }
                catch (Exception ex)
                {
                    LoggingSystem.Warning($"Could not create README file: {ex.Message}", "LocalFolder");
                }
            }
        }

        public void OpenMusicFolder()
        {
            string musicPath = GetMusicFolderPath();
            EnsureMusicFolderExists();
            
            try
            {
                // Cross-platform folder opening
                #if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                    System.Diagnostics.Process.Start("explorer.exe", musicPath.Replace('/', '\\'));
                #elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                    System.Diagnostics.Process.Start("open", musicPath);
                #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
                    System.Diagnostics.Process.Start("xdg-open", musicPath);
                #endif
                
                LoggingSystem.Info($"Opened music folder: {musicPath}", "LocalFolder");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Could not open music folder: {ex.Message}", "LocalFolder");
            }
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
        }

        public void Cleanup()
        {
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