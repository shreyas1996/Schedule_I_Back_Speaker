using System;
using System.Collections.Generic;
using System.Linq;
using BackSpeakerMod.NewBackend.Utils;

/// <summary>
/// Download status enumeration
/// </summary>
public enum DownloadStatus
{
    Pending,
    Downloading,
    Completed,
    Failed,
    Cancelled
}

/// <summary>
/// Download information class
/// </summary>
[Serializable]
public class DownloadInfo
{
    public string downloadId;
    public string songTitle;
    public string songUrl;
    public DownloadStatus status;
    public float progress; // 0.0 to 1.0
    public string filePath;
    public DateTime startTime;
    public DateTime endTime;
    public string errorMessage;
    public long fileSize;
    public long downloadedBytes;
    
    public DownloadInfo()
    {
        downloadId = System.Guid.NewGuid().ToString();
        songTitle = "";
        songUrl = "";
        filePath = "";
        errorMessage = "";
        status = DownloadStatus.Pending;
        progress = 0f;
        startTime = DateTime.Now;
    }
    
    public string GetProgressPercentage()
    {
        return $"{(progress * 100):F1}%";
    }
    
    public string GetStatusText()
    {
        switch (status)
        {
            case DownloadStatus.Pending: return "Waiting...";
            case DownloadStatus.Downloading: return $"Downloading {GetProgressPercentage()}";
            case DownloadStatus.Completed: return "Complete";
            case DownloadStatus.Failed: return "Failed";
            case DownloadStatus.Cancelled: return "Cancelled";
            default: return "Unknown";
        }
    }
}

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
        NewSongDetails? CurrentTrack { get; }
        
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
        
        #region Playlist Management
        
        /// <summary>
        /// Get all available playlists
        /// </summary>
        List<NewYouTubePlaylistInfo> GetAllPlaylists();
        
        /// <summary>
        /// Create a new playlist
        /// </summary>
        NewYouTubePlaylist CreatePlaylist(string name, string description = "");
        
        /// <summary>
        /// Save playlist changes
        /// </summary>
        bool SavePlaylist(NewYouTubePlaylist playlist);
        
        /// <summary>
        /// Delete a playlist
        /// </summary>
        bool DeletePlaylist(string playlistId);
        
        /// <summary>
        /// Load a specific playlist
        /// </summary>
        NewYouTubePlaylist LoadPlaylist(string playlistId);
        
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
        
        /// <summary>
        /// Play specific item from queue
        /// </summary>
        bool PlayQueueItem(int index);
        
        #endregion
        
        #region Download Management
        
        /// <summary>
        /// Download individual song
        /// </summary>
        bool DownloadSong(NewSongDetails song);
        
        /// <summary>
        /// Download entire playlist
        /// </summary>
        bool DownloadPlaylist(NewYouTubePlaylistInfo playlistInfo);
        
        /// <summary>
        /// Cancel download
        /// </summary>
        bool CancelDownload(string downloadId);
        
        /// <summary>
        /// Get all active downloads
        /// </summary>
        List<DownloadInfo> GetActiveDownloads();
        
        /// <summary>
        /// Get download progress for specific item
        /// </summary>
        DownloadInfo GetDownloadInfo(string downloadId);
        
        /// <summary>
        /// Clear completed downloads
        /// </summary>
        bool ClearCompletedDownloads();
        
        #endregion
        
        #region Playlist Management
        
        /// <summary>
        /// Create new playlist
        /// </summary>
        NewYouTubePlaylistInfo CreateNewPlaylist(string name, string description = "");
        
        /// <summary>
        /// Update playlist info (name, description)
        /// </summary>
        bool UpdatePlaylistInfo(string playlistId, string newName, string newDescription);
        
        /// <summary>
        /// Add song to specific playlist
        /// </summary>
        bool AddSongToPlaylist(string playlistId, NewSongDetails song);
        
        /// <summary>
        /// Remove song from specific playlist
        /// </summary>
        bool RemoveSongFromPlaylist(string playlistId, string songUrl);
        
        /// <summary>
        /// Get songs in playlist
        /// </summary>
        List<NewSongDetails> GetPlaylistSongs(string playlistId);
        
        /// <summary>
        /// Reorder songs in playlist
        /// </summary>
        bool ReorderPlaylistSongs(string playlistId, List<string> songUrls);
        
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
        private NewSongDetails? currentTrack = null;
        private float currentPosition = 0f;
        private List<NewSongDetails> currentQueue = new List<NewSongDetails>();
        
        // Download management
        private List<DownloadInfo> activeDownloads = new List<DownloadInfo>();
        private Dictionary<string, DownloadInfo> downloadLookup = new Dictionary<string, DownloadInfo>();
        
        // Playlist management
        private Dictionary<string, NewYouTubePlaylist> mockPlaylists = new Dictionary<string, NewYouTubePlaylist>();
        private bool playlistsInitialized = false;
        
        public MockMusicBackend()
        {
            InitializeMockDownloads();
        }
        
        private void InitializeMockDownloads()
        {
            // Create some sample downloads for demo purposes
            var sampleDownloads = new[]
            {
                new DownloadInfo
                {
                    songTitle = "Awesome Rock Song",
                    songUrl = "youtube://sample1",
                    status = DownloadStatus.Downloading,
                    progress = 0.65f,
                    fileSize = 5500000,
                    downloadedBytes = 3575000,
                    startTime = DateTime.Now.AddMinutes(-3)
                },
                new DownloadInfo
                {
                    songTitle = "Electronic Beats Mix",
                    songUrl = "youtube://sample2", 
                    status = DownloadStatus.Completed,
                    progress = 1.0f,
                    fileSize = 4200000,
                    downloadedBytes = 4200000,
                    startTime = DateTime.Now.AddMinutes(-10),
                    endTime = DateTime.Now.AddMinutes(-8),
                    filePath = "/Downloads/Electronic_Beats_Mix.mp3"
                },
                new DownloadInfo
                {
                    songTitle = "Classical Symphony",
                    songUrl = "youtube://sample3",
                    status = DownloadStatus.Pending,
                    progress = 0.0f,
                    fileSize = 7800000,
                    downloadedBytes = 0,
                    startTime = DateTime.Now.AddSeconds(-30)
                },
                new DownloadInfo
                {
                    songTitle = "Jazz Collection",
                    songUrl = "youtube://sample4",
                    status = DownloadStatus.Failed,
                    progress = 0.25f,
                    fileSize = 6100000,
                    downloadedBytes = 1525000,
                    startTime = DateTime.Now.AddMinutes(-5),
                    endTime = DateTime.Now.AddMinutes(-4),
                    errorMessage = "Network connection lost"
                },
                new DownloadInfo
                {
                    songTitle = "Hip Hop Hits",
                    songUrl = "youtube://sample5",
                    status = DownloadStatus.Downloading,
                    progress = 0.35f,
                    fileSize = 5900000,
                    downloadedBytes = 2065000,
                    startTime = DateTime.Now.AddMinutes(-2)
                }
            };
            
            foreach (var download in sampleDownloads)
            {
                activeDownloads.Add(download);
                downloadLookup[download.downloadId] = download;
            }
            
            NewLoggingSystem.Info($"Initialized MockMusicBackend with {sampleDownloads.Length} sample downloads", "MockBackend");
        }
        
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
        public NewSongDetails? CurrentTrack => currentTrack;
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
            // Enhanced mock jukebox tracks for better testing
            return new List<NewSongDetails>
            {
                new NewSongDetails { title = "Lobby Music", artist = "Schedule I Jukebox", url = "jukebox://lobby", duration = 180, source = "jukebox" },
                new NewSongDetails { title = "Blue Theme", artist = "Schedule I Jukebox", url = "jukebox://blue", duration = 240, source = "jukebox" },
                new NewSongDetails { title = "Background Ambient", artist = "Schedule I Jukebox", url = "jukebox://ambient", duration = 300, source = "jukebox" },
                new NewSongDetails { title = "Action Music", artist = "Schedule I Jukebox", url = "jukebox://action", duration = 195, source = "jukebox" },
                new NewSongDetails { title = "Elevator Jazz", artist = "Schedule I Jukebox", url = "jukebox://jazz", duration = 220, source = "jukebox" },
                new NewSongDetails { title = "Menu Sound", artist = "Schedule I Jukebox", url = "jukebox://menu", duration = 160, source = "jukebox" }
            };
        }
        
        public List<NewSongDetails> GetLocalTracks()
        {
            // Enhanced mock local tracks - simulating file browser with folders and files
            return new List<NewSongDetails>
            {
                new NewSongDetails { title = "My Favorites.mp3", artist = "Various Artists", url = "file://music/favorites/my_favorites.mp3", duration = 210, source = "local" },
                new NewSongDetails { title = "Rock Folder", artist = "Folder (15 files)", url = "folder://music/rock/", duration = 0, source = "local" },
                new NewSongDetails { title = "Jazz.mp3", artist = "Miles Davis", url = "file://music/jazz.mp3", duration = 195, source = "local" },
                new NewSongDetails { title = "Electronic", artist = "Folder (8 files)", url = "folder://music/electronic/", duration = 0, source = "local" },
                new NewSongDetails { title = "Classic.mp3", artist = "Beethoven", url = "file://music/classic.mp3", duration = 320, source = "local" },
                new NewSongDetails { title = "Chill Vibes.mp3", artist = "Lo-Fi Artist", url = "file://music/chill/chill_vibes.mp3", duration = 240, source = "local" }
            };
        }
        
        public List<NewSongDetails> SearchYouTube(string query)
        {
            // Enhanced mock YouTube search results
            if (string.IsNullOrWhiteSpace(query))
                return new List<NewSongDetails>();
                
            return new List<NewSongDetails>
            {
                new NewSongDetails { title = $"Amazing Song ({query})", artist = "Popular Artist", url = "https://youtube.com/watch?v=mock1", duration = 210, source = "youtube" },
                new NewSongDetails { title = $"Best Hit ({query})", artist = "Famous Band", url = "https://youtube.com/watch?v=mock2", duration = 195, source = "youtube" },
                new NewSongDetails { title = $"Classic Track ({query})", artist = "Legendary Singer", url = "https://youtube.com/watch?v=mock3", duration = 240, source = "youtube" },
                new NewSongDetails { title = $"New Release ({query})", artist = "Rising Star", url = "https://youtube.com/watch?v=mock4", duration = 180, source = "youtube" },
                new NewSongDetails { title = $"Popular Song ({query})", artist = "Top Artist", url = "https://youtube.com/watch?v=mock5", duration = 220, source = "youtube" },
                new NewSongDetails { title = $"Great Music ({query})", artist = "Awesome Musician", url = "https://youtube.com/watch?v=mock6", duration = 205, source = "youtube" }
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
        
        public bool PlayQueueItem(int index)
        {
            if (index >= 0 && index < currentQueue.Count)
            {
                var track = currentQueue[index];
                NewLoggingSystem.Info($"Mock: Playing queue item {index}: {track.title}", "MockBackend");
                
                // Set as current track and start playback
                currentTrack = track;
                isPlaying = true;
                OnTrackStarted?.Invoke(track);
                OnPlaybackResumed?.Invoke();
                
                return true;
            }
            
            NewLoggingSystem.Warning($"Mock: Invalid queue index {index}", "MockBackend");
            return false;
        }
        
        #endregion
        
        #region Download Management
        
        public bool DownloadSong(NewSongDetails song)
        {
            if (song == null || string.IsNullOrEmpty(song.url))
                return false;
                
            // Check if already downloading
            if (downloadLookup.Values.Any(d => d.songUrl == song.url && 
                (d.status == DownloadStatus.Pending || d.status == DownloadStatus.Downloading)))
            {
                NewLoggingSystem.Warning($"Song already downloading: {song.title}", "MockBackend");
                return false;
            }
            
            var downloadInfo = new DownloadInfo
            {
                songTitle = song.title,
                songUrl = song.url,
                status = DownloadStatus.Pending,
                fileSize = UnityEngine.Random.Range(3000000, 8000000) // 3-8 MB
            };
            
            activeDownloads.Add(downloadInfo);
            downloadLookup[downloadInfo.downloadId] = downloadInfo;
            
            // Start mock download simulation
            StartMockDownload(downloadInfo);
            
            NewLoggingSystem.Info($"Mock: Started downloading {song.title}", "MockBackend");
            return true;
        }
        
        public bool DownloadPlaylist(NewYouTubePlaylistInfo playlistInfo)
        {
            if (playlistInfo == null) return false;
            
            var playlist = LoadPlaylist(playlistInfo.id);
            if (playlist == null || playlist.songs.Count == 0)
            {
                NewLoggingSystem.Warning($"Playlist not found or empty: {playlistInfo.name}", "MockBackend");
                return false;
            }
            
            int startedDownloads = 0;
            foreach (var song in playlist.songs)
            {
                if (DownloadSong(song))
                    startedDownloads++;
            }
            
            NewLoggingSystem.Info($"Mock: Started downloading playlist '{playlistInfo.name}' - {startedDownloads}/{playlist.songs.Count} songs", "MockBackend");
            return startedDownloads > 0;
        }
        
        public bool CancelDownload(string downloadId)
        {
            if (downloadLookup.ContainsKey(downloadId))
            {
                var download = downloadLookup[downloadId];
                if (download.status == DownloadStatus.Pending || download.status == DownloadStatus.Downloading)
                {
                    download.status = DownloadStatus.Cancelled;
                    download.endTime = DateTime.Now;
                    NewLoggingSystem.Info($"Mock: Cancelled download {download.songTitle}", "MockBackend");
                    return true;
                }
            }
            return false;
        }
        
        public List<DownloadInfo> GetActiveDownloads()
        {
            // Return all downloads for the Downloads screen to show everything
            // In a real implementation, you might want separate methods for active vs all downloads
            return new List<DownloadInfo>(activeDownloads);
        }
        
        public DownloadInfo GetDownloadInfo(string downloadId)
        {
            return downloadLookup.ContainsKey(downloadId) ? downloadLookup[downloadId] : null!;
        }
        
        public bool ClearCompletedDownloads()
        {
            var completedIds = activeDownloads
                .Where(d => d.status == DownloadStatus.Completed || 
                           d.status == DownloadStatus.Failed || 
                           d.status == DownloadStatus.Cancelled)
                .Select(d => d.downloadId)
                .ToList();
                
            foreach (var id in completedIds)
            {
                var download = downloadLookup[id];
                activeDownloads.Remove(download);
                downloadLookup.Remove(id);
            }
            
            NewLoggingSystem.Info($"Mock: Cleared {completedIds.Count} completed downloads", "MockBackend");
            return true;
        }
        
        #endregion
        
        #region Playlist Management
        
        public List<NewYouTubePlaylistInfo> GetAllPlaylists()
        {
            EnsureMockPlaylistsInitialized();
            
            var playlistInfos = new List<NewYouTubePlaylistInfo>();
            foreach (var playlist in mockPlaylists.Values)
            {
                playlistInfos.Add(new NewYouTubePlaylistInfo
                {
                    id = playlist.id,
                    name = playlist.name,
                    description = playlist.description,
                    created = playlist.created,
                    lastModified = playlist.lastModified,
                    songCount = playlist.songs.Count
                });
            }
            
            return playlistInfos;
        }
        
        public NewYouTubePlaylist CreatePlaylist(string name, string description = "")
        {
            var playlist = new NewYouTubePlaylist(name)
            {
                description = description
            };
            NewLoggingSystem.Info($"Mock: Created playlist '{name}'", "MockBackend");
            return playlist;
        }
        
        public bool SavePlaylist(NewYouTubePlaylist playlist)
        {
            NewLoggingSystem.Info($"Mock: Saved playlist '{playlist.name}' with {playlist.songs.Count} songs", "MockBackend");
            return true;
        }
        
        public bool DeletePlaylist(string playlistId)
        {
            if (string.IsNullOrEmpty(playlistId))
                return false;
                
            if (mockPlaylists.ContainsKey(playlistId))
            {
                var playlistName = mockPlaylists[playlistId].name;
                mockPlaylists.Remove(playlistId);
                NewLoggingSystem.Info($"Mock: Deleted playlist '{playlistName}' (ID: {playlistId})", "MockBackend");
                return true;
            }
            
            NewLoggingSystem.Warning($"Mock: Playlist not found for deletion: {playlistId}", "MockBackend");
            return false;
        }
        
        public NewYouTubePlaylist LoadPlaylist(string playlistId)
        {
            if (string.IsNullOrEmpty(playlistId))
                return null!;
                
            EnsureMockPlaylistsInitialized();
            
            if (mockPlaylists.ContainsKey(playlistId))
            {
                NewLoggingSystem.Info($"Mock: Loaded playlist '{mockPlaylists[playlistId].name}' with {mockPlaylists[playlistId].songs.Count} songs", "MockBackend");
                return mockPlaylists[playlistId];
            }
            
            NewLoggingSystem.Warning($"Mock: Playlist not found: {playlistId}", "MockBackend");
            return null!;
        }
        
        public NewYouTubePlaylistInfo CreateNewPlaylist(string name, string description = "")
        {
            if (string.IsNullOrWhiteSpace(name))
                return null!;
                
            var playlist = new NewYouTubePlaylist(name.Trim())
            {
                description = description?.Trim() ?? ""
            };
            
            mockPlaylists[playlist.id] = playlist;
            
            var playlistInfo = new NewYouTubePlaylistInfo
            {
                id = playlist.id,
                name = playlist.name,
                description = playlist.description,
                created = playlist.created,
                lastModified = playlist.lastModified,
                songCount = 0
            };
            
            NewLoggingSystem.Info($"Mock: Created playlist '{name}' (ID: {playlist.id})", "MockBackend");
            return playlistInfo;
        }
        
        public bool UpdatePlaylistInfo(string playlistId, string newName, string newDescription)
        {
            if (string.IsNullOrEmpty(playlistId) || string.IsNullOrWhiteSpace(newName))
                return false;
                
            if (mockPlaylists.ContainsKey(playlistId))
            {
                var playlist = mockPlaylists[playlistId];
                playlist.name = newName.Trim();
                playlist.description = newDescription?.Trim() ?? "";
                playlist.lastModified = DateTime.Now;
                
                NewLoggingSystem.Info($"Mock: Updated playlist '{newName}' (ID: {playlistId})", "MockBackend");
                return true;
            }
            
            return false;
        }
        
        public bool AddSongToPlaylist(string playlistId, NewSongDetails song)
        {
            if (string.IsNullOrEmpty(playlistId) || song == null)
                return false;
                
            EnsureMockPlaylistsInitialized();
            
            if (mockPlaylists.ContainsKey(playlistId))
            {
                var playlist = mockPlaylists[playlistId];
                bool added = playlist.AddSong(song);
                
                if (added)
                {
                    NewLoggingSystem.Info($"Mock: Added '{song.title}' to playlist '{playlist.name}'", "MockBackend");
                }
                else
                {
                    NewLoggingSystem.Warning($"Mock: Song '{song.title}' already in playlist '{playlist.name}'", "MockBackend");
                }
                
                return added;
            }
            
            return false;
        }
        
        public bool RemoveSongFromPlaylist(string playlistId, string songUrl)
        {
            if (string.IsNullOrEmpty(playlistId) || string.IsNullOrEmpty(songUrl))
                return false;
                
            if (mockPlaylists.ContainsKey(playlistId))
            {
                var playlist = mockPlaylists[playlistId];
                bool removed = playlist.RemoveSong(songUrl);
                
                if (removed)
                {
                    NewLoggingSystem.Info($"Mock: Removed song from playlist '{playlist.name}'", "MockBackend");
                }
                
                return removed;
            }
            
            return false;
        }
        
        public List<NewSongDetails> GetPlaylistSongs(string playlistId)
        {
            if (string.IsNullOrEmpty(playlistId))
                return new List<NewSongDetails>();
                
            EnsureMockPlaylistsInitialized();
            
            if (mockPlaylists.ContainsKey(playlistId))
            {
                return new List<NewSongDetails>(mockPlaylists[playlistId].songs);
            }
            
            return new List<NewSongDetails>();
        }
        
        public bool ReorderPlaylistSongs(string playlistId, List<string> songUrls)
        {
            if (string.IsNullOrEmpty(playlistId) || songUrls == null || songUrls.Count == 0)
                return false;
                
            if (mockPlaylists.ContainsKey(playlistId))
            {
                var playlist = mockPlaylists[playlistId];
                var reorderedSongs = new List<NewSongDetails>();
                
                // Reorder according to the provided URL list
                foreach (var url in songUrls)
                {
                    var song = playlist.songs.FirstOrDefault(s => s.url == url);
                    if (song != null)
                    {
                        reorderedSongs.Add(song);
                    }
                }
                
                // Add any remaining songs not in the reorder list
                foreach (var song in playlist.songs)
                {
                    if (!reorderedSongs.Any(s => s.url == song.url))
                    {
                        reorderedSongs.Add(song);
                    }
                }
                
                playlist.songs = reorderedSongs;
                playlist.lastModified = DateTime.Now;
                
                NewLoggingSystem.Info($"Mock: Reordered songs in playlist '{playlist.name}'", "MockBackend");
                return true;
            }
            
            return false;
        }
        
        #endregion
        
        #region Helper Methods
        
        private void StartMockDownload(DownloadInfo downloadInfo)
        {
            // We can't use coroutines in a non-MonoBehaviour class
            // So we'll simulate instant completion for now in mock
            SimulateInstantDownload(downloadInfo);
        }
        
        private void SimulateInstantDownload(DownloadInfo downloadInfo)
        {
            downloadInfo.status = DownloadStatus.Downloading;
            downloadInfo.progress = 0.5f; // Simulate 50% progress
            downloadInfo.downloadedBytes = downloadInfo.fileSize / 2;
            
            // Simulate random completion or failure
            System.Threading.Tasks.Task.Run(async () =>
            {
                await System.Threading.Tasks.Task.Delay(UnityEngine.Random.Range(2000, 5000)); // 2-5 seconds
                
                if (downloadInfo.status == DownloadStatus.Downloading)
                {
                    // 90% success rate
                    if (UnityEngine.Random.value < 0.9f)
                    {
                        downloadInfo.status = DownloadStatus.Completed;
                        downloadInfo.progress = 1.0f;
                        downloadInfo.downloadedBytes = downloadInfo.fileSize;
                        downloadInfo.filePath = $"/mock/downloads/{downloadInfo.songTitle}.mp3";
                    }
                    else
                    {
                        downloadInfo.status = DownloadStatus.Failed;
                        downloadInfo.errorMessage = "Mock: Simulated network error";
                    }
                    
                    downloadInfo.endTime = DateTime.Now;
                }
            });
        }
        
        private void EnsureMockPlaylistsInitialized()
        {
            if (playlistsInitialized) return;
            
            // Create some sample playlists for testing
            var favoritesPlaylist = new NewYouTubePlaylist("My Favorites")
            {
                description = "All my favorite songs"
            };
            favoritesPlaylist.AddSong(new NewSongDetails { title = "Favorite Song 1", artist = "Great Artist", url = "https://youtube.com/watch?v=fav1", duration = 210, source = "youtube" });
            favoritesPlaylist.AddSong(new NewSongDetails { title = "Favorite Song 2", artist = "Amazing Band", url = "https://youtube.com/watch?v=fav2", duration = 195, source = "youtube" });
            favoritesPlaylist.AddSong(new NewSongDetails { title = "Favorite Song 3", artist = "Cool Artist", url = "https://youtube.com/watch?v=fav3", duration = 240, source = "youtube" });
            mockPlaylists[favoritesPlaylist.id] = favoritesPlaylist;
            
            var chillPlaylist = new NewYouTubePlaylist("Chill Vibes")
            {
                description = "Relaxing music for study and work"
            };
            chillPlaylist.AddSong(new NewSongDetails { title = "Chill Beat 1", artist = "Lo-Fi Master", url = "https://youtube.com/watch?v=chill1", duration = 180, source = "youtube" });
            chillPlaylist.AddSong(new NewSongDetails { title = "Ambient Track", artist = "Soundscape Artist", url = "https://youtube.com/watch?v=ambient1", duration = 300, source = "youtube" });
            mockPlaylists[chillPlaylist.id] = chillPlaylist;
            
            var workoutPlaylist = new NewYouTubePlaylist("Workout Mix")
            {
                description = "High energy tracks for exercise"
            };
            workoutPlaylist.AddSong(new NewSongDetails { title = "Pump Up Song", artist = "Energy Band", url = "https://youtube.com/watch?v=pump1", duration = 220, source = "youtube" });
            workoutPlaylist.AddSong(new NewSongDetails { title = "High BPM Track", artist = "Electronic Artist", url = "https://youtube.com/watch?v=bpm1", duration = 185, source = "youtube" });
            workoutPlaylist.AddSong(new NewSongDetails { title = "Motivational Anthem", artist = "Power Band", url = "https://youtube.com/watch?v=motiv1", duration = 200, source = "youtube" });
            mockPlaylists[workoutPlaylist.id] = workoutPlaylist;
            
            playlistsInitialized = true;
            NewLoggingSystem.Info("Mock: Initialized sample playlists", "MockBackend");
        }
        
        #endregion
        
        #region Events
        
        public event Action<NewSongDetails>? OnTrackStarted;
        public event Action? OnPlaybackPaused;
        public event Action? OnPlaybackResumed;
        public event Action<NewSongDetails>? OnTrackEnded;
        public event Action<float, float>? OnPlaybackProgress;
        
        // Trigger event to prevent CS0067 warning
        private void TriggerPlaybackProgress(float current, float total) => OnPlaybackProgress?.Invoke(current, total);
        public event Action<float>? OnVolumeChanged;
        public event Action<bool>? OnHeadphoneStateChanged;
        public event Action<List<NewSongDetails>>? OnQueueChanged;
        
        #endregion
    }
} 