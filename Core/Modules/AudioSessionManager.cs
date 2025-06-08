using System;
using System.Collections.Generic;
using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.Core.Modules
{
    /// <summary>
    /// Manages multiple audio sessions (one per music source)
    /// Coordinates between sessions so only one can play at a time
    /// </summary>
    public class AudioSessionManager
    {
        private readonly Dictionary<MusicSourceType, AudioSession> sessions;
        private readonly AudioController audioController;
        private readonly TrackLoader trackLoader;
        
        private MusicSourceType currentActiveSession = MusicSourceType.Jukebox;
        private MusicSourceType? globalPlayingSession = null; // Which session is actually playing audio
        private bool isInitialized = false;
        
        // Events
        public event Action? OnTracksReloaded;
        public event Action<MusicSourceType>? OnActiveSessionChanged;
        
        public AudioSessionManager()
        {
            sessions = new Dictionary<MusicSourceType, AudioSession>();
            audioController = new AudioController();
            trackLoader = new TrackLoader();
            
            // Create sessions for each music source
            foreach (MusicSourceType sourceType in Enum.GetValues<MusicSourceType>())
            {
                sessions[sourceType] = new AudioSession(sourceType);
            }
            
            // Wire up track loader events
            trackLoader.OnTracksLoaded += OnTracksLoaded;
            audioController.OnTracksChanged += () => OnTracksReloaded?.Invoke();
            
            LoggingSystem.Info("AudioSessionManager initialized with individual sessions", "AudioSessionManager");
        }
        
        /// <summary>
        /// Initialize the audio system with an audio source
        /// </summary>
        public bool Initialize(AudioSource audioSource)
        {
            try
            {
                audioController.Initialize(audioSource);
                
                // Initialize external music providers now that we have a GameObject
                trackLoader.InitializeExternalProviders(audioSource.gameObject);
                
                isInitialized = true;
                
                LoggingSystem.Info("AudioSessionManager initialized successfully", "AudioSessionManager");
                return true;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"AudioSessionManager initialization failed: {ex.Message}", "AudioSessionManager");
                return false;
            }
        }
        
        /// <summary>
        /// Set which session is currently being viewed in the UI
        /// This should stop current playback from other sessions when switching tabs
        /// </summary>
        public void SetActiveSession(MusicSourceType sessionType)
        {
            if (currentActiveSession == sessionType) return;
            
            LoggingSystem.Info($"Switching UI view from {currentActiveSession} to {sessionType}", "AudioSessionManager");
            
            // STOP CURRENT PLAYBACK when switching tabs
            if (globalPlayingSession.HasValue && globalPlayingSession != sessionType)
            {
                LoggingSystem.Debug($"Stopping playback from {globalPlayingSession} session due to tab switch", "AudioSessionManager");
                var previousSession = sessions[globalPlayingSession.Value];
                if (audioController.IsPlaying)
                {
                    previousSession.Pause(audioController.CurrentTime);
                    audioController.Pause();
                }
                globalPlayingSession = null; // Clear the playing session
            }
            
            currentActiveSession = sessionType;
            
            // Auto-load tracks if this session is empty (first time access)
            var session = sessions[sessionType];
            if (!session.HasTracks && isInitialized)
            {
                LoggingSystem.Info($"Auto-loading tracks for first-time access to {session.DisplayName}", "AudioSessionManager");
                LoadTracksForSession(sessionType);
            }
            
            OnActiveSessionChanged?.Invoke(sessionType);
        }
        
        /// <summary>
        /// Load tracks for a specific session
        /// </summary>
        public void LoadTracksForSession(MusicSourceType sessionType)
        {
            if (!isInitialized)
            {
                LoggingSystem.Warning("AudioSessionManager not initialized", "AudioSessionManager");
                return;
            }
            
            LoggingSystem.Info($"Loading tracks for session: {sessionType}", "AudioSessionManager");
            trackLoader.SetMusicSource(sessionType);
            trackLoader.LoadTracksFromCurrentSource();
        }
        
        /// <summary>
        /// Add a single track to a specific session
        /// </summary>
        public void AddTrackToSession(MusicSourceType sessionType, AudioClip audioClip, string title, string artist)
        {
            if (!isInitialized)
            {
                LoggingSystem.Warning("AudioSessionManager not initialized", "AudioSessionManager");
                return;
            }
            
            if (sessions.ContainsKey(sessionType))
            {
                sessions[sessionType].AddTrack(audioClip, title, artist);
                
                // If this is the active session, update the audio controller
                if (sessionType == currentActiveSession)
                {
                    var session = sessions[sessionType];
                    var allClips = session.GetAllClips();
                    var allTracks = session.GetAllTracks();
                    audioController.SetTracks(allClips, allTracks);
                    
                    // Trigger tracks reloaded event
                    OnTracksReloaded?.Invoke();
                }
            }
        }
        
        /// <summary>
        /// Add multiple tracks to a specific session
        /// </summary>
        public void AddTracksToSession(MusicSourceType sessionType, List<AudioClip> audioClips, List<(string title, string artist)> trackInfo)
        {
            if (!isInitialized)
            {
                LoggingSystem.Warning("AudioSessionManager not initialized", "AudioSessionManager");
                return;
            }
            
            if (sessions.ContainsKey(sessionType))
            {
                sessions[sessionType].AddTracks(audioClips, trackInfo);
                
                // If this is the active session, update the audio controller
                if (sessionType == currentActiveSession)
                {
                    var session = sessions[sessionType];
                    var allClips = session.GetAllClips();
                    var allTracks = session.GetAllTracks();
                    audioController.SetTracks(allClips, allTracks);
                    
                    // Trigger tracks reloaded event
                    OnTracksReloaded?.Invoke();
                }
            }
        }
        
        /// <summary>
        /// Force load tracks for a specific session, bypassing availability checks
        /// Useful for providers that need initialization (like LocalFolder)
        /// </summary>
        public void ForceLoadTracksForSession(MusicSourceType sessionType)
        {
            if (!isInitialized)
            {
                LoggingSystem.Warning("AudioSessionManager not initialized", "AudioSessionManager");
                return;
            }
            
            LoggingSystem.Info($"Force loading tracks for session: {sessionType}", "AudioSessionManager");
            trackLoader.ForceLoadFromSource(sessionType);
        }
        
        /// <summary>
        /// Handle tracks loaded from TrackLoader
        /// </summary>
        private void OnTracksLoaded(List<AudioClip> tracks, List<(string title, string artist)> trackInfo)
        {
            var currentSource = trackLoader.GetCurrentSourceType();
            if (sessions.ContainsKey(currentSource))
            {
                sessions[currentSource].LoadTracks(tracks, trackInfo);
                // LoggingSystem.Debug($"Loaded {tracks.Count} tracks into {currentSource} session", "AudioSessionManager");
                
                // If this is the active session, update the audio controller
                if (currentSource == currentActiveSession)
                {
                    audioController.SetTracks(tracks, trackInfo);
                }
            }
        }
        
        /// <summary>
        /// Play a track in a specific session
        /// </summary>
        public bool PlayTrack(MusicSourceType sessionType, int trackIndex)
        {
            if (!sessions.ContainsKey(sessionType))
                return false;
                
            var session = sessions[sessionType];
            if (!session.PlayTrack(trackIndex))
                return false;
            
            // Stop any currently playing session
            if (globalPlayingSession.HasValue && globalPlayingSession != sessionType)
            {
                var previousSession = sessions[globalPlayingSession.Value];
                if (audioController.IsPlaying)
                {
                    previousSession.Pause(audioController.CurrentTime);
                    audioController.Pause();
                }
            }
            
            // Set this session as the global playing session
            globalPlayingSession = sessionType;
            
            // If this session is not the currently active one, switch to it
            if (currentActiveSession != sessionType)
            {
                SetActiveSession(sessionType);
            }
            
            // Load the entire session's playlist into audio controller for proper repeat functionality
            if (session.HasTracks)
            {
                var allClips = session.GetAllClips();
                var allTracks = session.GetAllTracks();
                
                if (allClips.Count > 0 && allTracks.Count > 0)
                {
                    // Load entire playlist into audio controller
                    audioController.SetTracks(allClips, allTracks);
                    
                    // Apply this session's settings to audio controller
                    audioController.SetVolume(session.Volume);
                    audioController.RepeatMode = session.RepeatMode;
                    
                    // Play the specific track index
                    audioController.PlayTrack(trackIndex);
                    session.Resume();
                    
                    LoggingSystem.Info($"Playing track {trackIndex + 1}/{allClips.Count} from {sessionType} session (Volume: {session.Volume:P0}, Repeat: {session.RepeatMode})", "AudioSessionManager");
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Get data for the currently active session (for UI display)
        /// </summary>
        public AudioSession GetActiveSession()
        {
            return sessions[currentActiveSession];
        }
        
        /// <summary>
        /// Get data for a specific session
        /// </summary>
        public AudioSession GetSession(MusicSourceType sessionType)
        {
            return sessions[sessionType];
        }
        
        /// <summary>
        /// Check if any session is currently playing
        /// </summary>
        public bool IsPlaying => globalPlayingSession.HasValue && audioController.IsPlaying;
        
        /// <summary>
        /// Check if the active session is the one that's playing
        /// </summary>
        public bool IsActiveSessionPlaying => globalPlayingSession == currentActiveSession && audioController.IsPlaying;
        
        /// <summary>
        /// Get current music source that's actually playing (not just active in UI)
        /// </summary>
        public MusicSourceType GetCurrentMusicSource() => globalPlayingSession ?? currentActiveSession;
        
        // Delegate methods for active session
        public int GetTrackCount() => GetActiveSession().TrackCount;
        public string GetCurrentTrackInfo() 
        {
            if (globalPlayingSession.HasValue && audioController.IsPlaying)
            {
                // When music is playing, get track info from AudioController (which has current track index)
                return audioController.GetCurrentTrackInfo();
            }
            // Otherwise get from active session
            return GetActiveSession().GetCurrentTrackInfo();
        }
        
        public string GetCurrentArtistInfo() 
        {
            if (globalPlayingSession.HasValue && audioController.IsPlaying)
            {
                // When music is playing, get artist info from AudioController (which has current track index)
                return audioController.GetCurrentArtistInfo();
            }
            // Otherwise get from active session
            return GetActiveSession().GetCurrentArtistInfo();
        }
        
        public List<(string title, string artist)> GetAllTracks() => GetActiveSession().GetAllTracks();
        public int CurrentTrackIndex 
        {
            get
            {
                if (globalPlayingSession.HasValue && audioController.IsPlaying)
                {
                    // When music is playing, get current index from AudioController
                    return audioController.CurrentTrackIndex;
                }
                // Otherwise get from active session
                return GetActiveSession().CurrentTrackIndex;
            }
        }
        public float CurrentTime => IsActiveSessionPlaying ? audioController.CurrentTime : GetActiveSession().SavedProgress;
        public float TotalTime => IsActiveSessionPlaying ? audioController.TotalTime : 0f;
        public float Progress => IsActiveSessionPlaying ? audioController.Progress : 0f;
        public float CurrentVolume => GetActiveSession().Volume;
        public bool IsAudioReady() => audioController.IsAudioReady();
        
        // Audio control methods - now session-specific
        public void Play()
        {
            // If there's a global playing session, resume it
            if (globalPlayingSession.HasValue)
            {
                audioController.Play();
                sessions[globalPlayingSession.Value].Resume();
            }
            // Otherwise, try to start playing from the active session if it has tracks
            else
            {
                var activeSession = GetActiveSession();
                if (activeSession.HasTracks)
                {
                    PlayTrack(currentActiveSession, activeSession.CurrentTrackIndex);
                }
                else
                {
                    LoggingSystem.Warning($"Cannot play - {activeSession.DisplayName} has no tracks", "AudioSessionManager");
                }
            }
        }
        
        public void Pause()
        {
            if (globalPlayingSession.HasValue && audioController.IsPlaying)
            {
                sessions[globalPlayingSession.Value].Pause(audioController.CurrentTime);
                audioController.Pause();
            }
        }
        
        public void TogglePlayPause()
        {
            if (IsPlaying)
                Pause();
            else
                Play();
        }
        
        public void NextTrack()
        {
            // Only work on the currently playing session
            if (globalPlayingSession.HasValue)
            {
                var session = sessions[globalPlayingSession.Value];
                if (session.NextTrack())
                {
                    PlayTrack(globalPlayingSession.Value, session.CurrentTrackIndex);
                }
            }
            // If nothing is playing, but the active session has tracks, start from there
            else
            {
                var activeSession = GetActiveSession();
                if (activeSession.HasTracks)
                {
                    activeSession.NextTrack();
                    PlayTrack(currentActiveSession, activeSession.CurrentTrackIndex);
                }
                else
                {
                    LoggingSystem.Warning($"Cannot go to next track - {activeSession.DisplayName} has no tracks", "AudioSessionManager");
                }
            }
        }
        
        public void PreviousTrack()
        {
            // Only work on the currently playing session
            if (globalPlayingSession.HasValue)
            {
                var session = sessions[globalPlayingSession.Value];
                if (session.PreviousTrack())
                {
                    PlayTrack(globalPlayingSession.Value, session.CurrentTrackIndex);
                }
            }
            // If nothing is playing, but the active session has tracks, start from there
            else
            {
                var activeSession = GetActiveSession();
                if (activeSession.HasTracks)
                {
                    activeSession.PreviousTrack();
                    PlayTrack(currentActiveSession, activeSession.CurrentTrackIndex);
                }
                else
                {
                    LoggingSystem.Warning($"Cannot go to previous track - {activeSession.DisplayName} has no tracks", "AudioSessionManager");
                }
            }
        }
        
        public void SetVolume(float volume)
        {
            // Save to active session (for UI display and future playback)
            GetActiveSession().SetVolume(volume);
            
            // Only apply to audio controller if the active session is currently playing
            if (globalPlayingSession.HasValue && globalPlayingSession == currentActiveSession)
            {
                audioController.SetVolume(volume);
                LoggingSystem.Debug($"Applied volume {volume:P0} to currently playing session: {currentActiveSession}", "AudioSessionManager");
            }
            else
            {
                LoggingSystem.Debug($"Saved volume {volume:P0} to session {currentActiveSession} (not currently playing)", "AudioSessionManager");
            }
        }
        
        public void SeekToTime(float time) => audioController.SeekToTime(time);
        public void SeekToProgress(float progress) => audioController.SeekToProgress(progress);
        
        public RepeatMode RepeatMode
        {
            get => GetActiveSession().RepeatMode;
            set 
            { 
                // Save to active session (for UI display and future playback)
                GetActiveSession().SetRepeatMode(value);
                
                // Only apply to audio controller if the active session is currently playing
                if (globalPlayingSession.HasValue && globalPlayingSession == currentActiveSession)
                {
                    audioController.RepeatMode = value;
                    LoggingSystem.Debug($"Applied repeat mode {value} to currently playing session: {currentActiveSession}", "AudioSessionManager");
                }
                else
                {
                    LoggingSystem.Debug($"Saved repeat mode {value} to session {currentActiveSession} (not currently playing)", "AudioSessionManager");
                }
            }
        }
        
        public void Update()
        {
            if (!isInitialized) return;
            audioController.Update();
        }
        
        public void Reset()
        {
            audioController.Reset();
            globalPlayingSession = null;
            
            // Reset all sessions
            foreach (var session in sessions.Values)
            {
                session.Stop();
            }
            
            isInitialized = false;
        }
        
        public void Shutdown()
        {
            LoggingSystem.Info("Shutting down AudioSessionManager", "AudioSessionManager");
            
            try
            {
                Pause();
                
                if (trackLoader != null)
                    trackLoader.OnTracksLoaded -= OnTracksLoaded;
                
                Reset();
                LoggingSystem.Info("AudioSessionManager shutdown completed", "AudioSessionManager");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"AudioSessionManager shutdown failed: {ex.Message}", "AudioSessionManager");
            }
        }
        
        public string GetStatus()
        {
            var activeSession = GetActiveSession();
            var playingStatus = globalPlayingSession.HasValue ? $"Playing: {sessions[globalPlayingSession.Value].DisplayName}" : "No active playback";
            return $"Active: {activeSession.DisplayName} | {playingStatus} | {activeSession.GetStatus()}";
        }
        
        /// <summary>
        /// Add a YouTube song to the YouTube session playlist
        /// </summary>
        public bool AddYouTubeSong(SongDetails songDetails)
        {
            if (!isInitialized)
            {
                LoggingSystem.Warning("AudioSessionManager not initialized", "AudioSessionManager");
                return false;
            }
            
            if (sessions.ContainsKey(MusicSourceType.YouTube))
            {
                var session = sessions[MusicSourceType.YouTube];
                bool added = session.AddYouTubeSong(songDetails);
                
                if (added && MusicSourceType.YouTube == currentActiveSession)
                {
                    // Update UI for YouTube session
                    OnTracksReloaded?.Invoke();
                }
                
                return added;
            }
            
            return false;
        }
        
        /// <summary>
        /// Remove a YouTube song from the YouTube session playlist
        /// </summary>
        public bool RemoveYouTubeSong(string url)
        {
            if (!isInitialized)
            {
                LoggingSystem.Warning("AudioSessionManager not initialized", "AudioSessionManager");
                return false;
            }
            
            if (sessions.ContainsKey(MusicSourceType.YouTube))
            {
                var session = sessions[MusicSourceType.YouTube];
                bool removed = session.RemoveYouTubeSong(url);
                
                if (removed && MusicSourceType.YouTube == currentActiveSession)
                {
                    // Update UI for YouTube session
                    OnTracksReloaded?.Invoke();
                }
                
                return removed;
            }
            
            return false;
        }
        
        /// <summary>
        /// Check if a YouTube song exists in the playlist
        /// </summary>
        public bool ContainsYouTubeSong(string url)
        {
            if (!isInitialized) return false;
            
            if (sessions.ContainsKey(MusicSourceType.YouTube))
            {
                return sessions[MusicSourceType.YouTube].ContainsYouTubeSong(url);
            }
            
            return false;
        }
        
        /// <summary>
        /// Get YouTube playlist from the session
        /// </summary>
        public YouTubePlaylist? GetYouTubePlaylist()
        {
            if (!isInitialized) return null;
            
            if (sessions.ContainsKey(MusicSourceType.YouTube))
            {
                return sessions[MusicSourceType.YouTube].YouTubePlaylist;
            }
            
            return null;
        }
    }
} 