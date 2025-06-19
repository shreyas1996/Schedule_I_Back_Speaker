using System;
using System.Collections;
using UnityEngine;
using MelonLoader;
using BackSpeakerMod.S1Wrapper.Interfaces;
using BackSpeakerMod.S1Wrapper;
using BackSpeakerMod.NewBackend.Utils;

namespace BackSpeakerMod.NewBackend
{
    /// <summary>
    /// Manages audio playback and track control
    /// </summary>
    public class AudioManager
    {
        private IPlayer? _player;
        private AudioSource? _audioSource;
        private NewSongDetails? _currentTrack;
        private bool _isPlaying = false;
        private float _volume = 0.7f;
        
        public event Action<NewSongDetails>? OnTrackChanged;
        public event Action<bool>? OnPlayStateChanged;
        
        public NewSongDetails? GetCurrentTrack() => _currentTrack;
        public bool IsPlaying() => _isPlaying;
        
        public IEnumerator Initialize(IPlayer player)
        {
            _player = player;
            NewLoggingSystem.Info("Initializing AudioManager", "AudioManager");
            
            // Setup audio source on player
            SetupAudioSource();
            
            NewLoggingSystem.Info("✓ AudioManager initialized", "AudioManager");
            yield break;
        }
        
        private void SetupAudioSource()
        {
            if (_player?.GameObject == null) return;
            
            _audioSource = _player.GameObject.GetComponent<AudioSource>();
            if (_audioSource == null)
            {
                _audioSource = _player.GameObject.AddComponent<AudioSource>();
            }
            
            // Configure audio source
            _audioSource.volume = _volume;
            _audioSource.loop = false;
            _audioSource.playOnAwake = false;
            _audioSource.spatialBlend = 0f; // 2D audio for headphones
            
            NewLoggingSystem.Info("Audio source setup complete", "AudioManager");
        }
        
        public void Play()
        {
            if (_audioSource != null && _audioSource.clip != null)
            {
                _audioSource.Play();
                _isPlaying = true;
                OnPlayStateChanged?.Invoke(true);
                NewLoggingSystem.Info($"Playing: {_currentTrack?.title ?? "Unknown"}", "AudioManager");
            }
            else
            {
                NewLoggingSystem.Warning("Cannot play - no audio clip loaded", "AudioManager");
            }
        }
        
        public void Pause()
        {
            if (_audioSource != null)
            {
                _audioSource.Pause();
                _isPlaying = false;
                OnPlayStateChanged?.Invoke(false);
                NewLoggingSystem.Info("Playback paused", "AudioManager");
            }
        }
        
        public void Stop()
        {
            if (_audioSource != null)
            {
                _audioSource.Stop();
                _isPlaying = false;
                OnPlayStateChanged?.Invoke(false);
                NewLoggingSystem.Info("Playback stopped", "AudioManager");
            }
        }
        
        public void SetTrack(NewSongDetails track)
        {
            if (track == null) return;
            
            _currentTrack = track;
            
            // Load audio clip if file exists
            if (!string.IsNullOrEmpty(track.cachedFilePath) && System.IO.File.Exists(track.cachedFilePath))
            {
                MelonCoroutines.Start(AudioLoaderHelper.LoadAudioClipFromFile(track.cachedFilePath, OnAudioClipLoaded));
            }
            
            OnTrackChanged?.Invoke(track);
            NewLoggingSystem.Info($"Track set: {track.title}", "AudioManager");
        }
        
        private void OnAudioClipLoaded(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.clip = clip;
                NewLoggingSystem.Info($"Audio clip loaded using AudioHelper: {clip.name}", "AudioManager");
            }
        }
        
        public void SetVolume(float volume)
        {
            _volume = Mathf.Clamp01(volume);
            if (_audioSource != null)
            {
                _audioSource.volume = _volume;
            }
        }
        
        public void Shutdown()
        {
            NewLoggingSystem.Info("Shutting down AudioManager", "AudioManager");
            
            // Stop any playing audio
            Stop();
            
            // Clear references
            _player = null;
            _audioSource = null;
            _currentTrack = null;
            
            NewLoggingSystem.Info("✓ AudioManager shutdown complete", "AudioManager");
        }
    }
} 