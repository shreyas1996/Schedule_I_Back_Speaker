using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Utils;
using MelonLoader;

namespace BackSpeakerMod.Core.Modules
{
    /// <summary>
    /// Centralized YouTube download manager with queue system
    /// Handles individual downloads, batch downloads, and progress tracking
    /// </summary>
    public class YouTubeDownloadManager
    {
        private YouTubeDownloadCache downloadCache;
        private Dictionary<string, SongDetails> downloadQueue = new Dictionary<string, SongDetails>();
        private Dictionary<string, SongDetails> activeDownloads = new Dictionary<string, SongDetails>();
        private HashSet<string> cancelledDownloads = new HashSet<string>();
        private bool isProcessingQueue = false;
        
        private const int MaxConcurrentDownloads = 3;
        
        // Events
        public Action<SongDetails>? OnDownloadStarted;
        public Action<SongDetails>? OnDownloadProgress;
        public Action<SongDetails>? OnDownloadCompleted;
        public Action<SongDetails>? OnDownloadFailed;
        public Action<SongDetails>? OnDownloadCancelled;
        public Action? OnQueueUpdated;
        
        public YouTubeDownloadManager()
        {
            downloadCache = new YouTubeDownloadCache();
        }
        
        /// <summary>
        /// Queue a single song for download
        /// </summary>
        public bool QueueDownload(SongDetails song)
        {
            if (song == null) return false;
            
            var videoId = song.GetVideoId();
            
            // Check if already downloaded
            if (downloadCache.IsSongCached(song) && song.IsReadyToPlay())
            {
                LoggingSystem.Info($"Song already downloaded: {song.title}", "DownloadManager");
                return false;
            }
            
            // Check if already in queue or downloading
            if (downloadQueue.ContainsKey(videoId) || activeDownloads.ContainsKey(videoId))
            {
                LoggingSystem.Info($"Song already queued or downloading: {song.title}", "DownloadManager");
                return false;
            }
            
            // Add to queue
            downloadQueue[videoId] = song;
            song.isDownloading = false; // Mark as queued, not downloading yet
            song.downloadFailed = false;
            
            LoggingSystem.Info($"📥 Queued for download: {song.title} by {song.GetArtist()}", "DownloadManager");
            
            OnQueueUpdated?.Invoke();
            
            // Start processing if not already running
            if (!isProcessingQueue)
            {
                MelonCoroutines.Start(ProcessDownloadQueueCoroutine());
            }
            
            return true;
        }
        
        /// <summary>
        /// Queue multiple songs for download (Download All functionality)
        /// </summary>
        public int QueueDownloads(List<SongDetails> songs)
        {
            if (songs == null || songs.Count == 0) return 0;
            
            int queuedCount = 0;
            foreach (var song in songs)
            {
                if (QueueDownload(song))
                {
                    queuedCount++;
                }
            }
            
            LoggingSystem.Info($"📥 Queued {queuedCount} songs for download", "DownloadManager");
            return queuedCount;
        }
        
        /// <summary>
        /// Cancel a download (remove from queue or stop active download)
        /// </summary>
        public bool CancelDownload(SongDetails song)
        {
            if (song == null) return false;
            
            var videoId = song.GetVideoId();
            
            // Mark as cancelled
            cancelledDownloads.Add(videoId);
            
            // Remove from queue if present
            if (downloadQueue.ContainsKey(videoId))
            {
                downloadQueue.Remove(videoId);
                song.isDownloading = false;
                LoggingSystem.Info($"❌ Cancelled queued download: {song.title}", "DownloadManager");
                OnDownloadCancelled?.Invoke(song);
                OnQueueUpdated?.Invoke();
                return true;
            }
            
            // If actively downloading, it will be cancelled in the next update
            if (activeDownloads.ContainsKey(videoId))
            {
                LoggingSystem.Info($"❌ Cancelling active download: {song.title}", "DownloadManager");
                OnDownloadCancelled?.Invoke(song);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Get current download queue status
        /// </summary>
        public (int queued, int active, int total) GetQueueStatus()
        {
            return (downloadQueue.Count, activeDownloads.Count, downloadQueue.Count + activeDownloads.Count);
        }
        
        /// <summary>
        /// Get download status for a specific song
        /// </summary>
        public DownloadStatus GetDownloadStatus(SongDetails song)
        {
            if (song == null) return DownloadStatus.NotQueued;
            
            var videoId = song.GetVideoId();
            
            if (downloadCache.IsSongCached(song) && song.IsReadyToPlay())
                return DownloadStatus.Downloaded;
            
            if (activeDownloads.ContainsKey(videoId))
                return DownloadStatus.Downloading;
            
            if (downloadQueue.ContainsKey(videoId))
                return DownloadStatus.Queued;
            
            if (song.downloadFailed)
                return DownloadStatus.Failed;
            
            return DownloadStatus.NotQueued;
        }
        
        /// <summary>
        /// Get download progress for a specific song (0-100)
        /// </summary>
        public float GetDownloadProgress(SongDetails song)
        {
            if (song == null) return 0f;
            
            var status = GetDownloadStatus(song);
            if (status == DownloadStatus.Downloaded) return 100f;
            if (status == DownloadStatus.NotQueued || status == DownloadStatus.Failed) return 0f;
            
            // Parse progress from song.downloadProgress (e.g., "45%")
            if (!string.IsNullOrEmpty(song.downloadProgress))
            {
                var progressStr = song.downloadProgress.Replace("%", "");
                if (float.TryParse(progressStr, out float progress))
                {
                    return Mathf.Clamp(progress, 0f, 100f);
                }
            }
            
            return 0f;
        }
        
        /// <summary>
        /// Clear all queued downloads
        /// </summary>
        public void ClearQueue()
        {
            foreach (var song in downloadQueue.Values)
            {
                song.isDownloading = false;
                OnDownloadCancelled?.Invoke(song);
            }
            
            downloadQueue.Clear();
            OnQueueUpdated?.Invoke();
            LoggingSystem.Info("🗑️ Cleared download queue", "DownloadManager");
        }
        
        /// <summary>
        /// Main download processing coroutine
        /// </summary>
        private IEnumerator ProcessDownloadQueueCoroutine()
        {
            isProcessingQueue = true;
            LoggingSystem.Info("🔄 Download queue processing started", "DownloadManager");
            
            while (downloadQueue.Count > 0 || activeDownloads.Count > 0)
            {
                // Start new downloads if we have capacity
                while (downloadQueue.Count > 0 && activeDownloads.Count < MaxConcurrentDownloads)
                {
                    var nextItem = downloadQueue.First();
                    var videoId = nextItem.Key;
                    var song = nextItem.Value;
                    
                    // Check if cancelled
                    if (cancelledDownloads.Contains(videoId))
                    {
                        downloadQueue.Remove(videoId);
                        cancelledDownloads.Remove(videoId);
                        continue;
                    }
                    
                    // Move from queue to active downloads
                    downloadQueue.Remove(videoId);
                    activeDownloads[videoId] = song;
                    
                    // Start download
                    MelonCoroutines.Start(DownloadSongCoroutine(song, videoId));
                    
                    OnQueueUpdated?.Invoke();
                }
                
                // Wait before checking again
                yield return new UnityEngine.WaitForSeconds(0.5f);
            }
            
            isProcessingQueue = false;
            LoggingSystem.Info("🏁 Download queue processing finished", "DownloadManager");
        }
        
        /// <summary>
        /// Download a single song
        /// </summary>
        private IEnumerator DownloadSongCoroutine(SongDetails song, string videoId)
        {
            LoggingSystem.Info($"⬇️ Starting download: {song.title} by {song.GetArtist()}", "DownloadManager");
            
            // Mark as downloading
            song.isDownloading = true;
            song.downloadFailed = false;
            song.downloadProgress = "0%";
            
            OnDownloadStarted?.Invoke(song);
            
            bool downloadCompleted = false;
            bool downloadSuccess = false;
            
            // Use YoutubeHelper for actual download
            YoutubeHelper.DownloadSong(song, 
                (progress) => {
                    // Check if cancelled
                    if (cancelledDownloads.Contains(videoId))
                    {
                        downloadCompleted = true;
                        downloadSuccess = false;
                        return;
                    }
                    
                    song.downloadProgress = progress;
                    OnDownloadProgress?.Invoke(song);
                }, 
                (success) => {
                    downloadSuccess = success;
                    downloadCompleted = true;
                });
            
            // Wait for download to complete
            while (!downloadCompleted)
            {
                // Check if cancelled during download
                if (cancelledDownloads.Contains(videoId))
                {
                    LoggingSystem.Info($"❌ Download cancelled: {song.title}", "DownloadManager");
                    break;
                }
                
                yield return new UnityEngine.WaitForSeconds(0.1f);
            }
            
            // Clean up
            activeDownloads.Remove(videoId);
            cancelledDownloads.Remove(videoId);
            song.isDownloading = false;
            
            if (downloadSuccess && !cancelledDownloads.Contains(videoId))
            {
                LoggingSystem.Info($"✅ Download completed: {song.title}", "DownloadManager");
                song.MarkDownloadCompleted();
                OnDownloadCompleted?.Invoke(song);
            }
            else
            {
                LoggingSystem.Warning($"❌ Download failed: {song.title}", "DownloadManager");
                song.MarkDownloadFailed();
                OnDownloadFailed?.Invoke(song);
            }
            
            OnQueueUpdated?.Invoke();
        }
        
        /// <summary>
        /// Check if download cache has a song
        /// </summary>
        public bool IsSongCached(SongDetails song)
        {
            return downloadCache.IsSongCached(song);
        }
    }
    
    /// <summary>
    /// Download status enumeration
    /// </summary>
    public enum DownloadStatus
    {
        NotQueued,
        Queued,
        Downloading,
        Downloaded,
        Failed
    }
} 