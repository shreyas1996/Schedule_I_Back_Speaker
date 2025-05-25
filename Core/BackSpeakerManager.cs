using UnityEngine;
using MelonLoader;
using Il2CppScheduleOne.PlayerScripts;
using System.Collections.Generic;
using BackSpeakerMod.Utils;
using BackSpeakerMod.Core.Modules;

namespace BackSpeakerMod.Core
{
    public class BackSpeakerManager
    {
        private PlayerAttachment playerAttachment;
        private AudioController audioController;
        private TrackLoader trackLoader;
        
        // Event to notify UI when tracks are reloaded
        public System.Action OnTracksReloaded;

        public BackSpeakerManager()
        {
            LoggerUtil.Info("BackSpeakerManager: Initializing with modular architecture");
            
            // Initialize modules
            playerAttachment = new PlayerAttachment();
            audioController = new AudioController();
            trackLoader = new TrackLoader();
            
            // Wire up module events
            playerAttachment.OnSpeakerAttached += OnSpeakerAttached;
            trackLoader.OnTracksLoaded += OnTracksLoaded;
            audioController.OnTracksChanged += () => OnTracksReloaded?.Invoke();
            
            // Start the attachment process
            playerAttachment.Initialize();
        }

        private void OnSpeakerAttached(AudioSource audioSource)
        {
            LoggerUtil.Info("BackSpeakerManager: Speaker attached, initializing audio controller");
            audioController.Initialize(audioSource);
            LoadJukeboxTracksAfterAttachment();
        }

        private void OnTracksLoaded(List<AudioClip> tracks, List<(string title, string artist)> trackInfo)
        {
            audioController.SetTracks(tracks, trackInfo);
            LoggerUtil.Info($"BackSpeakerManager: Loaded {tracks.Count} tracks");
        }

        private void LoadJukeboxTracksAfterAttachment()
        {
            if (playerAttachment.IsAudioReady())
            {
                LoggerUtil.Info("BackSpeakerManager: Audio ready, loading tracks");
                trackLoader.LoadJukeboxTracks();
            }
            else
            {
                LoggerUtil.Info("BackSpeakerManager: Audio not ready, will retry track loading");
                MelonCoroutines.Start(RetryTrackLoading());
            }
        }

        private System.Collections.IEnumerator RetryTrackLoading()
        {
            yield return new WaitForSeconds(2f);
            LoadJukeboxTracksAfterAttachment();
        }

        public void Update()
        {
            audioController?.Update();
        }

        // Public API - delegate to modules
        public void Play() => audioController?.Play();
        public void Pause() => audioController?.Pause();
        public void TogglePlayPause() => audioController?.TogglePlayPause();
        public void SetVolume(float volume) => audioController?.SetVolume(volume);
        public void NextTrack() => audioController?.NextTrack();
        public void PreviousTrack() => audioController?.PreviousTrack();
        public void PlayTrack(int index) => audioController?.PlayTrack(index);
        public void SeekToTime(float time) => audioController?.SeekToTime(time);
        public void SeekToProgress(float progress) => audioController?.SeekToProgress(progress);
        public void ReloadTracks() => trackLoader?.LoadJukeboxTracks();
        public void TriggerManualAttachment() => playerAttachment?.TriggerManualAttachment();
        
        // Properties - delegate to modules
        public bool IsPlaying => audioController?.IsPlaying ?? false;
        public float CurrentVolume => audioController?.CurrentVolume ?? 0.5f;
        public int GetTrackCount() => audioController?.GetTrackCount() ?? 0;
        public bool IsAudioReady() => playerAttachment?.IsAudioReady() ?? false;
        public float CurrentTime => audioController?.CurrentTime ?? 0f;
        public float TotalTime => audioController?.TotalTime ?? 0f;
        public float Progress => audioController?.Progress ?? 0f;
        public int CurrentTrackIndex => audioController?.CurrentTrackIndex ?? 0;
        public string GetAttachmentStatus() => playerAttachment?.GetAttachmentStatus() ?? "Initializing...";
        
        public RepeatMode RepeatMode 
        { 
            get => audioController?.RepeatMode ?? RepeatMode.None;
            set { if (audioController != null) audioController.RepeatMode = value; }
        }

        public string GetCurrentTrackInfo() => audioController?.GetCurrentTrackInfo() ?? "No Track";
        public string GetCurrentArtistInfo() => audioController?.GetCurrentArtistInfo() ?? "Unknown Artist";
        public List<(string title, string artist)> GetAllTracks() => audioController?.GetAllTracks() ?? new List<(string, string)>();
    }
}