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
            LoggerUtil.Info($"Loaded {tracks.Count} music tracks from jukeboxes.");
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
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
                isPlaying = true;
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
            if (audioSource != null)
                audioSource.volume = Mathf.Clamp01(volume);
        }

        public void NextTrack()
        {
            if (tracks.Count == 0) return;
            int next = (currentTrackIndex + 1) % tracks.Count;
            SetTrack(next);
            Play();
        }

        public void PreviousTrack()
        {
            if (tracks.Count == 0) return;
            int prev = (currentTrackIndex - 1 + tracks.Count) % tracks.Count;
            SetTrack(prev);
            Play();
        }

        public string GetCurrentTrackInfo()
        {
            if (trackInfo.Count == 0) return "No Track";
            return trackInfo[currentTrackIndex].title;
        }

        public string GetCurrentArtistInfo()
        {
            if (trackInfo.Count == 0) return "";
            return trackInfo[currentTrackIndex].artist;
        }

        public bool IsPlaying => isPlaying;
        public float CurrentVolume => audioSource != null ? audioSource.volume : 0.5f;
    }
} 