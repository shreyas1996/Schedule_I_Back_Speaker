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

        public LocalFolderMusicProvider() : base() { }

        public void LoadTracks(Action<List<AudioClip>, List<(string title, string artist)>> onComplete)
        {
            var coroutine = LoadLocalTracksCoroutine(onComplete);
            MelonCoroutines.Start(coroutine);
        }

        private IEnumerator LoadLocalTracksCoroutine(Action<List<AudioClip>, List<(string title, string artist)>> onComplete)
        {
            // Run the async operation and wait for completion
            Task<(List<AudioClip>, List<(string, string)>)> loadTask = LoadLocalTracksAsync();
            
            while (!loadTask.IsCompleted)
            {
                yield return null; // Wait one frame
            }
            
            if (loadTask.IsFaulted)
            {
                LoggingSystem.Error($"Error loading local tracks: {loadTask.Exception?.Message}", "LocalFolder");
                onComplete?.Invoke(new List<AudioClip>(), new List<(string, string)>());
            }
            else
            {
                var (tracks, trackInfo) = loadTask.Result;
                onComplete?.Invoke(tracks, trackInfo);
            }
        }

        private async Task<(List<AudioClip>, List<(string title, string artist)>)> LoadLocalTracksAsync()
        {
            var tracks = new List<AudioClip>();
            var trackInfo = new List<(string title, string artist)>();

            string musicPath = GetMusicFolderPath();
            
            EnsureMusicFolderExists();
            
            LoggingSystem.Info($"Loading local tracks from: {musicPath}", "LocalFolder");

            if (!Directory.Exists(musicPath))
            {
                LoggingSystem.Warning($"Music directory does not exist: {musicPath}", "LocalFolder");
                return (tracks, trackInfo);
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

            // Load files in parallel for better performance
            var loadTasks = allFiles.Select(async filePath =>
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                LoggingSystem.Debug($"Loading: {fileName}", "LocalFolder");
                
                try
                {
                    var clip = await AudioHelper.LoadAudioFileAsync(filePath);
                    if (clip != null)
                    {
                        return (clip, FormatTrackTitle(fileName), "Local File");
                    }
                    else
                    {
                        LoggingSystem.Warning($"Failed to load: {fileName}", "LocalFolder");
                        return ((AudioClip?)null, "", "");
                    }
                }
                catch (Exception ex)
                {
                    LoggingSystem.Error($"Exception loading {fileName}: {ex.Message}", "LocalFolder");
                    return ((AudioClip?)null, "", "");
                }
            });

            // Wait for all loading tasks to complete
            var results = await Task.WhenAll(loadTasks);

            // Add successful results to our lists
            foreach (var (clip, title, artist) in results)
            {
                if (clip != null && !string.IsNullOrEmpty(title))
                {
                    tracks.Add(clip);
                    trackInfo.Add((title, artist));
                }
            }

            LoggingSystem.Info($"Successfully loaded {tracks.Count} local music tracks", "LocalFolder");
            return (tracks, trackInfo);
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
            // Nothing to clean up for local folder provider
        }
    }
} 