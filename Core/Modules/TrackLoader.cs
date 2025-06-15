using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using Il2CppScheduleOne.Audio;
using Il2CppScheduleOne.ObjectScripts;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using System;
using System.Globalization;
using Il2CppInterop.Runtime;
using System.Linq;

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

        public void InitializeExternalProviders(GameObject? parentObject)
        {
            if (parentObject == null)
            {
                LoggingSystem.Error("Parent object is null. Cannot initialize external providers.", "Audio");
                return;
            }

            try
            {
                LoggingSystem.Info($"Initializing external providers on GameObject: {parentObject.name}", "Audio");
                
                // Initialize local folder provider
                var localProvider = parentObject.AddComponent<LocalFolderMusicProvider>();
                musicProviders[MusicSourceType.LocalFolder] = localProvider;
                LoggingSystem.Info($"Added LocalFolderMusicProvider - Available: {localProvider.IsAvailable}", "Audio");

                // Initialize YouTube provider
                var youtubeProvider = parentObject.AddComponent<YouTubeMusicProvider>();
                musicProviders[MusicSourceType.YouTube] = youtubeProvider;
                LoggingSystem.Info($"Added YouTubeMusicProvider - Available: {youtubeProvider.IsAvailable}", "Audio");

                LoggingSystem.Info($"TrackLoader: External music providers initialized. Total providers: {musicProviders.Count}", "Audio");
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
            LoggingSystem.Info($"SetMusicSource called: {sourceType} (current: {currentSourceType}, loading: {isLoading})", "Audio");
            
            if (currentSourceType == sourceType || isLoading)
            {
                return;
            }

            if (!musicProviders.ContainsKey(sourceType))
            {
                LoggingSystem.Warning($"Music provider not available: {sourceType}. Available providers: [{string.Join(", ", musicProviders.Keys)}]", "Audio");
                return;
            }

            var provider = musicProviders[sourceType];
            
            // Always switch to the requested source, even if initially unavailable
            // Some providers (like LocalFolder) need to be activated to become available
            var previousSource = currentSourceType;
            currentSourceType = sourceType;

            if (!provider.IsAvailable)
            {
                LoggingSystem.Info($"Switching to {provider.DisplayName} (will attempt initialization)...", "Audio");
            }
            else
            {
                LoggingSystem.Info($"Switching to {provider.DisplayName}...", "Audio");
            }

            OnMusicSourceChanged?.Invoke(this, new MusicSourceChangedEventArgs(
                previousSource, currentSourceType, false, $"Switching to {provider.DisplayName}..."));

            LoadTracksFromCurrentSource();
        }

        /// <summary>
        /// Force load tracks from a specific source, regardless of availability status
        /// Useful for initializing providers that need activation
        /// </summary>
        public void ForceLoadFromSource(MusicSourceType sourceType)
        {
            LoggingSystem.Info($"ForceLoadFromSource called: {sourceType}", "Audio");
            
            if (!musicProviders.ContainsKey(sourceType))
            {
                LoggingSystem.Warning($"Cannot force load - provider not found: {sourceType}", "Audio");
                return;
            }

            if (isLoading)
            {
                LoggingSystem.Warning("Track loading already in progress", "Audio");
                return;
            }

            var previousSource = currentSourceType;
            currentSourceType = sourceType;
            
            var provider = musicProviders[sourceType];
            LoggingSystem.Info($"Force loading tracks from: {provider.DisplayName}", "Audio");
            
            isLoading = true;
            provider.LoadTracks((tracks, trackInfo) =>
            {
                isLoading = false;
                OnTracksLoaded?.Invoke(tracks, trackInfo);
                
                OnMusicSourceChanged?.Invoke(this, new MusicSourceChangedEventArgs(
                    previousSource, currentSourceType, true, $"Loaded {tracks.Count} tracks from {provider.DisplayName}"));
            });
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
                // PRIORITY 1: Try to load from main player-owned Jukebox (Il2CppScheduleOne.ObjectScripts)
                bool foundMainJukebox = TryLoadFromMainJukebox(tracks, trackInfo);
                
                // PRIORITY 2: Try to load from AmbientLoopJukebox objects (existing functionality)
                bool foundAmbientJukebox = TryLoadFromAmbientJukeboxes(tracks, trackInfo);
                
                return foundMainJukebox || foundAmbientJukebox;
            }
            catch (Exception e)
            {
                LoggingSystem.Error($"❌ Error loading from jukeboxes: {e.Message}", "Audio");
                return false;
            }
        }
        
        /// <summary>
        /// Try to load tracks from the main player-owned Jukebox (Il2CppScheduleOne.ObjectScripts)
        /// This is where other mods can add tracks that we want to access
        /// </summary>
        private bool TryLoadFromMainJukebox(List<AudioClip> tracks, List<(string title, string artist)> trackInfo)
        {
            try
            {
                LoggingSystem.Info("🎵 Searching for main player-owned Jukebox...", "Audio");
                
                // Method 1: Try to find Jukebox objects by type
                var mainJukeboxes = GameObject.FindObjectsOfType<Il2CppScheduleOne.ObjectScripts.Jukebox>();
                
                if (mainJukeboxes != null && mainJukeboxes.Length > 0)
                {
                    LoggingSystem.Info($"🎵 Found {mainJukeboxes.Length} main Jukebox(es)!", "Audio");
                    
                    var seen = new HashSet<AudioClip>();
                    int addedCount = 0;
                    
                    foreach (var jukebox in mainJukeboxes)
                    {
                        if (jukebox != null)
                        {
                            LoggingSystem.Debug($"Processing main Jukebox: {jukebox.name}", "Audio");
                            
                            // Try to access tracks from the main jukebox
                            // The exact property/method name may vary - we'll try common patterns
                            var jukeboxTracks = GetTracksFromMainJukebox(jukebox);
                            
                            if (jukeboxTracks != null && jukeboxTracks.Count > 0)
                            {
                                foreach (var clip in jukeboxTracks)
                                {
                                    if (clip != null && seen.Add(clip))
                                    {
                                        tracks.Add(clip);
                                        
                                        // Get the proper metadata we stored earlier
                                        var metadata = GetTrackMetadata(clip);
                                        trackInfo.Add((metadata.title, metadata.artist));
                                        addedCount++;
                                        LoggingSystem.Debug($"Added track from main jukebox: {metadata.title} by {metadata.artist}", "Audio");
                                    }
                                }
                            }
                        }
                    }
                    
                    if (addedCount > 0)
                    {
                        LoggingSystem.Info($"🎵 Successfully loaded {addedCount} tracks from main Jukebox!", "Audio");
                        return true;
                    }
                }
                else
                {
                    LoggingSystem.Debug("No main Jukebox objects found in scene", "Audio");
                }
                
                LoggingSystem.Warning("❌ Main jukebox found but no tracks extracted", "Audio");
                return false;
            }
            catch (Exception e)
            {
                LoggingSystem.Warning($"❌ Error accessing main Jukebox: {e.Message}", "Audio");
                return false;
            }
        }
        
        /// <summary>
        /// Get tracks from a main Jukebox object - uses the actual TrackList property
        /// </summary>
        private List<AudioClip> GetTracksFromMainJukebox(Il2CppScheduleOne.ObjectScripts.Jukebox jukebox)
        {
            try
            {
                LoggingSystem.Info($"🎵 ACCESSING MAIN JUKEBOX: {jukebox.name}", "Audio Main Jukebox");
                
                // Access the TrackList property directly
                var trackList = jukebox.TrackList;
                
                if (trackList == null)
                {
                    LoggingSystem.Warning("❌ TrackList is null - jukebox has no tracks", "Audio Main Jukebox");
                    return new List<AudioClip>();
                }
                
                LoggingSystem.Info($"🎵 Found TrackList with {trackList.Count} tracks", "Audio Main Jukebox");
                
                var clips = new List<AudioClip>();
                
                // Extract each track with proper metadata
                for (int i = 0; i < trackList.Count; i++)
                {
                    var track = trackList[i];
                    if (track != null && track.Clip != null)
                    {
                        // Use the proper TrackName and ArtistName from the Track object
                        string trackName = !string.IsNullOrEmpty(track.TrackName) ? track.TrackName : track.Clip.name;
                        string artistName = !string.IsNullOrEmpty(track.ArtistName) ? track.ArtistName : "Main Jukebox";
                        
                        LoggingSystem.Debug($"  ✅ Track {i + 1}: '{trackName}' by '{artistName}' (Duration: {track.Clip.length:F1}s)", "Audio Main Jukebox");
                        
                        clips.Add(track.Clip);
                        
                        // Store the proper metadata for track info display
                        // We need to store this mapping for later use in track info
                        StoreTrackMetadata(track.Clip, trackName, artistName);
                    }
                    else
                    {
                        LoggingSystem.Warning($"  ❌ Track {i + 1}: Invalid track or missing clip", "Audio Main Jukebox");
                    }
                }
                
                LoggingSystem.Debug($"🎉 SUCCESSFULLY EXTRACTED {clips.Count} AUDIO CLIPS FROM MAIN JUKEBOX!", "Audio Main Jukebox");
                return clips;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"❌ Error accessing main jukebox tracks: {ex.Message}", "Audio Main Jukebox");
                return new List<AudioClip>();
            }
        }
        
        // Dictionary to store track metadata mapping
        private static Dictionary<AudioClip, (string title, string artist)> trackMetadataMap = new Dictionary<AudioClip, (string, string)>();
        
        /// <summary>
        /// Store track metadata for later retrieval
        /// </summary>
        private void StoreTrackMetadata(AudioClip clip, string title, string artist)
        {
            if (clip != null)
            {
                trackMetadataMap[clip] = (title, artist);
            }
        }
        
        /// <summary>
        /// Get stored track metadata, fallback to clip name if not found
        /// </summary>
        public static (string title, string artist) GetTrackMetadata(AudioClip clip)
        {
            if (clip != null && trackMetadataMap.ContainsKey(clip))
            {
                return trackMetadataMap[clip];
            }
            
            // Fallback to clip name
            return (clip?.name ?? "Unknown Track", "Unknown Artist");
        }
        
        /// <summary>
        /// Load from AmbientLoopJukebox objects (existing functionality, now separated)
        /// </summary>
        private bool TryLoadFromAmbientJukeboxes(List<AudioClip> tracks, List<(string title, string artist)> trackInfo)
        {
            try
            {
                LoggingSystem.Debug("🎵 Searching for AmbientLoopJukebox objects...", "Audio");
                
                var jukeboxes = GameObject.FindObjectsOfType<AmbientLoopJukebox>();
                
                if (jukeboxes.Length == 0)
                {
                    LoggingSystem.Debug("No AmbientLoopJukebox objects found", "Audio");
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
                                trackInfo.Add((trackName, "Ambient Jukebox"));
                                addedCount++;
                            }
                        }
                    }
                }
                
                if (addedCount > 0)
                {
                    LoggingSystem.Info($"🎵 Found {addedCount} tracks from AmbientLoopJukebox objects!", "Audio");
                    return true;
                }
                
                return false;
            }
            catch (Exception e)
            {
                LoggingSystem.Error($"❌ Error loading from AmbientLoopJukebox: {e.Message}", "Audio");
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