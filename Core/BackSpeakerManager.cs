using System;
using System.Collections.Generic;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Modules;
using BackSpeakerMod.Core.Features.Audio.Managers;
using BackSpeakerMod.Utils;
using UnityEngine;

namespace BackSpeakerMod.Core
{
    /// <summary>
    /// Main BackSpeaker manager using the consolidated SystemManager
    /// Provides a clean, simple API for all mod functionality
    /// </summary>
    public class BackSpeakerManager
    {
        private static BackSpeakerManager? _instance;
        public static BackSpeakerManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new BackSpeakerManager();
                }
                return _instance;
            }
        }
        
        private readonly SystemManager systemManager;

        /// <summary>
        /// Event to notify UI when tracks are reloaded
        /// </summary>
        public Action? OnTracksReloaded
        {
            get => systemManager.OnTracksReloaded;
            set => systemManager.OnTracksReloaded = value;
        }

        /// <summary>
        /// Initialize BackSpeaker manager using consolidated SystemManager
        /// </summary>
        public BackSpeakerManager()
        {
            LoggingSystem.Info("BackSpeakerManager: Initializing with SystemManager", "Core");
            systemManager = new SystemManager();
            systemManager.Initialize();
        }

        /// <summary>
        /// Update all systems
        /// </summary>
        public void Update() => systemManager.Update();

        // Public API - Audio Control
        public void Play() => systemManager.Play();
        public void Pause() => systemManager.Pause();
        public void TogglePlayPause() => systemManager.TogglePlayPause();
        public void SetVolume(float volume) => systemManager.SetVolume(volume);
        public void NextTrack() => systemManager.NextTrack();
        public void PreviousTrack() => systemManager.PreviousTrack();
        public void PlayTrack(int index) => systemManager.PlayTrack(index);
        public void SeekToTime(float time) => systemManager.SeekToTime(time);
        public void SeekToProgress(float progress) => systemManager.SeekToProgress(progress);
        public void ReloadTracks() => systemManager.ReloadTracks();

        // Public API - Audio Properties
        public bool IsPlaying => systemManager.IsPlaying;
        public float CurrentVolume => systemManager.CurrentVolume;
        public int GetTrackCount() => systemManager.GetTrackCount();
        public bool IsAudioReady() => systemManager.IsAudioReady();
        public YouTubeDownloadManager GetDownloadManager() => systemManager.GetDownloadManager();
        public float CurrentTime => systemManager.CurrentTime;
        public float TotalTime => systemManager.TotalTime;
        public float Progress => systemManager.Progress;
        public int CurrentTrackIndex => systemManager.CurrentTrackIndex;
        public string GetCurrentTrackInfo() => systemManager.GetCurrentTrackInfo();
        public string GetCurrentArtistInfo() => systemManager.GetCurrentArtistInfo();
        public List<(string title, string artist)> GetAllTracks() => systemManager.GetAllTracks();

        public RepeatMode RepeatMode
        {
            get => systemManager.RepeatMode;
            set => systemManager.RepeatMode = value;
        }

        // Public API - Music Source Management
        public void SetMusicSource(MusicSourceType sourceType) => systemManager.SetMusicSource(sourceType);
        public MusicSourceType GetCurrentMusicSource() => systemManager.GetCurrentMusicSource();
        public List<(MusicSourceType type, string name, bool available)> GetAvailableMusicSources() => systemManager.GetAvailableMusicSources();
        public void LoadTracksFromCurrentSource() => systemManager.LoadTracksFromCurrentSource();
        public void ForceLoadFromSource(MusicSourceType sourceType) => systemManager.ForceLoadFromSource(sourceType);
        
        // Public API - Session Track Management
        public void AddTrackToSession(MusicSourceType sessionType, AudioClip audioClip, string title, string artist) => systemManager.AddTrackToSession(sessionType, audioClip, title, artist);
        public void AddTracksToSession(MusicSourceType sessionType, List<AudioClip> audioClips, List<(string title, string artist)> trackInfo) => systemManager.AddTracksToSession(sessionType, audioClips, trackInfo);
        public AudioSession GetSession(MusicSourceType sessionType) => systemManager.GetSession(sessionType);
        
        // Public API - YouTube Playlist Management
        public bool AddYouTubeSong(SongDetails songDetails) => systemManager.AddYouTubeSong(songDetails);
        public bool RemoveYouTubeSong(string url) => systemManager.RemoveYouTubeSong(url);
        public bool ContainsYouTubeSong(string url) => systemManager.ContainsYouTubeSong(url);
        public void ClearYouTubePlaylist() => systemManager.ClearYouTubePlaylist();
        public void LoadYouTubePlaylist(List<SongDetails> playlistSongs) => systemManager.LoadYouTubePlaylist(playlistSongs);

        // Public API - Player Attachment
        public void TriggerManualAttachment() => systemManager.TriggerManualAttachment();
        public string GetAttachmentStatus() => systemManager.GetAttachmentStatus();

        // Public API - Headphones
        public bool AttachHeadphones() => systemManager.AttachHeadphones();
        public void RemoveHeadphones() => systemManager.RemoveHeadphones();
        public bool ToggleHeadphones() => systemManager.ToggleHeadphones();
        public bool AreHeadphonesAttached() => systemManager.AreHeadphonesAttached();
        public string GetHeadphoneStatus() => systemManager.GetHeadphoneStatus();

        // Public API - Configuration
        public T? GetSetting<T>(string key, T? defaultValue = default(T)) => systemManager.GetSetting(key, defaultValue);
        public void SetSetting<T>(string key, T value) => systemManager.SetSetting(key, value);

        /// <summary>
        /// Get overall system status
        /// </summary>
        public string GetSystemStatus() => systemManager.GetSystemStatus();
        
        /// <summary>
        /// Whether system is initialized
        /// </summary>
        public bool IsInitialized => systemManager.IsInitialized;
        
        /// <summary>
        /// Shutdown all systems
        /// </summary>
        public void Shutdown() => systemManager.Shutdown();
    }
}