using System;
using System.Collections.Generic;
using BackSpeakerMod.NewBackend.Utils;

namespace BackSpeakerMod.NewFrontend.UI.Interfaces
{
    /// <summary>
    /// Interface for music backend integration
    /// Provides abstraction between UI and backend systems
    /// </summary>
    public interface IMusicBackend
    {
        #region Playback Control
        
        /// <summary>
        /// Start playing a track
        /// </summary>
        bool PlayTrack(NewSongDetails track);
        
        /// <summary>
        /// Pause current playback
        /// </summary>
        bool PausePlayback();
        
        /// <summary>
        /// Resume current playback
        /// </summary>
        bool ResumePlayback();
        
        /// <summary>
        /// Stop current playback
        /// </summary>
        bool StopPlayback();
        
        /// <summary>
        /// Skip to next track in queue
        /// </summary>
        bool NextTrack();
        
        /// <summary>
        /// Go to previous track in queue
        /// </summary>
        bool PreviousTrack();
        
        /// <summary>
        /// Set playback volume (0.0 to 1.0)
        /// </summary>
        bool SetVolume(float volume);
        
        /// <summary>
        /// Seek to position in current track (0.0 to 1.0)
        /// </summary>
        bool SeekTo(float position);
        
        #endregion
        
        #region Playback State
        
        /// <summary>
        /// Check if music is currently playing
        /// </summary>
        bool IsPlaying { get; }
        
        /// <summary>
        /// Get current playback volume
        /// </summary>
        float Volume { get; }
        
        /// <summary>
        /// Get current track
        /// </summary>
        NewSongDetails CurrentTrack { get; }
        
        /// <summary>
        /// Get current playback position in seconds
        /// </summary>
        float CurrentPosition { get; }
        
        /// <summary>
        /// Get total duration of current track in seconds
        /// </summary>
        float TotalDuration { get; }
        
        #endregion
        
        #region Headphone Control
        
        /// <summary>
        /// Enable headphones
        /// </summary>
        bool EnableHeadphones();
        
        /// <summary>
        /// Disable headphones
        /// </summary>
        bool DisableHeadphones();
        
        /// <summary>
        /// Check if headphones are enabled
        /// </summary>
        bool HeadphonesEnabled { get; }
        
        #endregion
        
        #region Music Sources
        
        /// <summary>
        /// Get available jukebox tracks
        /// </summary>
        List<NewSongDetails> GetJukeboxTracks();
        
        /// <summary>
        /// Get available local music tracks
        /// </summary>
        List<NewSongDetails> GetLocalTracks();
        
        /// <summary>
        /// Search YouTube for tracks
        /// </summary>
        List<NewSongDetails> SearchYouTube(string query);
        
        /// <summary>
        /// Get tracks from YouTube playlist URL
        /// </summary>
        List<NewSongDetails> GetYouTubePlaylist(string playlistUrl);
        
        #endregion
        
        #region Queue Management
        
        /// <summary>
        /// Get current queue
        /// </summary>
        List<NewSongDetails> GetCurrentQueue();
        
        /// <summary>
        /// Set current queue
        /// </summary>
        bool SetQueue(List<NewSongDetails> tracks);
        
        /// <summary>
        /// Add track to queue
        /// </summary>
        bool AddToQueue(NewSongDetails track);
        
        /// <summary>
        /// Remove track from queue
        /// </summary>
        bool RemoveFromQueue(int index);
        
        /// <summary>
        /// Clear queue
        /// </summary>
        bool ClearQueue();
        
        /// <summary>
        /// Shuffle queue
        /// </summary>
        bool ShuffleQueue();
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Fired when a track starts playing
        /// </summary>
        event Action<NewSongDetails> OnTrackStarted;
        
        /// <summary>
        /// Fired when playback is paused
        /// </summary>
        event Action OnPlaybackPaused;
        
        /// <summary>
        /// Fired when playback is resumed
        /// </summary>
        event Action OnPlaybackResumed;
        
        /// <summary>
        /// Fired when a track ends
        /// </summary>
        event Action<NewSongDetails> OnTrackEnded;
        
        /// <summary>
        /// Fired when playback position changes
        /// </summary>
        event Action<float, float> OnPlaybackProgress; // currentTime, totalTime
        
        /// <summary>
        /// Fired when volume changes
        /// </summary>
        event Action<float> OnVolumeChanged;
        
        /// <summary>
        /// Fired when headphone state changes
        /// </summary>
        event Action<bool> OnHeadphoneStateChanged;
        
        /// <summary>
        /// Fired when queue changes
        /// </summary>
        event Action<List<NewSongDetails>> OnQueueChanged;
        
        #endregion
    }
    
    /// <summary>
    /// Mock implementation of IMusicBackend for UI testing
    /// </summary>
    public class MockMusicBackend : IMusicBackend
    {
        #region Private Fields
        
        private bool isPlaying = false;
        private float volume = 0.7f;
        private bool headphonesEnabled = false;
        private NewSongDetails currentTrack = null;
        private float currentPosition = 0f;
        private List<NewSongDetails> currentQueue = new List<NewSongDetails>();
        
        #endregion
        
        #region Playback Control
        
        public bool PlayTrack(NewSongDetails track)
        {
            if (!headphonesEnabled)
            {
                NewLoggingSystem.Warning("Cannot play track - headphones not enabled", "MockBackend");
                return false;
            }
            
            currentTrack = track;
            isPlaying = true;
            currentPosition = 0f;
            
            OnTrackStarted?.Invoke(track);
            NewLoggingSystem.Info($"Mock: Playing track {track.title}", "MockBackend");
            return true;
        }
        
        public bool PausePlayback()
        {
            if (isPlaying)
            {
                isPlaying = false;
                OnPlaybackPaused?.Invoke();
                NewLoggingSystem.Info("Mock: Playback paused", "MockBackend");
                return true;
            }
            return false;
        }
        
        public bool ResumePlayback()
        {
            if (!isPlaying && currentTrack != null && headphonesEnabled)
            {
                isPlaying = true;
                OnPlaybackResumed?.Invoke();
                NewLoggingSystem.Info("Mock: Playback resumed", "MockBackend");
                return true;
            }
            return false;
        }
        
        public bool StopPlayback()
        {
            if (currentTrack != null)
            {
                var track = currentTrack;
                isPlaying = false;
                currentTrack = null;
                currentPosition = 0f;
                OnTrackEnded?.Invoke(track);
                NewLoggingSystem.Info("Mock: Playback stopped", "MockBackend");
                return true;
            }
            return false;
        }
        
        public bool NextTrack()
        {
            NewLoggingSystem.Info("Mock: Next track", "MockBackend");
            // TODO: Implement queue navigation
            return true;
        }
        
        public bool PreviousTrack()
        {
            NewLoggingSystem.Info("Mock: Previous track", "MockBackend");
            // TODO: Implement queue navigation
            return true;
        }
        
        public bool SetVolume(float newVolume)
        {
            volume = UnityEngine.Mathf.Clamp01(newVolume);
            OnVolumeChanged?.Invoke(volume);
            NewLoggingSystem.Info($"Mock: Volume set to {volume:F2}", "MockBackend");
            return true;
        }
        
        public bool SeekTo(float position)
        {
            if (currentTrack != null)
            {
                currentPosition = UnityEngine.Mathf.Clamp(position * currentTrack.duration, 0f, currentTrack.duration);
                NewLoggingSystem.Info($"Mock: Seeked to {currentPosition:F2}s", "MockBackend");
                return true;
            }
            return false;
        }
        
        #endregion
        
        #region Playback State
        
        public bool IsPlaying => isPlaying;
        public float Volume => volume;
        public NewSongDetails CurrentTrack => currentTrack;
        public float CurrentPosition => currentPosition;
        public float TotalDuration => currentTrack?.duration ?? 0f;
        
        #endregion
        
        #region Headphone Control
        
        public bool EnableHeadphones()
        {
            headphonesEnabled = true;
            OnHeadphoneStateChanged?.Invoke(true);
            NewLoggingSystem.Info("Mock: Headphones enabled", "MockBackend");
            return true;
        }
        
        public bool DisableHeadphones()
        {
            headphonesEnabled = false;
            if (isPlaying)
            {
                PausePlayback();
            }
            OnHeadphoneStateChanged?.Invoke(false);
            NewLoggingSystem.Info("Mock: Headphones disabled", "MockBackend");
            return true;
        }
        
        public bool HeadphonesEnabled => headphonesEnabled;
        
        #endregion
        
        #region Music Sources
        
        public List<NewSongDetails> GetJukeboxTracks()
        {
            return new List<NewSongDetails>
            {
                new NewSongDetails { title = "Lobby Music", artist = "Schedule I Jukebox", url = "jukebox://lobby", duration = 180, source = "jukebox" },
                new NewSongDetails { title = "Main Theme", artist = "Schedule I Jukebox", url = "jukebox://main", duration = 240, source = "jukebox" },
                new NewSongDetails { title = "Background Ambient", artist = "Schedule I Jukebox", url = "jukebox://ambient", duration = 300, source = "jukebox" }
            };
        }
        
        public List<NewSongDetails> GetLocalTracks()
        {
            return new List<NewSongDetails>
            {
                new NewSongDetails { title = "Local Song 1", artist = "Local Artist", url = "file://song1.mp3", duration = 210, source = "local" },
                new NewSongDetails { title = "Local Song 2", artist = "Local Artist", url = "file://song2.mp3", duration = 195, source = "local" }
            };
        }
        
        public List<NewSongDetails> SearchYouTube(string query)
        {
            return new List<NewSongDetails>
            {
                new NewSongDetails { title = $"YouTube Result for '{query}'", artist = "YouTube Artist", url = "youtube://result1", duration = 200, source = "youtube" }
            };
        }
        
        public List<NewSongDetails> GetYouTubePlaylist(string playlistUrl)
        {
            return new List<NewSongDetails>
            {
                new NewSongDetails { title = "Playlist Song 1", artist = "YouTube Artist", url = "youtube://playlist1", duration = 180, source = "youtube" },
                new NewSongDetails { title = "Playlist Song 2", artist = "YouTube Artist", url = "youtube://playlist2", duration = 220, source = "youtube" }
            };
        }
        
        #endregion
        
        #region Queue Management
        
        public List<NewSongDetails> GetCurrentQueue() => new List<NewSongDetails>(currentQueue);
        
        public bool SetQueue(List<NewSongDetails> tracks)
        {
            currentQueue = new List<NewSongDetails>(tracks);
            OnQueueChanged?.Invoke(GetCurrentQueue());
            NewLoggingSystem.Info($"Mock: Queue set with {tracks.Count} tracks", "MockBackend");
            return true;
        }
        
        public bool AddToQueue(NewSongDetails track)
        {
            currentQueue.Add(track);
            OnQueueChanged?.Invoke(GetCurrentQueue());
            NewLoggingSystem.Info($"Mock: Added {track.title} to queue", "MockBackend");
            return true;
        }
        
        public bool RemoveFromQueue(int index)
        {
            if (index >= 0 && index < currentQueue.Count)
            {
                var track = currentQueue[index];
                currentQueue.RemoveAt(index);
                OnQueueChanged?.Invoke(GetCurrentQueue());
                NewLoggingSystem.Info($"Mock: Removed {track.title} from queue", "MockBackend");
                return true;
            }
            return false;
        }
        
        public bool ClearQueue()
        {
            currentQueue.Clear();
            OnQueueChanged?.Invoke(GetCurrentQueue());
            NewLoggingSystem.Info("Mock: Queue cleared", "MockBackend");
            return true;
        }
        
        public bool ShuffleQueue()
        {
            // Simple shuffle
            for (int i = 0; i < currentQueue.Count; i++)
            {
                var temp = currentQueue[i];
                var randomIndex = UnityEngine.Random.Range(i, currentQueue.Count);
                currentQueue[i] = currentQueue[randomIndex];
                currentQueue[randomIndex] = temp;
            }
            OnQueueChanged?.Invoke(GetCurrentQueue());
            NewLoggingSystem.Info("Mock: Queue shuffled", "MockBackend");
            return true;
        }
        
        #endregion
        
        #region Events
        
        public event Action<NewSongDetails> OnTrackStarted;
        public event Action OnPlaybackPaused;
        public event Action OnPlaybackResumed;
        public event Action<NewSongDetails> OnTrackEnded;
        public event Action<float, float> OnPlaybackProgress;
        public event Action<float> OnVolumeChanged;
        public event Action<bool> OnHeadphoneStateChanged;
        public event Action<List<NewSongDetails>> OnQueueChanged;
        
        #endregion
    }
} 