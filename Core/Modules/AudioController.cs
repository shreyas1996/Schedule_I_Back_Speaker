using UnityEngine;
using System.Collections.Generic;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using System;

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
        
        public Action OnTracksChanged;

        public void Initialize(AudioSource audioSource)
        {
            this.audioSource = audioSource;
            LoggingSystem.Info("AudioController initialized", "Audio");
        }

        public void Reset()
        {
            // Stop any playing audio first
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            
            isPlaying = false;
            audioSource = null;
            tracks.Clear();
            trackInfo.Clear();
            currentTrackIndex = 0;
            
            LoggingSystem.Info("AudioController reset - audio stopped and cleared", "Audio");
        }

        public void SetTracks(List<AudioClip> tracks, List<(string title, string artist)> trackInfo)
        {
            this.tracks = tracks;
            this.trackInfo = trackInfo;
            
            // Always set the first track when tracks are loaded
            if (tracks.Count > 0)
            {
                currentTrackIndex = 0;
                SetTrack(currentTrackIndex);
            }
            
            OnTracksChanged?.Invoke();
            LoggingSystem.Info($"Loaded {tracks.Count} tracks", "Audio");
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
            if (audioSource != null)
            {
                audioSource.Pause();
                isPlaying = false;
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
            try
            {
                if (trackInfo == null || trackInfo.Count == 0)
                {
                    LoggingSystem.Warning("No track info available - trackInfo is null or empty", "Audio");
                    return "No Track";
                }
                
                if (currentTrackIndex < 0 || currentTrackIndex >= trackInfo.Count)
                {
                    LoggingSystem.Warning($"Invalid track index {currentTrackIndex} - valid range is 0 to {trackInfo.Count - 1}", "Audio");
                    return "No Track";
                }
                
                var track = trackInfo[currentTrackIndex];
                string title = track.title;
                
                if (string.IsNullOrEmpty(title) || title.Trim().Length == 0)
                {
                    LoggingSystem.Warning($"Track {currentTrackIndex} has empty or null title", "Audio");
                    return "Unknown Track";
                }
                
                return title;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error getting current track info: {ex.Message}", "Audio");
                return "Error Loading Track";
            }
        }

        public string GetCurrentArtistInfo()
        {
            try
            {
                if (trackInfo == null || trackInfo.Count == 0)
                    return "Unknown Artist";
                    
                if (currentTrackIndex < 0 || currentTrackIndex >= trackInfo.Count)
                    return "Unknown Artist";
                
                var track = trackInfo[currentTrackIndex];
                string artist = track.artist;
                
                if (string.IsNullOrEmpty(artist) || artist.Trim().Length == 0)
                    return "Unknown Artist";
                    
                return artist;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error getting current artist info: {ex.Message}", "Audio");
                return "Unknown Artist";
            }
        }

        public List<(string title, string artist)> GetAllTracks() => new List<(string, string)>(trackInfo);
    }
} 