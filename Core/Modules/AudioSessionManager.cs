using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        private readonly YouTubeStreamingController youtubeStreamingController;
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
            youtubeStreamingController = new YouTubeStreamingController();
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
                youtubeStreamingController.Initialize(audioSource);
                
                // Wire up YouTube streaming events
                youtubeStreamingController.OnTrackChanged += () => OnTracksReloaded?.Invoke();
                youtubeStreamingController.OnPlaylistChanged += () => OnTracksReloaded?.Invoke();
                
                // Initialize external music providers now that we have a GameObject
                trackLoader.InitializeExternalProviders(audioSource.gameObject);
                
                // Initialize cached songs for YouTube session after providers are ready
                InitializeYouTubeCachedSongs();
                
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
        /// Initialize cached songs for the YouTube session
        /// </summary>
        private void InitializeYouTubeCachedSongs()
        {
            try
            {
                var youtubeProvider = trackLoader.GetYouTubeProvider();
                var youtubeSession = sessions[MusicSourceType.YouTube];
                
                if (youtubeProvider != null && youtubeSession != null)
                {
                    youtubeSession.InitializeCachedSongs(youtubeProvider);
                    LoggingSystem.Info("YouTube session initialized with cached songs", "AudioSessionManager");
                }
                else
                {
                    LoggingSystem.Warning("Could not initialize YouTube cached songs - provider or session not available", "AudioSessionManager");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error initializing YouTube cached songs: {ex.Message}", "AudioSessionManager");
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
                
                // Handle YouTube vs regular audio differently
                if (globalPlayingSession == MusicSourceType.YouTube)
                {
                    youtubeStreamingController.Pause();
                    previousSession.Pause(youtubeStreamingController.CurrentTime);
                }
                else if (audioController.IsPlaying)
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
                // Skip loading tracks into YouTube sessions - they use playlist system instead
                if (currentSource == MusicSourceType.YouTube)
                {
                    LoggingSystem.Debug("Skipping track loading for YouTube session - uses playlist system", "AudioSessionManager");
                    
                    // But still trigger UI refresh for YouTube sessions to update the playlist display
                    if (currentSource == currentActiveSession)
                    {
                        OnTracksReloaded?.Invoke();
                        LoggingSystem.Debug("Triggered UI refresh for YouTube session after cached songs loaded", "AudioSessionManager");
                    }
                    return;
                }
                
                sessions[currentSource].LoadTracks(tracks, trackInfo);
                // LoggingSystem.Debug($"Loaded {tracks.Count} tracks into {currentSource} session", "AudioSessionManager");
                
                // If this is the active session, update the audio controller
                if (currentSource == currentActiveSession)
                {
                    audioController.SetTracks(tracks, trackInfo);
                    OnTracksReloaded?.Invoke();
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
                
                // Handle YouTube vs regular audio differently
                if (globalPlayingSession == MusicSourceType.YouTube)
                {
                    youtubeStreamingController.Pause();
                    previousSession.Pause(youtubeStreamingController.CurrentTime);
                }
                else if (audioController.IsPlaying)
                {
                    previousSession.Pause(audioController.CurrentTime);
                    audioController.Pause();
                }
            }
            
            // CRITICAL: Set this session as the global playing session IMMEDIATELY
            globalPlayingSession = sessionType;
            
            // If this session is not the currently active one, switch to it
            if (currentActiveSession != sessionType)
            {
                SetActiveSession(sessionType);
            }
            
            // Check if current track is a YouTube song (for YouTube sessions)
            bool isYouTubeSong = sessionType == MusicSourceType.YouTube && session.IsCurrentTrackYouTube();
            
            if (isYouTubeSong)
            {
                // Use YouTube streaming controller for YouTube songs
                var currentSong = session.GetCurrentYouTubeSong();
                if (currentSong != null)
                {
                    LoggingSystem.Info($"Playing YouTube song: {currentSong.title}", "AudioSessionManager");
                    
                    // Start streaming playback directly with the song
                    _ = StartYouTubeStream(currentSong, session);
                    
                    LoggingSystem.Info($"Starting YouTube stream for track {trackIndex + 1}/{session.TrackCount}", "AudioSessionManager");
                    return true;
                }
                else
                {
                    LoggingSystem.Warning("YouTube song details not found", "AudioSessionManager");
                    globalPlayingSession = null; // Clear on failure
                    return false;
                }
            }
            else
            {
                // Use regular audio controller for non-YouTube songs
                if (session.HasTracks)
                {
                    var allClips = session.GetAllClips();
                    var allTracks = session.GetAllTracks();
                    
                    if (allClips.Count > 0 && allTracks.Count > 0)
                    {
                        // For mixed playlists, we need to map the track index to audio clip index
                        var currentClip = session.GetCurrentClip();
                        if (currentClip != null)
                        {
                            // Find the audio clip index for this track
                            int audioClipIndex = allClips.IndexOf(currentClip);
                            if (audioClipIndex >= 0)
                            {
                                // Load entire playlist into audio controller
                                audioController.SetTracks(allClips, allTracks.Where((_, i) => session.GetAllClips().Count > i).ToList());
                                
                                // Apply this session's settings to audio controller
                                audioController.SetVolume(session.Volume);
                                audioController.RepeatMode = session.RepeatMode;
                                
                                // Play the specific audio clip
                                audioController.PlayTrack(audioClipIndex);
                                session.Resume();
                                
                                LoggingSystem.Info($"Playing regular track {trackIndex + 1}/{allClips.Count} from {sessionType} session", "AudioSessionManager");
                                return true;
                            }
                        }
                        else
                        {
                            LoggingSystem.Warning($"Current track has no audio clip (may be YouTube song in non-YouTube session)", "AudioSessionManager");
                            globalPlayingSession = null; // Clear on failure
                            return false;
                        }
                    }
                }
            }
            
            globalPlayingSession = null; // Clear on failure
            return false;
        }
        
        /// <summary>
        /// Helper method to start YouTube streaming asynchronously
        /// </summary>
        private async Task StartYouTubeStream(SongDetails songDetails, AudioSession session)
        {
            try
            {
                // Apply session settings to YouTube controller
                youtubeStreamingController.SetVolume(session.Volume);
                youtubeStreamingController.RepeatMode = session.RepeatMode;
                
                LoggingSystem.Info($"Starting YouTube stream for: {songDetails.title}", "AudioSessionManager");
                
                bool success = await youtubeStreamingController.PlaySong(songDetails);
                if (success)
                {
                    session.Resume();
                    LoggingSystem.Info($"YouTube stream started successfully for track {songDetails.title}", "AudioSessionManager");
                }
                else
                {
                    LoggingSystem.Warning($"Failed to start YouTube stream for track {songDetails.title}", "AudioSessionManager");
                    // Note: Don't clear globalPlayingSession here - let the caller handle failure
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error starting YouTube stream: {ex.Message}", "AudioSessionManager");
                // Note: Don't clear globalPlayingSession here - let the caller handle failure
            }
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
        public bool IsPlaying => globalPlayingSession.HasValue && 
            (globalPlayingSession == MusicSourceType.YouTube ? youtubeStreamingController.IsPlaying : audioController.IsPlaying);
        
        /// <summary>
        /// Check if the active session is the one that's playing
        /// </summary>
        public bool IsActiveSessionPlaying => globalPlayingSession == currentActiveSession && 
            (globalPlayingSession == MusicSourceType.YouTube ? youtubeStreamingController.IsPlaying : audioController.IsPlaying);
        
        /// <summary>
        /// Get current music source that's actually playing (not just active in UI)
        /// </summary>
        public MusicSourceType GetCurrentMusicSource() => globalPlayingSession ?? currentActiveSession;
        
        // Delegate methods for active session
        public int GetTrackCount() => GetActiveSession().TrackCount;
        public string GetCurrentTrackInfo() 
        {
            if (globalPlayingSession.HasValue)
            {
                if (globalPlayingSession == MusicSourceType.YouTube && youtubeStreamingController.IsPlaying)
                {
                    return youtubeStreamingController.GetCurrentTrackInfo();
                }
                else if (audioController.IsPlaying)
                {
                    return audioController.GetCurrentTrackInfo();
                }
            }
            // Otherwise get from active session
            return GetActiveSession().GetCurrentTrackInfo();
        }
        
        public string GetCurrentArtistInfo() 
        {
            if (globalPlayingSession.HasValue)
            {
                if (globalPlayingSession == MusicSourceType.YouTube && youtubeStreamingController.IsPlaying)
                {
                    return youtubeStreamingController.GetCurrentArtistInfo();
                }
                else if (audioController.IsPlaying)
                {
                    return audioController.GetCurrentArtistInfo();
                }
            }
            // Otherwise get from active session
            return GetActiveSession().GetCurrentArtistInfo();
        }
        
        public List<(string title, string artist)> GetAllTracks() => GetActiveSession().GetAllTracks();
        public int CurrentTrackIndex 
        {
            get
            {
                if (globalPlayingSession.HasValue)
                {
                    if (globalPlayingSession == MusicSourceType.YouTube)
                    {
                        // For YouTube, get the track index from the session
                        return sessions[globalPlayingSession.Value].CurrentTrackIndex;
                    }
                    else if (audioController.IsPlaying)
                    {
                        return audioController.CurrentTrackIndex;
                    }
                }
                // Otherwise get from active session
                return GetActiveSession().CurrentTrackIndex;
            }
        }
        public float CurrentTime 
        { 
            get 
            {
                if (globalPlayingSession.HasValue)
                {
                    if (globalPlayingSession == MusicSourceType.YouTube && youtubeStreamingController.IsPlaying)
                    {
                        return youtubeStreamingController.CurrentTime;
                    }
                    else if (audioController.IsPlaying)
                    {
                        return audioController.CurrentTime;
                    }
                }
                // Otherwise get saved progress from active session
                return GetActiveSession().SavedProgress;
            }
        }
        public float TotalTime 
        { 
            get 
            {
                if (globalPlayingSession.HasValue)
                {
                    if (globalPlayingSession == MusicSourceType.YouTube && youtubeStreamingController.IsPlaying)
                    {
                        return youtubeStreamingController.TotalTime;
                    }
                    else if (audioController.IsPlaying)
                    {
                        return audioController.TotalTime;
                    }
                }
                return 0f;
            }
        }
        public float Progress 
        { 
            get 
            {
                if (globalPlayingSession.HasValue)
                {
                    if (globalPlayingSession == MusicSourceType.YouTube && youtubeStreamingController.IsPlaying)
                    {
                        return youtubeStreamingController.Progress;
                    }
                    else if (audioController.IsPlaying)
                    {
                        return audioController.Progress;
                    }
                }
                return 0f;
            }
        }
        public float CurrentVolume => GetActiveSession().Volume;
        public bool IsAudioReady() => globalPlayingSession == MusicSourceType.YouTube ? 
            youtubeStreamingController.IsAudioReady : audioController.IsAudioReady();
        
        // Audio control methods - now session-specific
        public void Play()
        {
            // If there's a global playing session, resume it
            if (globalPlayingSession.HasValue)
            {
                if (globalPlayingSession == MusicSourceType.YouTube)
                {
                    youtubeStreamingController.Play();
                    sessions[globalPlayingSession.Value].Resume();
                }
                else
                {
                    audioController.Play();
                    sessions[globalPlayingSession.Value].Resume();
                }
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
            if (globalPlayingSession.HasValue)
            {
                if (globalPlayingSession == MusicSourceType.YouTube && youtubeStreamingController.IsPlaying)
                {
                    var currentTime = youtubeStreamingController.CurrentTime;
                    youtubeStreamingController.Pause();
                    sessions[globalPlayingSession.Value].Pause(currentTime);
                }
                else if (audioController.IsPlaying)
                {
                    sessions[globalPlayingSession.Value].Pause(audioController.CurrentTime);
                    audioController.Pause();
                }
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
                
                if (globalPlayingSession == MusicSourceType.YouTube)
                {
                    // Handle YouTube streaming asynchronously
                    HandleYouTubeNextTrack(session);
                }
                else if (session.NextTrack())
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
                
                if (globalPlayingSession == MusicSourceType.YouTube)
                {
                    // Handle YouTube streaming asynchronously
                    HandleYouTubePreviousTrack(session);
                }
                else if (session.PreviousTrack())
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
        
        /// <summary>
        /// Helper method to handle YouTube next track
        /// </summary>
        private void HandleYouTubeNextTrack(AudioSession session)
        {
            try
            {
                // Use the session's NextTrack method to get the next song
                bool hasNext = session.NextTrack();
                if (hasNext)
                {
                    var nextSong = session.GetCurrentYouTubeSong();
                    if (nextSong != null)
                    {
                        // Start playing the next song
                        _ = StartYouTubeStream(nextSong, session);
                        LoggingSystem.Debug("YouTube next track completed successfully", "AudioSessionManager");
                    }
                    else
                    {
                        LoggingSystem.Warning("Next YouTube song is null", "AudioSessionManager");
                    }
                }
                else
                {
                    LoggingSystem.Warning("No next YouTube track available", "AudioSessionManager");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error handling YouTube next track: {ex.Message}", "AudioSessionManager");
            }
        }
        
        /// <summary>
        /// Helper method to handle YouTube previous track
        /// </summary>
        private void HandleYouTubePreviousTrack(AudioSession session)
        {
            try
            {
                // Use the session's PreviousTrack method to get the previous song
                bool hasPrevious = session.PreviousTrack();
                if (hasPrevious)
                {
                    var previousSong = session.GetCurrentYouTubeSong();
                    if (previousSong != null)
                    {
                        // Start playing the previous song
                        _ = StartYouTubeStream(previousSong, session);
                        LoggingSystem.Debug("YouTube previous track completed successfully", "AudioSessionManager");
                    }
                    else
                    {
                        LoggingSystem.Warning("Previous YouTube song is null", "AudioSessionManager");
                    }
                }
                else
                {
                    LoggingSystem.Warning("No previous YouTube track available", "AudioSessionManager");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error handling YouTube previous track: {ex.Message}", "AudioSessionManager");
            }
        }
        
        public void SetVolume(float volume)
        {
            // Save to active session (for UI display and future playback)
            GetActiveSession().SetVolume(volume);
            
            // Apply to audio controller based on what's currently playing
            if (globalPlayingSession.HasValue)
            {
                if (globalPlayingSession == MusicSourceType.YouTube)
                {
                    youtubeStreamingController.SetVolume(volume);
                    LoggingSystem.Debug($"Applied volume {volume:P0} to currently playing YouTube session: {globalPlayingSession}", "AudioSessionManager");
                }
                else
                {
                    audioController.SetVolume(volume);
                    LoggingSystem.Debug($"Applied volume {volume:P0} to currently playing session: {globalPlayingSession}", "AudioSessionManager");
                }
            }
            else
            {
                LoggingSystem.Debug($"Saved volume {volume:P0} to session {currentActiveSession} (not currently playing)", "AudioSessionManager");
            }
        }
        
        public void SeekToTime(float time)
        {
            if (globalPlayingSession.HasValue)
            {
                if (globalPlayingSession == MusicSourceType.YouTube)
                {
                    youtubeStreamingController.SeekToTime(time);
                    LoggingSystem.Debug($"Applied seek to time {time:F1}s to YouTube session", "AudioSessionManager");
                }
                else
                {
                    audioController.SeekToTime(time);
                    LoggingSystem.Debug($"Applied seek to time {time:F1}s to regular session", "AudioSessionManager");
                }
            }
            else
            {
                LoggingSystem.Debug($"No playing session - seek ignored", "AudioSessionManager");
            }
        }
        
        public void SeekToProgress(float progress)
        {
            if (globalPlayingSession.HasValue)
            {
                if (globalPlayingSession == MusicSourceType.YouTube)
                {
                    youtubeStreamingController.SeekToProgress(progress);
                    LoggingSystem.Debug($"Applied seek to progress {progress:P0} to YouTube session", "AudioSessionManager");
                }
                else
                {
                    audioController.SeekToProgress(progress);
                    LoggingSystem.Debug($"Applied seek to progress {progress:P0} to regular session", "AudioSessionManager");
                }
            }
            else
            {
                LoggingSystem.Debug($"No playing session - seek ignored", "AudioSessionManager");
            }
        }
        
        public RepeatMode RepeatMode
        {
            get => GetActiveSession().RepeatMode;
            set 
            { 
                // Save to active session (for UI display and future playback)
                GetActiveSession().SetRepeatMode(value);
                
                // Only apply to controller if the active session is currently playing
                if (globalPlayingSession.HasValue && globalPlayingSession == currentActiveSession)
                {
                    if (globalPlayingSession == MusicSourceType.YouTube)
                    {
                        youtubeStreamingController.RepeatMode = value;
                        LoggingSystem.Debug($"Applied repeat mode {value} to currently playing YouTube session: {currentActiveSession}", "AudioSessionManager");
                    }
                    else
                    {
                        audioController.RepeatMode = value;
                        LoggingSystem.Debug($"Applied repeat mode {value} to currently playing session: {currentActiveSession}", "AudioSessionManager");
                    }
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
            
            // Update regular audio controller
            audioController.Update();
            
            // Update YouTube streaming controller
            youtubeStreamingController.Update();
        }
        
        public void Reset()
        {
            audioController.Reset();
            youtubeStreamingController.Reset();
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
                
                if (added)
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
                
                // Check if the song being removed is currently playing
                bool wasCurrentlyPlayingSong = false;
                if (globalPlayingSession == MusicSourceType.YouTube && session.IsCurrentTrackYouTube())
                {
                    var currentSong = session.GetCurrentYouTubeSong();
                    if (currentSong != null && currentSong.url == url)
                    {
                        wasCurrentlyPlayingSong = true;
                        LoggingSystem.Info($"Stopping playback of song being removed: {currentSong.title}", "AudioSessionManager");
                        
                        // Stop the YouTube streaming controller
                        youtubeStreamingController.Stop();
                        session.Stop();
                        globalPlayingSession = null; // Clear playing session
                    }
                }
                
                bool removed = session.RemoveYouTubeSong(url);
                
                if (removed)
                {
                    // If the playlist is now empty and something was playing, make sure everything is stopped
                    if (!session.HasTracks && globalPlayingSession == MusicSourceType.YouTube)
                    {
                        LoggingSystem.Info("YouTube playlist is now empty - stopping all playback", "AudioSessionManager");
                        youtubeStreamingController.Stop();
                        globalPlayingSession = null;
                    }
                    
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
    }
} 