using UnityEngine;
using MelonLoader;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.Audio;
using System.Collections.Generic;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.Core
{
    public class BackSpeakerManager
    {
        private GameObject speakerObject;
        private AudioSource audioSource;
        private Player currentPlayer;
        private List<AudioClip> tracks = new List<AudioClip>();
        private List<(string title, string artist)> trackInfo = new List<(string, string)>();
        private int currentTrackIndex = 0;
        private bool isPlaying = false;
        
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
            audioSource.loop = true;
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
        
        // Check if audio system is ready
        public bool IsAudioReady() => audioSource != null && currentPlayer != null;
        
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