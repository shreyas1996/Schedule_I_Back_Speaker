using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;
using BackSpeakerMod.Core.System;
using UnityEngine.Networking;
using MelonLoader;

namespace BackSpeakerMod.Core.Modules
{
    /// <summary>
    /// Provides music from local folder files
    /// </summary>
    public class LocalFolderMusicProvider : MonoBehaviour, IMusicSourceProvider
    {
        public MusicSourceType SourceType => MusicSourceType.LocalFolder;
        public string DisplayName => "Local Music Folder";
        public bool IsAvailable => Directory.Exists(GetMusicFolderPath());

        private Dictionary<string, object> configuration = new Dictionary<string, object>();

        public LocalFolderMusicProvider() : base() { }

        public void LoadTracks(Action<List<AudioClip>, List<(string title, string artist)>> onComplete)
        {
            var coroutine = LoadLocalTracks(onComplete);
            MelonCoroutines.Start(coroutine);
        }

        private IEnumerator LoadLocalTracks(Action<List<AudioClip>, List<(string title, string artist)>> onComplete)
        {
            var tracks = new List<AudioClip>();
            var trackInfo = new List<(string title, string artist)>();

            string musicPath = GetMusicFolderPath();
            
            EnsureMusicFolderExists();

            if (Directory.Exists(musicPath))
            {
                // Supported audio file extensions
                string[] extensions = { "*.mp3", "*.wav", "*.ogg", "*.m4a", "*.aac" };
                
                foreach (string extension in extensions)
                {
                    var files = Directory.GetFiles(musicPath, extension, SearchOption.TopDirectoryOnly);
                    
                    foreach (string filePath in files)
                    {
                        yield return LoadAudioFile(filePath, tracks, trackInfo);
                    }
                }
            }

            LoggingSystem.Info($"Loaded {tracks.Count} local music tracks", "LocalFolder");
            onComplete?.Invoke(tracks, trackInfo);
        }

        private IEnumerator LoadAudioFile(string filePath, List<AudioClip> tracks, List<(string title, string artist)> trackInfo)
        {
            AudioType audioType = GetAudioType(filePath);
            
            if (audioType == AudioType.UNKNOWN)
            {
                LoggingSystem.Warning($"Unsupported audio format: {filePath}", "LocalFolder");
                yield break;
            }

            string uri = "file://" + filePath.Replace("\\", "/");
            
            var request = UnityWebRequestMultimedia.GetAudioClip(uri, audioType);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                if (clip != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    clip.name = fileName;
                    
                    tracks.Add(clip);
                    trackInfo.Add((FormatTrackTitle(fileName), "Local File"));
                    
                    LoggingSystem.Debug($"Loaded audio file: {fileName}", "LocalFolder");
                }
            }
            else
            {
                LoggingSystem.Warning($"Failed to load audio file: {filePath} - {request.error}", "LocalFolder");
            }
            
            request.Dispose();
        }

        private AudioType GetAudioType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            
            return extension switch
            {
                ".mp3" => AudioType.MPEG,
                ".wav" => AudioType.WAV,
                ".ogg" => AudioType.OGGVORBIS,
                ".m4a" => AudioType.MPEG,
                ".aac" => AudioType.MPEG,
                _ => AudioType.UNKNOWN
            };
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

Supported formats:
- MP3 (recommended)
- WAV
- OGG
- M4A
- AAC

Instructions:
1. Copy your music files to this folder
2. Go back to BackSpeaker and click 'Refresh Tracks'
3. Your music will appear in the playlist

Tips:
- Use descriptive filenames for better organization
- MP3 format provides best compatibility
- Avoid very large files that may cause memory issues

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
            configuration["SupportedFormats"] = new[] { "MP3", "WAV", "OGG", "M4A", "AAC" };
            
            try
            {
                string musicPath = GetMusicFolderPath();
                if (Directory.Exists(musicPath))
                {
                    var files = Directory.GetFiles(musicPath, "*.*", SearchOption.TopDirectoryOnly);
                    configuration["FileCount"] = files.Length;
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