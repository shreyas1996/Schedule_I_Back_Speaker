using UnityEngine;
using System.Collections.Generic;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.Core.Modules
{
    public enum RepeatMode
    {
        None,       // No repeat - stop after playlist ends
        RepeatOne,  // Repeat current song
        RepeatAll   // Repeat entire playlist
    }

    public class AudioController
    {
        private AudioSource audioSource;
        private List<AudioClip> tracks = new List<AudioClip>();
        private List<(string title, string artist)> trackInfo = new List<(string, string)>();
        private int currentTrackIndex = 0;
        private bool isPlaying = false;
        private RepeatMode repeatMode = RepeatMode.None;
        
        public System.Action OnTracksChanged;

        public void Initialize(AudioSource audioSource)
        {
            this.audioSource = audioSource;
            LoggerUtil.Info("AudioController: Initialized");
        }

        public void SetTracks(List<AudioClip> tracks, List<(string title, string artist)> trackInfo)
        {
            this.tracks = tracks;
            this.trackInfo = trackInfo;
            
            // Reset to first track if current index is invalid
            if (tracks.Count > 0 && currentTrackIndex >= tracks.Count)
            {
                currentTrackIndex = 0;
                SetTrack(currentTrackIndex);
            }
            
            OnTracksChanged?.Invoke();
            LoggerUtil.Info($"AudioController: Set {tracks.Count} tracks");
        }

        public void Play()
        {
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
                isPlaying = true;
                LoggerUtil.Info($"AudioController: Playing '{GetCurrentTrackInfo()}'");
            }
        }

        public void Pause()
        {
            if (audioSource != null)
            {
                audioSource.Pause();
                isPlaying = false;
                LoggerUtil.Info("AudioController: Paused");
            }
        }

        public void TogglePlayPause()
        {
            if (IsPlaying) Pause();
            else Play();
        }

        public void SetVolume(float volume)
        {
            volume = Mathf.Clamp01(volume);
            if (audioSource != null)
            {
                audioSource.volume = volume;
                LoggerUtil.Info($"AudioController: Volume set to {volume:P0}");
            }
        }

        public void NextTrack()
        {
            if (tracks.Count == 0) return;
            
            bool wasPlaying = isPlaying;
            NextTrackWithoutAutoPlay();
            
            if (wasPlaying) Play();
        }

        public void PreviousTrack()
        {
            if (tracks.Count == 0) return;
            
            bool wasPlaying = isPlaying;
            currentTrackIndex = (currentTrackIndex - 1 + tracks.Count) % tracks.Count;
            SetTrack(currentTrackIndex);
            
            if (wasPlaying) Play();
        }

        public void PlayTrack(int index)
        {
            if (index >= 0 && index < tracks.Count)
            {
                currentTrackIndex = index;
                SetTrack(currentTrackIndex);
                Play();
            }
        }

        public void SeekToTime(float time)
        {
            if (audioSource != null && audioSource.clip != null)
            {
                time = Mathf.Clamp(time, 0f, audioSource.clip.length);
                audioSource.time = time;
            }
        }

        public void SeekToProgress(float progress)
        {
            if (audioSource != null && audioSource.clip != null)
            {
                progress = Mathf.Clamp01(progress);
                audioSource.time = progress * audioSource.clip.length;
            }
        }

        public void Update()
        {
            CheckForTrackEnd();
        }

        private void SetTrack(int index)
        {
            if (index >= 0 && index < tracks.Count && audioSource != null)
            {
                audioSource.clip = tracks[index];
                currentTrackIndex = index;
                LoggerUtil.Info($"AudioController: Set track {index}: '{GetCurrentTrackInfo()}'");
            }
        }

        private void NextTrackWithoutAutoPlay()
        {
            if (repeatMode == RepeatMode.RepeatOne) return;
            
            if (currentTrackIndex < tracks.Count - 1)
            {
                currentTrackIndex++;
            }
            else
            {
                currentTrackIndex = repeatMode == RepeatMode.RepeatAll ? 0 : tracks.Count - 1;
            }
            
            SetTrack(currentTrackIndex);
        }

        private void CheckForTrackEnd()
        {
            if (audioSource != null && isPlaying && !audioSource.isPlaying)
            {
                if (repeatMode == RepeatMode.RepeatOne)
                {
                    Play(); // Restart same track
                }
                else if (repeatMode == RepeatMode.RepeatAll || currentTrackIndex < tracks.Count - 1)
                {
                    NextTrack(); // Auto advance
                }
                else
                {
                    isPlaying = false; // Stop at end of playlist
                }
            }
        }

        // Properties and getters
        public bool IsPlaying => isPlaying;
        public float CurrentVolume => audioSource != null ? audioSource.volume : 0.5f;
        public int GetTrackCount() => tracks.Count;
        public float CurrentTime => audioSource != null ? audioSource.time : 0f;
        public float TotalTime => audioSource != null && audioSource.clip != null ? audioSource.clip.length : 0f;
        public float Progress => TotalTime > 0 ? CurrentTime / TotalTime : 0f;
        public int CurrentTrackIndex => currentTrackIndex;
        public bool IsAudioReady() => audioSource != null;
        
        public RepeatMode RepeatMode 
        { 
            get => repeatMode;
            set => repeatMode = value;
        }

        public string GetCurrentTrackInfo()
        {
            if (currentTrackIndex >= 0 && currentTrackIndex < trackInfo.Count)
                return trackInfo[currentTrackIndex].title;
            return "No Track";
        }

        public string GetCurrentArtistInfo()
        {
            if (currentTrackIndex >= 0 && currentTrackIndex < trackInfo.Count)
                return trackInfo[currentTrackIndex].artist;
            return "Unknown Artist";
        }

        public List<(string title, string artist)> GetAllTracks() => new List<(string, string)>(trackInfo);
    }
} 