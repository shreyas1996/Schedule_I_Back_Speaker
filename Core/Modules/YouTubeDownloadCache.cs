using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Linq;
using BackSpeakerMod.Utils;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Features.Audio;
using MelonLoader;

namespace BackSpeakerMod.Core.Modules
{
    /// <summary>
    /// Manages YouTube audio downloads and caching
    /// Handles smart downloading, cache checking, and background downloading
    /// </summary>
    public class YouTubeDownloadCache
    {
        private readonly string cacheDirectory;
        private Dictionary<string, SongDetails> downloadQueue = new Dictionary<string, SongDetails>();
        private HashSet<string> activeDownloads = new HashSet<string>();
        private readonly object cacheLock = new object();
        
        private bool isBackgroundDownloading = false;
        private const int MaxConcurrentDownloads = 1;
        
        // Events
        public Action<SongDetails>? OnDownloadCompleted;
        public Action<SongDetails>? OnDownloadFailed;
        public Action<SongDetails>? OnDownloadStarted;
        
        public YouTubeDownloadCache()
        {
            LoggingSystem.Info("YouTube Download Cache initialized", "YouTubeCache");
            
            // Create cache directory in the mod's data folder
            var gameDirectory = Directory.GetCurrentDirectory();
            cacheDirectory = Path.Combine(gameDirectory, "Mods", "BackSpeaker", "Cache", "YouTube");
            
            try
            {
                Directory.CreateDirectory(cacheDirectory);
                LoggingSystem.Info($"YouTube cache directory: {cacheDirectory}", "YouTubeCache");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to create cache directory: {ex.Message}", "YouTubeCache");
                throw;
            }
        }
        
        /// <summary>
        /// Check if a song is already cached and ready to play
        /// </summary>
        public bool IsSongCached(SongDetails song)
        {
            lock (cacheLock)
            {
                try
                {
                    var videoId = song.GetVideoId();
                    if (string.IsNullOrEmpty(videoId))
                    {
                        LoggingSystem.Warning($"Cannot check cache - no video ID for song: {song.title}", "YouTubeCache");
                        return false;
                    }

                    LoggingSystem.Debug($"=== CACHE CHECK START for {song.title} (ID: {videoId}) ===", "YouTubeCache");
                    
                    // List ALL files in cache directory for debugging
                    if (Directory.Exists(cacheDirectory))
                    {
                        var allFiles = Directory.GetFiles(cacheDirectory, "*.mp3", SearchOption.TopDirectoryOnly);
                        LoggingSystem.Debug($"Cache directory contains {allFiles.Length} MP3 files:", "YouTubeCache");
                        foreach (var file in allFiles)
                        {
                            var fileInfo = new FileInfo(file);
                            LoggingSystem.Debug($"  - {Path.GetFileName(file)} ({fileInfo.Length} bytes)", "YouTubeCache");
                        }
                    }
                    else
                    {
                        LoggingSystem.Warning($"Cache directory does not exist: {cacheDirectory}", "YouTubeCache");
                        return false;
                    }

                    // Method 1: Check exact video ID filename
                    var exactFilePath = Path.Combine(cacheDirectory, $"{videoId}.mp3");
                    LoggingSystem.Debug($"Checking exact match: {exactFilePath}", "YouTubeCache");
                    
                    if (File.Exists(exactFilePath))
                    {
                        var fileInfo = new FileInfo(exactFilePath);
                        LoggingSystem.Debug($"Found exact match file, size: {fileInfo.Length} bytes", "YouTubeCache");
                        
                        if (fileInfo.Length > 0)
                        {
                            song.cachedFilePath = exactFilePath;
                            song.isDownloaded = true;
                            song.downloadFailed = false;
                            song.isDownloading = false;
                            song.fileSizeBytes = fileInfo.Length;
                            
                            LoggingSystem.Info($"✅ CACHE HIT (exact): {song.title} -> {Path.GetFileName(exactFilePath)} ({fileInfo.Length} bytes)", "YouTubeCache");
                            return true;
                        }
                        else
                        {
                            LoggingSystem.Warning($"Found exact match but file is empty, deleting: {exactFilePath}", "YouTubeCache");
                            try { File.Delete(exactFilePath); } catch { }
                        }
                    }
                    else
                    {
                        LoggingSystem.Debug($"No exact match found for: {exactFilePath}", "YouTubeCache");
                    }

                    // Method 2: Check files containing video ID (more flexible)
                    var allMp3Files = Directory.GetFiles(cacheDirectory, "*.mp3", SearchOption.TopDirectoryOnly);
                    LoggingSystem.Debug($"Searching {allMp3Files.Length} MP3 files for video ID '{videoId}'", "YouTubeCache");
                    
                    foreach (var filePath in allMp3Files)
                    {
                        var fileName = Path.GetFileNameWithoutExtension(filePath);
                        LoggingSystem.Debug($"Checking filename: '{fileName}' for video ID '{videoId}'", "YouTubeCache");
                        
                        if (fileName.Contains(videoId))
                        {
                            LoggingSystem.Debug($"Found filename containing video ID: {fileName}", "YouTubeCache");
                            
                            if (File.Exists(filePath))
                            {
                                var fileInfo = new FileInfo(filePath);
                                LoggingSystem.Debug($"File exists, size: {fileInfo.Length} bytes", "YouTubeCache");
                                
                                if (fileInfo.Length > 0)
                                {
                                    song.cachedFilePath = filePath;
                                    song.isDownloaded = true;
                                    song.downloadFailed = false;
                                    song.isDownloading = false;
                                    song.fileSizeBytes = fileInfo.Length;
                                    
                                    LoggingSystem.Info($"✅ CACHE HIT (contains): {song.title} -> {Path.GetFileName(filePath)} ({fileInfo.Length} bytes)", "YouTubeCache");
                                    return true;
                                }
                                else
                                {
                                    LoggingSystem.Warning($"Found matching file but it's empty, deleting: {filePath}", "YouTubeCache");
                                    try { File.Delete(filePath); } catch { }
                                }
                            }
                        }
                    }
                    
                    // Method 3: Check if song metadata thinks it's cached
                    if (song.isDownloaded && !string.IsNullOrEmpty(song.cachedFilePath))
                    {
                        LoggingSystem.Debug($"Song metadata indicates cached file: {song.cachedFilePath}", "YouTubeCache");
                        
                        if (File.Exists(song.cachedFilePath))
                        {
                            var fileInfo = new FileInfo(song.cachedFilePath);
                            LoggingSystem.Debug($"Metadata file exists, size: {fileInfo.Length} bytes", "YouTubeCache");
                            
                            if (fileInfo.Length > 0)
                            {
                                song.fileSizeBytes = fileInfo.Length;
                                LoggingSystem.Info($"✅ CACHE HIT (metadata): {song.title} -> {Path.GetFileName(song.cachedFilePath)} ({fileInfo.Length} bytes)", "YouTubeCache");
                                return true;
                            }
                            else
                            {
                                LoggingSystem.Warning($"Metadata file is empty, resetting: {song.cachedFilePath}", "YouTubeCache");
                                try { File.Delete(song.cachedFilePath); } catch { }
                                ResetSongMetadata(song);
                            }
                        }
                        else
                        {
                            LoggingSystem.Warning($"Metadata file missing, resetting: {song.cachedFilePath}", "YouTubeCache");
                            ResetSongMetadata(song);
                        }
                    }
                    
                    LoggingSystem.Info($"❌ CACHE MISS: {song.title} (ID: {videoId}) - not found in cache", "YouTubeCache");
                    LoggingSystem.Debug($"=== CACHE CHECK END ===", "YouTubeCache");
                    return false;
                }
                catch (Exception ex)
                {
                    LoggingSystem.Error($"Error checking cache for {song.title}: {ex.Message}", "YouTubeCache");
                    LoggingSystem.Error($"Stack trace: {ex.StackTrace}", "YouTubeCache");
                    return false;
                }
            }
        }
        
        /// <summary>
        /// Reset song metadata for fresh download attempt
        /// </summary>
        private void ResetSongMetadata(SongDetails song)
        {
            song.isDownloaded = false;
            song.cachedFilePath = null;
            song.fileSizeBytes = null;
            song.downloadFailed = false;
            song.isDownloading = false;
        }
        
        /// <summary>
        /// Queue songs for background downloading using MelonCoroutines
        /// </summary>
        public void QueueForBackgroundDownload(List<SongDetails> songs)
        {
            if (songs == null || songs.Count == 0) return;
            
            lock (cacheLock)
            {
                foreach (var song in songs)
                {
                    var videoId = song.GetVideoId();
                    if (!IsSongCached(song) && !downloadQueue.ContainsKey(videoId) && !activeDownloads.Contains(videoId))
                    {
                        downloadQueue[videoId] = song;
                        LoggingSystem.Info($"📥 Queued for background download: {song.title} by {song.GetArtist()}", "YouTubeCache");
                    }
                }
            }
            
            // Start background downloading if not already running
            if (!isBackgroundDownloading)
            {
                MelonCoroutines.Start(BackgroundDownloadCoroutine());
            }
        }
        
        /// <summary>
        /// Queue a single song for priority download (immediate playback needs)
        /// </summary>
        public void QueueForPriorityDownload(SongDetails song)
        {
            if (song == null) return;
            
            var videoId = song.GetVideoId();
            if (IsSongCached(song))
            {
                LoggingSystem.Info($"Song already cached, skipping priority download: {song.title}", "YouTubeCache");
                return;
            }
            
            lock (cacheLock)
            {
                if (!activeDownloads.Contains(videoId))
                {
                    // Insert at front of queue for priority
                    var tempQueue = new Dictionary<string, SongDetails> { { videoId, song } };
                    foreach (var kvp in downloadQueue)
                    {
                        tempQueue[kvp.Key] = kvp.Value;
                    }
                    downloadQueue = tempQueue;
                    
                    LoggingSystem.Info($"🚀 Queued for priority download: {song.title} by {song.GetArtist()}", "YouTubeCache");
                }
            }
            
            // Start background downloading if not already running
            if (!isBackgroundDownloading)
            {
                MelonCoroutines.Start(BackgroundDownloadCoroutine());
            }
        }
        
        /// <summary>
        /// Background download coroutine using MelonCoroutines (Unity-safe)
        /// </summary>
        private IEnumerator BackgroundDownloadCoroutine()
        {
            isBackgroundDownloading = true;
            LoggingSystem.Info("🔄 Background download coroutine started", "YouTubeCache");
            
            while (true)
            {
                SongDetails? nextSong = null;
                string? videoId = null;
                
                // Get next song to download
                lock (cacheLock)
                {
                    if (downloadQueue.Count == 0)
                    {
                        LoggingSystem.Info("📭 Download queue empty, stopping background downloads", "YouTubeCache");
                        break;
                    }
                    
                    if (activeDownloads.Count >= MaxConcurrentDownloads)
                    {
                        LoggingSystem.Info($"⏳ Max concurrent downloads reached ({MaxConcurrentDownloads}), waiting...", "YouTubeCache");
                        // Wait and continue to next iteration
                        yield return new UnityEngine.WaitForSeconds(1f);
                        continue;
                    }
                    
                    var firstItem = downloadQueue.First();
                    videoId = firstItem.Key;
                    nextSong = firstItem.Value;
                    
                    downloadQueue.Remove(videoId);
                    activeDownloads.Add(videoId);
                }
                
                if (nextSong != null && !string.IsNullOrEmpty(videoId))
                {
                    LoggingSystem.Info($"⬇️ Starting background download: {nextSong.title} by {nextSong.GetArtist()}", "YouTubeCache");
                    
                    // Start download coroutine
                    yield return MelonCoroutines.Start(DownloadSongCoroutine(nextSong, videoId));
                }
                
                // Small delay between downloads
                yield return new UnityEngine.WaitForSeconds(0.5f);
            }
            
            isBackgroundDownloading = false;
            LoggingSystem.Info("🏁 Background download coroutine finished", "YouTubeCache");
        }
        
        /// <summary>
        /// Download a single song using coroutines
        /// </summary>
        private IEnumerator DownloadSongCoroutine(SongDetails song, string videoId)
        {
            bool downloadCompleted = false;
            bool downloadSuccess = false;
            
            LoggingSystem.Info($"🎵 Downloading: {song.title} by {song.GetArtist()}", "YouTubeCache");
            
            // Mark song as downloading
            song.isDownloading = true;
            song.downloadFailed = false;
            
            // Fire download started event
            OnDownloadStarted?.Invoke(song);
            
            // Use YoutubeHelper with coroutine-safe callback
            YoutubeHelper.DownloadSong(song, (success) => {
                downloadSuccess = success;
                downloadCompleted = true;
            });
            
            // Wait for download to complete
            while (!downloadCompleted)
            {
                yield return new UnityEngine.WaitForSeconds(0.1f);
            }
            
            // Update song metadata based on result
            song.isDownloading = false;
            
            if (downloadSuccess)
            {
                LoggingSystem.Info($"✅ Successfully downloaded: {song.title}", "YouTubeCache");
                
                // Mark as downloaded and update metadata
                song.isDownloaded = true;
                song.downloadFailed = false;
                song.downloadTimestamp = DateTime.Now;
                
                // Try to get the cached file path and size
                var cachedPath = YoutubeHelper.FindDownloadedFile(song.url ?? "");
                if (!string.IsNullOrEmpty(cachedPath) && File.Exists(cachedPath))
                {
                    song.cachedFilePath = cachedPath;
                    try
                    {
                        song.fileSizeBytes = new FileInfo(cachedPath).Length;
                    }
                    catch (Exception ex)
                    {
                        LoggingSystem.Warning($"Could not get file size for {cachedPath}: {ex.Message}", "YouTubeCache");
                    }
                }
                
                // Save metadata to persistent storage
                bool metadataSaved = YouTubeMetadataManager.AddOrUpdateSong(song);
                if (metadataSaved)
                {
                    LoggingSystem.Debug($"Saved metadata for downloaded song: {song.title}", "YouTubeCache");
                }
                else
                {
                    LoggingSystem.Warning($"Failed to save metadata for: {song.title}", "YouTubeCache");
                }
                
                // Update playlist status for this song
                YouTubePlaylistManager.UpdateSongDownloadStatus(videoId, true, cachedPath);
                
                // Fire download completed event
                OnDownloadCompleted?.Invoke(song);
            }
            else
            {
                LoggingSystem.Warning($"❌ Failed to download: {song.title}", "YouTubeCache");
                
                // Mark as failed
                song.downloadFailed = true;
                song.isDownloaded = false;
                
                // Fire download failed event
                OnDownloadFailed?.Invoke(song);
            }
            
            // Remove from active downloads
            lock (cacheLock)
            {
                activeDownloads.Remove(videoId);
            }
            
            LoggingSystem.Info($"🏁 Download completed for: {song.title} (Active downloads: {activeDownloads.Count})", "YouTubeCache");
        }
        
        /// <summary>
        /// Check if a song is currently downloading
        /// </summary>
        public bool IsSongDownloading(SongDetails songDetails)
        {
            if (songDetails == null) return false;
            
            var videoId = songDetails.GetVideoId();
            lock (cacheLock)
            {
                return activeDownloads.Contains(videoId);
            }
        }

        /// <summary>
        /// Get cache statistics
        /// </summary>
        public (int totalFiles, long totalSizeBytes) GetCacheStats()
        {
            try
            {
                if (!Directory.Exists(YoutubeHelper.GetYouTubeCacheDirectory()))
                    return (0, 0);

                var files = Directory.GetFiles(YoutubeHelper.GetYouTubeCacheDirectory(), "*.mp3");
                var totalSize = files.Sum(f => new FileInfo(f).Length);
                
                return (files.Length, totalSize);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error getting cache stats: {ex.Message}", "YouTubeCache");
                return (0, 0);
            }
        }

        /// <summary>
        /// Clean up old cached files
        /// </summary>
        public void CleanupOldFiles(TimeSpan maxAge)
        {
            try
            {
                var cacheDir = YoutubeHelper.GetYouTubeCacheDirectory();
                if (!Directory.Exists(cacheDir)) return;

                var cutoffTime = DateTime.Now - maxAge;
                var files = Directory.GetFiles(cacheDir, "*.mp3");
                
                foreach (var file in files)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.LastAccessTime < cutoffTime)
                        {
                            File.Delete(file);
                            LoggingSystem.Info($"Deleted old cache file: {Path.GetFileName(file)}", "YouTubeCache");
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingSystem.Warning($"Failed to delete old cache file {file}: {ex.Message}", "YouTubeCache");
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error cleaning up old files: {ex.Message}", "YouTubeCache");
            }
        }

        /// <summary>
        /// Get current download status
        /// </summary>
        public string GetDownloadStatus()
        {
            lock (cacheLock)
            {
                return $"Queue: {downloadQueue.Count}, Active: {activeDownloads.Count}";
            }
        }
    }
} 