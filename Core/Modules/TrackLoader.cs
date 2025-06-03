using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using Il2CppScheduleOne.Audio;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using System;
using System.Globalization;
using Il2CppInterop.Runtime;

namespace BackSpeakerMod.Core.Modules
{
    public class TrackLoader : MonoBehaviour
    {
        public Action<List<AudioClip>, List<(string title, string artist)>>? OnTracksLoaded;
        public event EventHandler<MusicSourceChangedEventArgs>? OnMusicSourceChanged;

        // Music source providers
        private readonly Dictionary<MusicSourceType, IMusicSourceProvider> musicProviders;
        private MusicSourceType currentSourceType = MusicSourceType.Jukebox;
        private bool isLoading = false;

        // Jukebox provider (original functionality)
        private JukeboxMusicProvider? jukeboxProvider;

        // public TrackLoader() : base() { }

        public TrackLoader()
        {
            musicProviders = new Dictionary<MusicSourceType, IMusicSourceProvider>();
            InitializeMusicProviders();
        }

        private void InitializeMusicProviders()
        {
            try
            {
                // Initialize jukebox provider (keep original functionality)
                jukeboxProvider = new JukeboxMusicProvider();
                musicProviders[MusicSourceType.Jukebox] = jukeboxProvider;

                LoggingSystem.Info("TrackLoader: Music providers initialized", "Audio");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to initialize music providers: {ex.Message}", "Audio");
            }
        }

        public void InitializeExternalProviders(GameObject parentObject)
        {
            try
            {
                // Initialize local folder provider
                var localProvider = parentObject.AddComponent<LocalFolderMusicProvider>();
                musicProviders[MusicSourceType.LocalFolder] = localProvider;

                // Initialize YouTube provider
                var youtubeProvider = parentObject.AddComponent<YouTubeMusicProvider>();
                musicProviders[MusicSourceType.YouTube] = youtubeProvider;

                LoggingSystem.Info("TrackLoader: External music providers initialized", "Audio");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to initialize external music providers: {ex.Message}", "Audio");
            }
        }

        /// <summary>
        /// Switch to a different music source
        /// </summary>
        public void SetMusicSource(MusicSourceType sourceType)
        {
            if (currentSourceType == sourceType || isLoading)
            {
                return;
            }

            if (!musicProviders.ContainsKey(sourceType))
            {
                LoggingSystem.Warning($"Music provider not available: {sourceType}", "Audio");
                return;
            }

            var provider = musicProviders[sourceType];
            if (!provider.IsAvailable)
            {
                LoggingSystem.Warning($"Music provider not available: {provider.DisplayName}", "Audio");
                OnMusicSourceChanged?.Invoke(this, new MusicSourceChangedEventArgs(
                    currentSourceType, sourceType, false, $"{provider.DisplayName} is not available"));
                return;
            }

            var previousSource = currentSourceType;
            currentSourceType = sourceType;

            OnMusicSourceChanged?.Invoke(this, new MusicSourceChangedEventArgs(
                previousSource, currentSourceType, false, $"Switching to {provider.DisplayName}..."));

            LoadTracksFromCurrentSource();
        }

        /// <summary>
        /// Get current music source type
        /// </summary>
        public MusicSourceType GetCurrentSourceType()
        {
            return currentSourceType;
        }

        /// <summary>
        /// Get available music sources
        /// </summary>
        public List<(MusicSourceType type, string name, bool available)> GetAvailableSources()
        {
            var sources = new List<(MusicSourceType, string, bool)>();
            
            foreach (var kvp in musicProviders)
            {
                sources.Add((kvp.Key, kvp.Value.DisplayName, kvp.Value.IsAvailable));
            }

            return sources;
        }

        /// <summary>
        /// Load tracks from current music source
        /// </summary>
        public void LoadTracksFromCurrentSource()
        {
            if (isLoading)
            {
                LoggingSystem.Warning("Track loading already in progress", "Audio");
                return;
            }

            if (!musicProviders.ContainsKey(currentSourceType))
            {
                LoggingSystem.Error($"No provider available for source type: {currentSourceType}", "Audio");
                return;
            }

            isLoading = true;
            var provider = musicProviders[currentSourceType];
            
            LoggingSystem.Info($"Loading tracks from: {provider.DisplayName}", "Audio");
            
            provider.LoadTracks((tracks, trackInfo) =>
            {
                isLoading = false;
                OnTracksLoaded?.Invoke(tracks, trackInfo);
                
                OnMusicSourceChanged?.Invoke(this, new MusicSourceChangedEventArgs(
                    currentSourceType, currentSourceType, true, $"Loaded {tracks.Count} tracks from {provider.DisplayName}"));
            });
        }

        /// <summary>
        /// Get YouTube provider for direct URL downloads
        /// </summary>
        public YouTubeMusicProvider? GetYouTubeProvider()
        {
            if (musicProviders.ContainsKey(MusicSourceType.YouTube))
            {
                return musicProviders[MusicSourceType.YouTube] as YouTubeMusicProvider;
            }
            return null;
        }

        /// <summary>
        /// Get local folder provider for folder operations
        /// </summary>
        public LocalFolderMusicProvider? GetLocalFolderProvider()
        {
            if (musicProviders.ContainsKey(MusicSourceType.LocalFolder))
            {
                return musicProviders[MusicSourceType.LocalFolder] as LocalFolderMusicProvider;
            }
            return null;
        }

        /// <summary>
        /// Legacy method - kept for backward compatibility
        /// </summary>
        public void LoadJukeboxTracks()
        {
            SetMusicSource(MusicSourceType.Jukebox);
        }

        private void OnDestroy()
        {
            // Cleanup all providers
            foreach (var provider in musicProviders.Values)
            {
                provider.Cleanup();
            }
        }
    }

    /// <summary>
    /// Jukebox music provider (original functionality extracted)
    /// </summary>
    public class JukeboxMusicProvider : IMusicSourceProvider
    {
        public MusicSourceType SourceType => MusicSourceType.Jukebox;
        public string DisplayName => "In-Game Jukebox";
        public bool IsAvailable => true; // Always available for testing

        public void LoadTracks(Action<List<AudioClip>, List<(string title, string artist)>> onComplete)
        {
            var tracks = new List<AudioClip>();
            var trackInfo = new List<(string title, string artist)>();
            
            LoggingSystem.Info("🎵 Starting song detection - PRIORITIZING JUKEBOX MUSIC...", "Audio");
            
            // PRIORITY 1: Search for AmbientLoopJukebox objects - THE REAL MUSIC! 🎵
            bool foundJukeboxMusic = TryLoadFromJukeboxes(tracks, trackInfo);
            
            // Only fallback to game audio if no jukebox music found
            if (!foundJukeboxMusic)
            {
                LoggingSystem.Info("⚠️ No jukebox music found, falling back to game audio sources...", "Audio");
                TryLoadFromGameAudio(tracks, trackInfo);
            }
            
            LoggingSystem.Info($"🎵 Final result: Loaded {tracks.Count} music tracks total.", "Audio");
            LogTrackSummary(tracks, trackInfo);
            
            onComplete?.Invoke(tracks, trackInfo);
        }

        private bool TryLoadFromJukeboxes(List<AudioClip> tracks, List<(string title, string artist)> trackInfo)
        {
            try
            {
                var jukeboxes = GameObject.FindObjectsOfType<AmbientLoopJukebox>();
                
                if (jukeboxes.Length == 0)
                {
                    return false;
                }
                
                var seen = new HashSet<AudioClip>();
                int addedCount = 0;
                
                foreach (var jukebox in jukeboxes)
                {
                    var clips = jukebox.Clips;
                    if (clips != null && clips.Count > 0)
                    {
                        foreach (var clip in clips)
                        {
                            if (clip != null && seen.Add(clip))
                            {
                                tracks.Add(clip);
                                string trackName = FormatTrackName(clip.name);
                                trackInfo.Add((trackName, "Jukebox Music"));
                                addedCount++;
                            }
                        }
                    }
                }
                
                return addedCount > 0;
            }
            catch (Exception e)
            {
                LoggingSystem.Error($"❌ Error loading from jukeboxes: {e.Message}", "Audio");
                return false;
            }
        }
        
        private bool TryLoadFromGameAudio(List<AudioClip> tracks, List<(string title, string artist)> trackInfo)
        {
            try
            {
                var musicPlayer = Il2CppScheduleOne.Audio.MusicPlayer.instance;
                if (musicPlayer != null && musicPlayer.Tracks != null)
                {
                    var seen = new HashSet<AudioClip>();
                    int addedCount = 0;
                    
                    foreach (var musicTrack in musicPlayer.Tracks)
                    {
                        if (musicTrack?.Controller?.AudioSource?.clip != null)
                        {
                            var clip = musicTrack.Controller.AudioSource.clip;
                            if (seen.Add(clip))
                            {
                                tracks.Add(clip);
                                string trackName = !string.IsNullOrEmpty(musicTrack.TrackName) ? musicTrack.TrackName : clip.name;
                                trackInfo.Add((trackName, "Game Audio"));
                                addedCount++;
                            }
                        }
                    }
                    
                    return addedCount > 0;
                }
            }
            catch (Exception e)
            {
                LoggingSystem.Warning($"❌ Failed to load from MusicPlayer: {e.Message}", "Audio");
            }
            return false;
        }
        
        private string FormatTrackName(string clipName)
        {
            if (string.IsNullOrEmpty(clipName))
                return "Unknown Track";
                
            // Keep a copy of the original name as fallback
            string original = clipName.Trim();
            if (string.IsNullOrEmpty(original))
                return "Unknown Track";
                
            try
            {
                string formatted = original;
                
                // Remove file extensions
                if (formatted.Contains("."))
                {
                    int lastDot = formatted.LastIndexOf('.');
                    if (lastDot > 0) // Don't remove if dot is at the beginning
                        formatted = formatted.Substring(0, lastDot);
                }
                
                // Remove common prefixes (but be more careful)
                if (formatted.Length > 6 && formatted.StartsWith("audio_", StringComparison.OrdinalIgnoreCase))
                    formatted = formatted.Substring(6);
                else if (formatted.Length > 6 && formatted.StartsWith("music_", StringComparison.OrdinalIgnoreCase))
                    formatted = formatted.Substring(6);
                
                // Replace underscores with spaces
                formatted = formatted.Replace('_', ' ');
                
                // Remove extra whitespace
                formatted = Regex.Replace(formatted, @"\s+", " ").Trim();
                
                // Capitalize properly, but handle special cases
                if (!string.IsNullOrEmpty(formatted))
                {
                    try
                    {
                        formatted = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(formatted.ToLower());
                    }
                    catch
                    {
                        // If capitalization fails, just clean up the original
                        formatted = original.Replace('_', ' ').Trim();
                    }
                }
                
                // Final validation - if we ended up with empty string, use original
                if (string.IsNullOrEmpty(formatted) || formatted.Trim().Length == 0)
                    formatted = original;
                
                return formatted;
            }
            catch (Exception ex)
            {
                LoggingSystem.Warning($"Error formatting track name '{original}': {ex.Message}", "Audio");
                // Return the original name if formatting fails
                return original;
            }
        }
        
        private void LogTrackSummary(List<AudioClip> tracks, List<(string title, string artist)> trackInfo)
        {
            if (tracks.Count == 0)
            {
                LoggingSystem.Warning("❌ No tracks found!", "Audio");
                return;
            }
            
            LoggingSystem.Info("🎵 === TRACK SUMMARY ===", "Audio");
            for (int i = 0; i < Math.Min(tracks.Count, 10); i++)
            {
                var track = tracks[i];
                var info = trackInfo[i];
                LoggingSystem.Info($"   {i + 1}. '{info.title}' by {info.artist} ({track.length:F1}s)", "Audio");
            }
            
            if (tracks.Count > 10)
            {
                LoggingSystem.Info($"   ... and {tracks.Count - 10} more tracks", "Audio");
            }
        }

        public Dictionary<string, object> GetConfiguration()
        {
            return new Dictionary<string, object>
            {
                {"Description", "Loads music from in-game jukebox objects"},
                {"Priority", "Highest"},
                {"Fallback", "Game audio sources"}
            };
        }

        public void ApplyConfiguration(Dictionary<string, object> config)
        {
            // No configuration options for jukebox provider
        }

        public void Cleanup()
        {
            // Nothing to clean up for jukebox provider
        }
    }
} 