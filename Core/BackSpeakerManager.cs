using UnityEngine;
using MelonLoader;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Audio;
using System.Collections.Generic;
using System.Collections;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.Core
{
    public enum RepeatMode
    {
        None,       // No repeat - stop after playlist ends
        RepeatOne,  // Repeat current song
        RepeatAll   // Repeat entire playlist
    }

    public class BackSpeakerManager
    {
        private GameObject speakerObject;
        private AudioSource audioSource;
        private Player currentPlayer;
        private List<AudioClip> tracks = new List<AudioClip>();
        private List<(string title, string artist)> trackInfo = new List<(string, string)>();
        private int currentTrackIndex = 0;
        private bool isPlaying = false;
        // Always auto-advance except in specific cases (RepeatOne, or None at end of playlist)
        private RepeatMode repeatMode = RepeatMode.None;
        
        // Event to notify UI when tracks are reloaded
        public System.Action OnTracksReloaded;

        public BackSpeakerManager()
        {
            LoggerUtil.Info("BackSpeakerManager: Initializing with auto-attach enabled");
            // Subscribe to player spawn event for auto-attachment
            Player.onLocalPlayerSpawned += (Il2CppSystem.Action)(OnLocalPlayerSpawned);
            
            // Try immediate attachment if player is already spawned
            TryInitialAttachment();
        }

        private void LoadJukeboxTracks()
        {
            tracks.Clear();
            trackInfo.Clear();
            
            LoggerUtil.Info("🎵 Starting song detection - PRIORITIZING JUKEBOX MUSIC...");
            
            // PRIORITY 1: Search for AmbientLoopJukebox objects - THE REAL MUSIC! 🎵
            bool foundJukeboxMusic = TryLoadFromJukeboxes();
            
            // Only fallback to game audio if no jukebox music found
            if (!foundJukeboxMusic)
            {
                LoggerUtil.Info("⚠️ No jukebox music found, falling back to game audio sources...");
                TryLoadFromGameAudio();
            }
            
            LoggerUtil.Info($"🎵 Final result: Loaded {tracks.Count} music tracks total.");
            LogTrackSummary();
            
            // Reset to first track if we have tracks and current index is invalid
            if (tracks.Count > 0 && currentTrackIndex >= tracks.Count)
            {
                currentTrackIndex = 0;
                SetTrack(currentTrackIndex);
            }
            
            // Notify UI that tracks have been reloaded
            OnTracksReloaded?.Invoke();
        }
        
        private bool TryLoadFromJukeboxes()
        {
            try
            {
                LoggerUtil.Info("🎵 PRIORITY METHOD: Searching for jukebox music...");
                var jukeboxes = GameObject.FindObjectsOfType<AmbientLoopJukebox>();
                LoggerUtil.Info($"Found {jukeboxes.Length} jukebox objects in the scene");
                
                if (jukeboxes.Length == 0)
                {
                    LoggerUtil.Warn("❌ No AmbientLoopJukebox objects found in scene!");
                    return false;
                }
                
                var seen = new HashSet<AudioClip>();
                int addedCount = 0;
                
                foreach (var jukebox in jukeboxes)
                {
                    LoggerUtil.Info($"   🎵 Checking jukebox: '{jukebox.name}' at position {jukebox.transform.position}");
                    
                    var clips = jukebox.Clips;
                    if (clips != null && clips.Count > 0)
                    {
                        LoggerUtil.Info($"      ✅ This jukebox has {clips.Count} clips!");
                        
                        foreach (var clip in clips)
                        {
                            if (clip != null && seen.Add(clip))
                            {
                                tracks.Add(clip);
                                // Format track name properly
                                string trackName = FormatTrackName(clip.name);
                                trackInfo.Add((trackName, "Jukebox Music"));
                                addedCount++;
                                LoggerUtil.Info($"      ♪ Added: '{trackName}' ({clip.length:F1}s)");
                            }
                        }
                    }
                    else
                    {
                        LoggerUtil.Warn($"      ❌ Jukebox '{jukebox.name}' has no clips or clips is null");
                    }
                }
                
                if (addedCount > 0)
                {
                    LoggerUtil.Info($"✅ SUCCESS: Loaded {addedCount} jukebox tracks from {jukeboxes.Length} jukeboxes!");
                    return true;
                }
                else
                {
                    LoggerUtil.Warn($"❌ Found {jukeboxes.Length} jukeboxes but no valid music clips in any of them");
                    return false;
                }
            }
            catch (System.Exception e)
            {
                LoggerUtil.Error($"❌ Error loading from jukeboxes: {e.Message}");
                LoggerUtil.Error($"Stack trace: {e.StackTrace}");
                return false;
            }
        }
        
        private bool TryLoadFromGameAudio()
        {
            try
            {
                LoggerUtil.Info("🎵 FALLBACK: Searching game's MusicPlayer system...");
                var musicPlayer = Il2CppScheduleOne.Audio.MusicPlayer.instance;
                if (musicPlayer != null && musicPlayer.Tracks != null)
                {
                    LoggerUtil.Info($"Found MusicPlayer with {musicPlayer.Tracks.Count} tracks");
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
                                LoggerUtil.Info($"   ♪ Added game audio: '{trackName}' ({clip.length:F1}s)");
                            }
                        }
                    }
                    
                    if (addedCount > 0)
                    {
                        LoggerUtil.Info($"✅ Loaded {addedCount} tracks from game audio system");
                        return true;
                    }
                }
                else
                {
                    LoggerUtil.Info("⚠️ MusicPlayer.instance is null or has no tracks");
                }
            }
            catch (System.Exception e)
            {
                LoggerUtil.Warn($"❌ Failed to load from MusicPlayer: {e.Message}");
            }
            return false;
        }
        
        private string FormatTrackName(string clipName)
        {
            if (string.IsNullOrEmpty(clipName))
                return "Unknown Track";
                
            // Clean up the track name
            string formatted = clipName;
            
            // Remove file extensions
            if (formatted.Contains("."))
                formatted = formatted.Substring(0, formatted.LastIndexOf('.'));
                
            // Remove common prefixes
            if (formatted.StartsWith("audio_", System.StringComparison.OrdinalIgnoreCase))
                formatted = formatted.Substring(6);
            if (formatted.StartsWith("music_", System.StringComparison.OrdinalIgnoreCase))
                formatted = formatted.Substring(6);
            if (formatted.StartsWith("track_", System.StringComparison.OrdinalIgnoreCase))
                formatted = formatted.Substring(6);
                
            // Replace underscores and dashes with spaces
            formatted = formatted.Replace('_', ' ').Replace('-', ' ');
            
            // Capitalize first letter of each word
            var words = formatted.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length > 0)
                {
                    words[i] = char.ToUpper(words[i][0]) + words[i].Substring(1).ToLower();
                }
            }
            
            return string.Join(" ", words);
        }
        
        private void LogTrackSummary()
        {
            if (tracks.Count > 0)
            {
                LoggerUtil.Info("🎵 LOADED TRACKS:");
                for (int i = 0; i < tracks.Count && i < 10; i++) // Show first 10
                {
                    var track = trackInfo[i];
                    var clip = tracks[i];
                    LoggerUtil.Info($"   {i+1}. '{track.title}' by {track.artist} ({clip.length:F1}s)");
                }
                if (tracks.Count > 10)
                {
                    LoggerUtil.Info($"   ... and {tracks.Count - 10} more tracks");
                }
            }
            else
            {
                LoggerUtil.Error("❌ NO MUSIC FOUND! This could mean:");
                LoggerUtil.Error("   - No jukeboxes in this game scene");
                LoggerUtil.Error("   - Jukeboxes exist but have no music loaded");
                LoggerUtil.Error("   - You're in a menu/loading screen");
                LoggerUtil.Error("   - Try moving to a different area with jukeboxes");
            }
        }

        private void TryInitialAttachment()
        {
            LoggerUtil.Info("BackSpeakerManager: Attempting initial attachment...");
            LoggerUtil.Info($"BackSpeakerManager: Player.Local is null: {Player.Local == null}");
            
            if (Player.Local != null)
            {
                LoggerUtil.Info("BackSpeakerManager: Player already spawned, attaching immediately");
                OnLocalPlayerSpawned();
            }
            else
            {
                LoggerUtil.Info("BackSpeakerManager: Player not yet spawned, waiting for spawn event");
                LoggerUtil.Info("BackSpeakerManager: You may need to spawn/respawn in-game for auto-attach to work");
                
                // Also try to poll for player periodically as a backup
                MelonLoader.MelonCoroutines.Start(PollForPlayer());
            }
        }

        private System.Collections.IEnumerator PollForPlayer()
        {
            int attempts = 0;
            while (Player.Local == null && attempts < 30) // Try for 30 seconds
            {
                yield return new UnityEngine.WaitForSeconds(1f);
                attempts++;
                LoggerUtil.Info($"BackSpeakerManager: Polling for player... attempt {attempts}/30");
                
                if (Player.Local != null)
                {
                    LoggerUtil.Info("BackSpeakerManager: Player found via polling!");
                    OnLocalPlayerSpawned();
                    yield break;
                }
            }
            
            if (Player.Local == null)
            {
                LoggerUtil.Warn("BackSpeakerManager: Player not found after 30 seconds. Try spawning/respawning in game.");
            }
        }

        private void OnLocalPlayerSpawned()
        {
            LoggerUtil.Info("BackSpeakerManager: Player spawn detected, starting auto-attachment");
            currentPlayer = Player.Local;
            if (currentPlayer == null) {
                LoggerUtil.Warn("Local player not found after spawn event!");
                return;
            }
            
            if (audioSource != null)
            {
                LoggerUtil.Info("BackSpeakerManager: Speaker already attached, skipping");
                return;
            }
            
            AttachSpeakerToPlayer(currentPlayer);
        }

        private void AttachSpeakerToPlayer(Player player)
        {
            LoggerUtil.Info("BackSpeakerManager: Starting speaker attachment process...");
            var playerObj = player.LocalGameObject;
            if (playerObj == null) {
                LoggerUtil.Warn("Player GameObject not found! Retrying in 2 seconds...");
                // Retry attachment after a delay
                MelonLoader.MelonCoroutines.Start(RetryAttachment(player));
                return;
            }
            
            try
            {
                speakerObject = new GameObject("BackSpeaker");
                speakerObject.transform.SetParent(playerObj.transform);
                speakerObject.transform.localPosition = new Vector3(0, 1, -0.3f); // Behind player
                audioSource = speakerObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 1.0f; // 3D sound
                audioSource.loop = false; // Don't loop - we'll handle auto-advance manually
                audioSource.volume = 0.7f; // Default volume
                
                LoggerUtil.Info("BackSpeakerManager: ✅ Speaker successfully attached to player!");
                
                // Auto-load tracks immediately after successful attachment
                LoadJukeboxTracksAfterAttachment();
                
                // Force UI update to show new status
                OnTracksReloaded?.Invoke();
            }
            catch (System.Exception e)
            {
                LoggerUtil.Error($"BackSpeakerManager: Failed to attach speaker: {e.Message}");
                // Retry after error
                MelonLoader.MelonCoroutines.Start(RetryAttachment(player));
            }
        }

        private System.Collections.IEnumerator RetryAttachment(Player player)
        {
            yield return new UnityEngine.WaitForSeconds(2f);
            LoggerUtil.Info("BackSpeakerManager: Retrying speaker attachment...");
            AttachSpeakerToPlayer(player);
        }

        private void LoadJukeboxTracksAfterAttachment()
        {
            LoggerUtil.Info("BackSpeakerManager: Auto-loading tracks after successful attachment...");
            LoadJukeboxTracks();
            
            if (tracks.Count > 0)
            {
                LoggerUtil.Info($"BackSpeakerManager: ✅ Auto-loaded {tracks.Count} tracks successfully!");
                SetTrack(currentTrackIndex);
                OnTracksReloaded?.Invoke(); // Update UI immediately
            }
            else
            {
                LoggerUtil.Warn("BackSpeakerManager: No tracks found after auto-load, will retry...");
                // Retry loading tracks after a delay
                MelonLoader.MelonCoroutines.Start(RetryTrackLoading());
            }
        }

        private System.Collections.IEnumerator RetryTrackLoading()
        {
            yield return new UnityEngine.WaitForSeconds(3f);
            LoggerUtil.Info("BackSpeakerManager: Retrying track loading...");
            LoadJukeboxTracksAfterAttachment();
        }

        private void SetTrack(int index)
        {
            if (audioSource == null || tracks.Count == 0) return;
            currentTrackIndex = Mathf.Clamp(index, 0, tracks.Count - 1);
            audioSource.clip = tracks[currentTrackIndex];
            if (isPlaying)
                audioSource.Play();
        }

        public void Play()
        {
            LoggerUtil.Info($"Play() called. AudioSource null: {audioSource == null}, Clip null: {audioSource?.clip == null}, Track count: {tracks.Count}");
            if (audioSource == null)
            {
                LoggerUtil.Warn("AudioSource is null - player might not be spawned yet");
                return;
            }
            
            if (audioSource.clip == null)
            {
                LoggerUtil.Warn("AudioSource clip is null - setting track");
                SetTrack(currentTrackIndex);
            }
            
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
                isPlaying = true;
                LoggerUtil.Info("Audio started playing");
            }
        }

        public void Pause()
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Pause();
                isPlaying = false;
            }
        }

        public void TogglePlayPause()
        {
            if (isPlaying)
                Pause();
            else
                Play();
        }

        public void SetVolume(float volume)
        {
            LoggerUtil.Info($"SetVolume() called with value: {volume}");
            if (audioSource != null)
            {
                audioSource.volume = Mathf.Clamp01(volume);
                LoggerUtil.Info($"Volume set to: {audioSource.volume}");
            }
            else
            {
                LoggerUtil.Warn("AudioSource is null - cannot set volume");
            }
        }

        public void NextTrack()
        {
            LoggerUtil.Info($"NextTrack() called. Current index: {currentTrackIndex}, Track count: {tracks.Count}");
            if (tracks.Count == 0) return;
            int next = (currentTrackIndex + 1) % tracks.Count;
            LoggerUtil.Info($"Moving to track {next}");
            SetTrack(next);
            Play();
            // Trigger UI update to show new track
            OnTracksReloaded?.Invoke();
        }

        private void NextTrackWithoutAutoPlay()
        {
            LoggerUtil.Info($"NextTrackWithoutAutoPlay() called. Current index: {currentTrackIndex}, Track count: {tracks.Count}");
            if (tracks.Count == 0) return;
            int next = (currentTrackIndex + 1) % tracks.Count;
            LoggerUtil.Info($"Moving to track {next}");
            SetTrack(next);
            // Don't call Play() here - let the track advance but continue playing from previous state
            if (isPlaying) {
                Play(); // Only start playing if we were already playing
            }
            // Trigger UI update to show new track
            OnTracksReloaded?.Invoke();
        }

        public void PreviousTrack()
        {
            LoggerUtil.Info($"PreviousTrack() called. Current index: {currentTrackIndex}, Track count: {tracks.Count}");
            if (tracks.Count == 0) return;
            int prev = (currentTrackIndex - 1 + tracks.Count) % tracks.Count;
            LoggerUtil.Info($"Moving to track {prev}");
            SetTrack(prev);
            Play();
            // Trigger UI update to show new track
            OnTracksReloaded?.Invoke();
        }

        public string GetCurrentTrackInfo()
        {
            if (trackInfo.Count == 0) 
            {
                return "No Songs Loaded";
            }
            return trackInfo[currentTrackIndex].title;
        }

        public string GetCurrentArtistInfo()
        {
            if (trackInfo.Count == 0) 
            {
                return "Load some music first";
            }
            return trackInfo[currentTrackIndex].artist;
        }

        public bool IsPlaying => isPlaying;
        public float CurrentVolume => audioSource != null ? audioSource.volume : 0.5f;
        
        // Allow manual reloading tracks if auto-load failed
        public void ReloadTracks()
        {
            LoggerUtil.Info("Manual track reload requested...");
            if (audioSource == null)
            {
                LoggerUtil.Warn("Cannot reload tracks - speaker not attached yet");
                return;
            }
            LoadJukeboxTracksAfterAttachment();
        }
        
        // Get track count for debugging
        public int GetTrackCount() => tracks.Count;
        
        // Call this regularly to check for auto-advance
        public void Update()
        {
            // Check if current track finished and handle repeat/auto-advance
            if (audioSource != null && isPlaying && !audioSource.isPlaying)
            {
                LoggerUtil.Info($"Current track finished. Repeat mode: {repeatMode}");
                
                switch (repeatMode)
                {
                    case RepeatMode.RepeatOne:
                        LoggerUtil.Info("Repeating current track");
                        Play(); // Just restart the same track
                        break;
                        
                    case RepeatMode.RepeatAll:
                        LoggerUtil.Info("Repeat all - advancing to next track");
                        NextTrack();
                        break;
                        
                    case RepeatMode.None:
                        if (currentTrackIndex < tracks.Count - 1)
                        {
                            LoggerUtil.Info("Auto-advancing to next track (no repeat mode)");
                            NextTrackWithoutAutoPlay(); // Advance and continue playing
                        }
                        else
                        {
                            LoggerUtil.Info("Reached end of playlist - stopping and resetting to first track");
                            isPlaying = false;
                            currentTrackIndex = 0; // Reset to first track
                            SetTrack(0); // Set first track but don't play it
                            OnTracksReloaded?.Invoke(); // Update UI to show stopped state
                        }
                        break;
                }
            }
        }
        
        // Check if audio system is ready
        public bool IsAudioReady() => audioSource != null && currentPlayer != null;
        
        // Auto-advance is now always enabled except for specific repeat modes
        
        // Repeat mode control
        public RepeatMode RepeatMode 
        { 
            get => repeatMode; 
            set 
            { 
                repeatMode = value;
                LoggerUtil.Info($"Repeat mode set to: {repeatMode}");
            } 
        }
        
        // Song progress and seeking
        public float CurrentTime => audioSource != null ? audioSource.time : 0f;
        public float TotalTime => audioSource != null && audioSource.clip != null ? audioSource.clip.length : 0f;
        public float Progress => TotalTime > 0 ? CurrentTime / TotalTime : 0f;
        
        public void SeekToTime(float time)
        {
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.time = Mathf.Clamp(time, 0f, audioSource.clip.length);
                LoggerUtil.Info($"Seeked to time: {audioSource.time:F2}s");
            }
        }
        
        public void SeekToProgress(float progress)
        {
            if (audioSource != null && audioSource.clip != null)
            {
                float targetTime = Mathf.Clamp01(progress) * audioSource.clip.length;
                SeekToTime(targetTime);
            }
        }
        
        // Playlist management
        public void PlayTrack(int index)
        {
            if (index >= 0 && index < tracks.Count)
            {
                LoggerUtil.Info($"Playing track {index} from playlist");
                SetTrack(index);
                Play();
                OnTracksReloaded?.Invoke(); // Update UI
            }
        }
        
        public List<(string title, string artist)> GetAllTracks() => new List<(string, string)>(trackInfo);
        public int CurrentTrackIndex => currentTrackIndex;
        
        private string lastStatus = "";
        
        // Get attachment status for UI
        public string GetAttachmentStatus()
        {
            string status;
            if (audioSource != null && currentPlayer != null)
                status = "ATTACHED";
            else if (currentPlayer != null)
                status = "ATTACHING...";
            else if (Player.Local != null)
                status = "PLAYER FOUND";
            else
                status = "SPAWN IN GAME";
                
            // Only log when status changes to reduce spam
            if (status != lastStatus)
            {
                LoggerUtil.Info($"GetAttachmentStatus: audioSource={audioSource != null}, currentPlayer={currentPlayer != null}, status={status}");
                lastStatus = status;
            }
            return status;
        }

        // Manual trigger for attachment if auto-detection fails
        public void TriggerManualAttachment()
        {
            LoggerUtil.Info("BackSpeakerManager: Manual attachment triggered");
            if (Player.Local != null && audioSource == null)
            {
                LoggerUtil.Info("BackSpeakerManager: Player found, starting manual attachment");
                OnLocalPlayerSpawned();
            }
            else if (Player.Local == null)
            {
                LoggerUtil.Warn("BackSpeakerManager: No player found - spawn in game first");
            }
            else
            {
                LoggerUtil.Info("BackSpeakerManager: Already attached or in progress");
            }
        }
    }
}