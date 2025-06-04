using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Utils;
using System.Linq;
using UnityEngine.Networking;
using MelonLoader;

namespace BackSpeakerMod.Core.Modules
{
    /// <summary>
    /// Provides music from YouTube (placeholder implementation)
    /// </summary>
    public class YouTubeMusicProvider : MonoBehaviour, IMusicSourceProvider
    {
        public MusicSourceType SourceType => MusicSourceType.YouTube;
        public string DisplayName => "YouTube Music";
        public bool IsAvailable => true; // Simple implementation

        private Dictionary<string, object> configuration = new Dictionary<string, object>();
        private readonly Dictionary<string, AudioClip> cachedClips = new Dictionary<string, AudioClip>();
        
        private string? downloadPath;
        private const int maxCacheSize = 50;

        private void Awake()
        {
            downloadPath = Path.Combine(GetCacheDirectory(), "YouTube");
            InitializeConfiguration();
        }

        public void LoadTracks(Action<List<AudioClip>, List<(string title, string artist)>>? onComplete)
        {
            var tracks = new List<AudioClip>();
            var trackInfo = new List<(string title, string artist)>();
            
            var coroutine = LoadCachedTracks(tracks, trackInfo, onComplete);
            MelonCoroutines.Start(coroutine);
        }

        private IEnumerator LoadCachedTracks(List<AudioClip> tracks, List<(string title, string artist)> trackInfo, Action<List<AudioClip>, List<(string title, string artist)>>? onComplete)
        {
            if (downloadPath != null && Directory.Exists(downloadPath))
            {
                var audioFiles = Directory.GetFiles(downloadPath, "*.mp3", SearchOption.TopDirectoryOnly);
                
                foreach (string filePath in audioFiles)
                {
                    yield return LoadCachedAudioFile(filePath, tracks, trackInfo);
                }
            }
            
            LoggingSystem.Info($"Loaded {tracks.Count} cached YouTube tracks", "YouTube");
            onComplete?.Invoke(tracks, trackInfo);
        }

        private IEnumerator LoadCachedAudioFile(string filePath, List<AudioClip> tracks, List<(string title, string artist)> trackInfo)
        {
            string uri = "file://" + filePath.Replace("\\", "/");
            
            var request = UnityWebRequestMultimedia.GetAudioClip(uri, AudioType.MPEG);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);
                if (clip != null)
                {
                    string fileName = Path.GetFileNameWithoutExtension(filePath);
                    clip.name = fileName;
                    
                    tracks.Add(clip);
                    trackInfo.Add((FormatYouTubeTitle(fileName), "YouTube"));
                    
                    if (!cachedClips.ContainsKey(fileName))
                    {
                        cachedClips[fileName] = clip;
                    }
                }
            }
            
            request.Dispose();
        }

        /// <summary>
        /// Placeholder download method (simplified for now)
        /// </summary>
        public void DownloadFromYouTube(string url, Action<AudioClip, (string title, string artist)>? onComplete)
        {
            LoggingSystem.Info($"YouTube download requested: {url}", "YouTube");
            LoggingSystem.Info("YouTube download feature coming soon! For now, add MP3 files manually to the cache folder.", "YouTube");
            
            // Return placeholder response
            onComplete?.Invoke(null!, ("YouTube feature under development", "Coming Soon"));
        }

        private string FormatYouTubeTitle(string fileName)
        {
            return fileName
                .Replace("_", " ")
                .Replace("-", " - ")
                .Trim();
        }

        private string GetCacheDirectory()
        {
            string gameDirectory = Directory.GetCurrentDirectory();
            return Path.Combine(gameDirectory, "Mods", "BackSpeaker", "Cache");
        }

        private void InitializeConfiguration()
        {
            configuration["Library"] = "Simple Implementation";
            configuration["DownloadPath"] = downloadPath ?? "";
            configuration["MaxCacheSize"] = maxCacheSize;
            configuration["ExternalDependencies"] = "None";
            configuration["Note"] = "YouTube integration simplified for better compatibility";
        }

        public Dictionary<string, object> GetConfiguration()
        {
            return new Dictionary<string, object>(configuration);
        }

        public void ApplyConfiguration(Dictionary<string, object> config)
        {
            // Configuration options could be added here in the future
        }

        public void Cleanup()
        {
            cachedClips.Clear();
        }
    }
} 