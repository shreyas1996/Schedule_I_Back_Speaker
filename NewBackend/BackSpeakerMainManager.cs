using System;
using UnityEngine;
using MelonLoader;
using BackSpeakerMod.S1Wrapper;
using BackSpeakerMod.S1Wrapper.Interfaces;
using BackSpeakerMod.NewBackend.Utils;


namespace BackSpeakerMod.NewBackend
{
    /// <summary>
    /// Simple main manager that coordinates individual module managers
    /// Clean, non-redundant architecture
    /// </summary>
    public class BackSpeakerMainManager
    {
        private static BackSpeakerMainManager? _instance;
        private static readonly object _lock = new object();
        
        public static BackSpeakerMainManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new BackSpeakerMainManager();
                    }
                }
                return _instance;
            }
        }
        
        // Module managers
        private HeadphoneManager? _headphoneManager;
        private AudioManager? _audioManager;
        private PlaylistManager? _playlistManager;
        
        // Core state
        private bool _isInitialized = false;
        private IPlayer? _currentPlayer;
        
        // Events
        public event Action<NewSongDetails>? OnTrackChanged;
        public event Action<bool>? OnPlayStateChanged;
        public event Action<bool>? OnHeadphonesStateChanged;
        
        private BackSpeakerMainManager()
        {
            NewLoggingSystem.Info("BackSpeakerMainManager created", "MainManager");
        }
        
        public void Initialize(IPlayer player)
        {
            if (_isInitialized)
            {
                NewLoggingSystem.Warning("MainManager already initialized", "MainManager");
                return;
            }
            
            _currentPlayer = player;
            NewLoggingSystem.Info($"Initializing MainManager for player: {player.Name}", "MainManager");
            
            MelonCoroutines.Start(InitializeAsync());
        }
        
        private System.Collections.IEnumerator InitializeAsync()
        {
            NewLoggingSystem.Info("Starting MainManager initialization...", "MainManager");
            
            // Initialize headphone manager
            _headphoneManager = new HeadphoneManager();
            yield return MelonCoroutines.Start(_headphoneManager.Initialize(_currentPlayer));
            
            // Initialize audio manager
            _audioManager = new AudioManager();
            yield return MelonCoroutines.Start(_audioManager.Initialize(_currentPlayer));
            
            // Initialize playlist manager
            _playlistManager = new PlaylistManager();
            yield return MelonCoroutines.Start(_playlistManager.Initialize());
            
            // Subscribe to events
            if (_headphoneManager != null)
                _headphoneManager.OnHeadphonesStateChanged += (attached) => OnHeadphonesStateChanged?.Invoke(attached);
                
            if (_audioManager != null)
            {
                _audioManager.OnTrackChanged += (track) => OnTrackChanged?.Invoke(track);
                _audioManager.OnPlayStateChanged += (playing) => OnPlayStateChanged?.Invoke(playing);
            }
            
            _isInitialized = true;
            NewLoggingSystem.Info("✓ BackSpeakerMainManager initialized successfully", "MainManager");
        }
        
        // Public API - delegates to appropriate managers
        public bool IsInitialized => _isInitialized;
        public bool AreHeadphonesAttached => _headphoneManager?.AreHeadphonesAttached ?? false;
        public NewSongDetails? GetCurrentTrack() => _audioManager?.GetCurrentTrack();
        public bool IsPlaying() => _audioManager?.IsPlaying() ?? false;
        
        // Audio control
        public void Play() => _audioManager?.Play();
        public void Pause() => _audioManager?.Pause();
        public void Stop() => _audioManager?.Stop();
        public void SetTrack(NewSongDetails track) => _audioManager?.SetTrack(track);
        public void SetVolume(float volume) => _audioManager?.SetVolume(volume);
        
        // Music sources
        public System.Collections.Generic.List<NewSongDetails> GetJukeboxTracks() => _playlistManager?.GetJukeboxTracks() ?? new System.Collections.Generic.List<NewSongDetails>();
        public System.Collections.Generic.List<NewSongDetails> GetLocalFolderTracks() => _playlistManager?.GetLocalFolderTracks() ?? new System.Collections.Generic.List<NewSongDetails>();
        public System.Collections.Generic.List<NewSongDetails> GetYouTubeTracks() => _playlistManager?.GetYouTubeTracks() ?? new System.Collections.Generic.List<NewSongDetails>();
        
        // Playlist management
        public System.Collections.Generic.List<string> GetPlaylistNames(string source) => _playlistManager?.GetPlaylistNames(source) ?? new System.Collections.Generic.List<string>();
        public System.Collections.Generic.List<NewSongDetails> GetPlaylistTracks(string playlistName, string source) => _playlistManager?.GetPlaylistTracks(playlistName, source) ?? new System.Collections.Generic.List<NewSongDetails>();
        public bool CreatePlaylist(string playlistName, string source) => _playlistManager?.CreatePlaylist(playlistName, source) ?? false;
        public bool DeletePlaylist(string playlistName, string source) => _playlistManager?.DeletePlaylist(playlistName, source) ?? false;
        
        public void Shutdown()
        {
            NewLoggingSystem.Info("Shutting down BackSpeakerMainManager", "MainManager");
            
            // Shutdown managers
            _audioManager?.Shutdown();
            _headphoneManager?.Shutdown();
            _playlistManager?.Shutdown();
            
            // Clear references
            _currentPlayer = null;
            _isInitialized = false;
            
            NewLoggingSystem.Info("✓ BackSpeakerMainManager shutdown complete", "MainManager");
        }
    }
} 