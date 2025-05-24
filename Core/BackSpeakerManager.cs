using UnityEngine;
using MelonLoader;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Audio;
using System.Collections.Generic;
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
            // Subscribe to player spawn event
            Player.onLocalPlayerSpawned += (Il2CppSystem.Action)(OnLocalPlayerSpawned);
            // Load all jukebox music from the game
            LoadJukeboxTracks();
        }

        private void LoadJukeboxTracks()
        {
            tracks.Clear();
            trackInfo.Clear();
            
            // Try to get music from the game's MusicPlayer singleton first
            try
            {
                var musicPlayer = Il2CppScheduleOne.Audio.MusicPlayer.instance;
                if (musicPlayer != null && musicPlayer.Tracks != null)
                {
                    LoggerUtil.Info($"Found MusicPlayer with {musicPlayer.Tracks.Count} tracks");
                    var seen = new HashSet<AudioClip>();
                    
                    foreach (var musicTrack in musicPlayer.Tracks)
                    {
                        if (musicTrack?.Controller?.AudioSource?.clip != null)
                        {
                            var clip = musicTrack.Controller.AudioSource.clip;
                            if (seen.Add(clip))
                            {
                                tracks.Add(clip);
                                // Use track name if available, otherwise clip name
                                string trackName = !string.IsNullOrEmpty(musicTrack.TrackName) ? musicTrack.TrackName : clip.name;
                                trackInfo.Add((trackName, "Game Music"));
                                LoggerUtil.Info($"Added track: {trackName}");
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                LoggerUtil.Warn($"Failed to load from MusicPlayer: {e.Message}");
            }
            
            // Fallback to AmbientLoopJukebox if MusicPlayer didn't work
            if (tracks.Count == 0)
            {
                LoggerUtil.Info("Falling back to AmbientLoopJukebox search...");
                var jukeboxes = GameObject.FindObjectsOfType<AmbientLoopJukebox>();
                var seen = new HashSet<AudioClip>();
                foreach (var jukebox in jukeboxes)
                {
                    var clips = jukebox.Clips;
                    if (clips != null)
                    {
                        foreach (var clip in clips)
                        {
                            if (clip != null && seen.Add(clip))
                            {
                                tracks.Add(clip);
                                // Use clip name as title, artist unknown
                                trackInfo.Add((clip.name, "Game Artist"));
                            }
                        }
                    }
                }
            }
            
            LoggerUtil.Info($"Loaded {tracks.Count} music tracks total.");
            
            // Reset to first track if we have tracks and current index is invalid
            if (tracks.Count > 0 && currentTrackIndex >= tracks.Count)
            {
                currentTrackIndex = 0;
                SetTrack(currentTrackIndex);
            }
            
            // Notify UI that tracks have been reloaded
            OnTracksReloaded?.Invoke();
        }

        private void OnLocalPlayerSpawned()
        {
            currentPlayer = Player.Local;
            if (currentPlayer == null) {
                LoggerUtil.Warn("Local player not found!");
                return;
            }
            AttachSpeakerToPlayer(currentPlayer);
        }

        private void AttachSpeakerToPlayer(Player player)
        {
            var playerObj = player.LocalGameObject;
            if (playerObj == null) {
                LoggerUtil.Warn("Player GameObject not found!");
                return;
            }
            speakerObject = new GameObject("BackSpeaker");
            speakerObject.transform.SetParent(playerObj.transform);
            speakerObject.transform.localPosition = new Vector3(0, 1, -0.3f); // Example position
            audioSource = speakerObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f; // 3D sound
            audioSource.loop = false; // Don't loop - we'll handle auto-advance manually
            LoggerUtil.Info("Back speaker attached to player.");
            SetTrack(currentTrackIndex);
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
                LoggerUtil.Info("GetCurrentTrackInfo: No tracks loaded");
                return "No Songs Loaded";
            }
            LoggerUtil.Info($"GetCurrentTrackInfo: Track {currentTrackIndex}: {trackInfo[currentTrackIndex].title}");
            return trackInfo[currentTrackIndex].title;
        }

        public string GetCurrentArtistInfo()
        {
            if (trackInfo.Count == 0) 
            {
                LoggerUtil.Info("GetCurrentArtistInfo: No tracks loaded");
                return "Load some music first";
            }
            LoggerUtil.Info($"GetCurrentArtistInfo: Artist {currentTrackIndex}: {trackInfo[currentTrackIndex].artist}");
            return trackInfo[currentTrackIndex].artist;
        }

        public bool IsPlaying => isPlaying;
        public float CurrentVolume => audioSource != null ? audioSource.volume : 0.5f;
        
        // Allow reloading tracks on demand in case they weren't available during initialization
        public void ReloadTracks()
        {
            LoggerUtil.Info("Manually reloading music tracks...");
            LoadJukeboxTracks();
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
        
        // Manually try to attach speaker if player is available
        public void TryAttachSpeaker()
        {
            LoggerUtil.Info("Manually trying to attach speaker...");
            currentPlayer = Player.Local;
            if (currentPlayer != null)
            {
                LoggerUtil.Info("Local player found, attaching speaker");
                AttachSpeakerToPlayer(currentPlayer);
            }
            else
            {
                LoggerUtil.Warn("Local player not found yet");
            }
        }
    }
} 