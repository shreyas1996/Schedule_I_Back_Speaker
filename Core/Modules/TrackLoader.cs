using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Text.RegularExpressions;
using Il2CppScheduleOne.Audio;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using System;
using System.Globalization;

namespace BackSpeakerMod.Core.Modules
{
    public class TrackLoader
    {
        public Action<List<AudioClip>, List<(string title, string artist)>>? OnTracksLoaded;

        public void LoadJukeboxTracks()
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
            
            OnTracksLoaded?.Invoke(tracks, trackInfo);
        }
        
        private bool TryLoadFromJukeboxes(List<AudioClip> tracks, List<(string title, string artist)> trackInfo)
        {
            try
            {
                // LoggerUtil.Info("🎵 PRIORITY METHOD: Searching for jukebox music...");
                var jukeboxes = GameObject.FindObjectsOfType<AmbientLoopJukebox>();
                // LoggerUtil.Info($"Found {jukeboxes.Length} jukebox objects in the scene");
                
                if (jukeboxes.Length == 0)
                {
                    // LoggerUtil.Warn("❌ No AmbientLoopJukebox objects found in scene!");
                    return false;
                }
                
                var seen = new HashSet<AudioClip>();
                int addedCount = 0;
                
                foreach (var jukebox in jukeboxes)
                {
                    // LoggerUtil.Info($"   🎵 Checking jukebox: '{jukebox.name}' at position {jukebox.transform.position}");
                    
                    var clips = jukebox.Clips;
                    if (clips != null && clips.Count > 0)
                    {
                        // LoggerUtil.Info($"      ✅ This jukebox has {clips.Count} clips!");
                        
                        foreach (var clip in clips)
                        {
                            if (clip != null && seen.Add(clip))
                            {
                                tracks.Add(clip);
                                // Format track name properly
                                string trackName = FormatTrackName(clip.name);
                                trackInfo.Add((trackName, "Jukebox Music"));
                                addedCount++;
                                // LoggerUtil.Info($"      ♪ Added: '{trackName}' ({clip.length:F1}s)");
                            }
                        }
                    }
                    else
                    {
                        // LoggerUtil.Warn($"      ❌ Jukebox '{jukebox.name}' has no clips or clips is null");
                    }
                }
                
                if (addedCount > 0)
                {
                    // LoggerUtil.Info($"✅ SUCCESS: Loaded {addedCount} jukebox tracks from {jukeboxes.Length} jukeboxes!");
                    return true;
                }
                else
                {
                    // LoggerUtil.Warn($"❌ Found {jukeboxes.Length} jukeboxes but no valid music clips in any of them");
                    return false;
                }
            }
            catch (Exception e)
            {
                LoggingSystem.Error($"❌ Error loading from jukeboxes: {e.Message}", "Audio");
                LoggingSystem.Error($"Stack trace: {e.StackTrace}", "Audio");
                return false;
            }
        }
        
        private bool TryLoadFromGameAudio(List<AudioClip> tracks, List<(string title, string artist)> trackInfo)
        {
            try
            {
                // LoggerUtil.Info("🎵 FALLBACK: Searching game's MusicPlayer system...");
                var musicPlayer = Il2CppScheduleOne.Audio.MusicPlayer.instance;
                if (musicPlayer != null && musicPlayer.Tracks != null)
                {
                    // LoggerUtil.Info($"Found MusicPlayer with {musicPlayer.Tracks.Count} tracks");
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
                                // LoggerUtil.Info($"   ♪ Added game audio: '{trackName}' ({clip.length:F1}s)");
                            }
                        }
                    }
                    
                    if (addedCount > 0)
                    {
                        // LoggerUtil.Info($"✅ Loaded {addedCount} tracks from game audio system");
                        return true;
                    }
                }
                else
                {
                    // LoggerUtil.Info("⚠️ MusicPlayer.instance is null or has no tracks");
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
            LoggingSystem.Info("🎵 === TRACK SUMMARY ===", "Audio");
            if (tracks.Count == 0)
            {
                LoggingSystem.Warning("❌ No tracks loaded!", "Audio");
                return;
            }
            
            var artists = new HashSet<string>();
            float totalDuration = 0f;
            
            for (int i = 0; i < tracks.Count && i < trackInfo.Count; i++)
            {
                var track = tracks[i];
                var info = trackInfo[i];
                totalDuration += track.length;
                artists.Add(info.artist);
                LoggingSystem.Debug($"   {i + 1:00}. '{info.title}' by {info.artist} ({track.length:F1}s)", "Audio");
            }
            
            LoggingSystem.Info($"🎵 Total: {tracks.Count} tracks, {artists.Count} sources, {totalDuration / 60:F1} minutes", "Audio");
            LoggingSystem.Info("🎵 ==================", "Audio");
        }
    }
} 