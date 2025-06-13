using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Utils;
using System.IO;
using MelonLoader;

namespace BackSpeakerMod.Core.Modules
{
    /// <summary>
    /// YouTube streaming controller with download-and-play functionality
    /// Handles smart downloading, caching, and background pre-loading
    /// Works directly with individual SongDetails without requiring a playlist
    /// </summary>
    public class YouTubeStreamingController
    {
        private AudioSource? audioSource;
        private SongDetails? currentSong;
        private YouTubeDownloadCache downloadCache;
        private bool isPlaying = false;
        private bool isLoading = false;
        private float volume = 0.75f;
        private RepeatMode repeatMode = RepeatMode.None;
        
        // Current playback state
        private AudioClip? currentAudioClip;
        
        // Download waiting system
        private SongDetails? pendingPlaySong = null;
        private bool isWaitingForDownload = false;

        // Events
        public Action? OnTrackChanged;
        public Action? OnPlaylistChanged;

        public YouTubeStreamingController()
        {
            downloadCache = new YouTubeDownloadCache();

            // Wire up download events
            downloadCache.OnDownloadCompleted += OnSongDownloadCompleted;
            downloadCache.OnDownloadFailed += OnSongDownloadFailed;
            downloadCache.OnDownloadStarted += OnSongDownloadStarted;
            downloadCache.OnDownloadProgress += OnSongDownloadProgress;
        }
        
        public void Initialize(AudioSource audioSource)
        {
            this.audioSource = audioSource;
            LoggingSystem.Info("YouTubeStreamingController initialized with download-and-play system", "YouTubeStreaming");
        }
        
        /// <summary>
        /// Play a specific song directly
        /// </summary>
        public async Task<bool> PlaySong(SongDetails songDetails)
        {
            if (songDetails == null)
                return false;
                
            currentSong = songDetails;
            isLoading = true;
            LoggingSystem.Info($"🎵 Starting YouTube playback for: {songDetails.title} by {songDetails.GetArtist()}", "YouTubeStreaming");
            
            try
            {
                // Clear any previous pending downloads
                pendingPlaySong = null;
                isWaitingForDownload = false;
                
                // Check if song is already cached
                if (downloadCache.IsSongCached(songDetails))
                {
                    LoggingSystem.Info($"✅ Song is cached, loading from file: {songDetails.title}", "YouTubeStreaming");
                    return await LoadAndPlayCachedSong(songDetails);
                }
                
                // Check if song is currently downloading
                if (songDetails.isDownloading)
                {
                    LoggingSystem.Info($"⏳ Song is currently downloading, waiting for completion: {songDetails.title}", "YouTubeStreaming");
                    
                    // Set up waiting for download completion
                    pendingPlaySong = songDetails;
                    isWaitingForDownload = true;
                    
                    // Update UI to show waiting state
                    return true; // Return true to indicate we're handling it
                }
                
                // Song not cached and not downloading - start download
                LoggingSystem.Info($"⬇️ Song not cached, starting download: {songDetails.title}", "YouTubeStreaming");
                
                // Set up waiting for this download
                pendingPlaySong = songDetails;
                isWaitingForDownload = true;
                
                // Start the download with priority (for immediate playback)
                bool downloadStarted = await Task.Run(() => {
                    try
                    {
                        // Use priority download for immediate playback needs
                        downloadCache.QueueForPriorityDownload(songDetails);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        LoggingSystem.Error($"Error starting priority download: {ex.Message}", "YouTubeStreaming");
                        return false;
                    }
                });
                
                if (downloadStarted)
                {
                    LoggingSystem.Info($"📥 Download started for: {songDetails.title} - waiting for completion...", "YouTubeStreaming");
                    return true;
                }
                else
                {
                    LoggingSystem.Error($"❌ Failed to start download for: {songDetails.title}", "YouTubeStreaming");
                    pendingPlaySong = null;
                    isWaitingForDownload = false;
                    isLoading = false;
                    return false;
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error playing YouTube track {songDetails.title}: {ex.Message}", "YouTubeStreaming");
                pendingPlaySong = null;
                isWaitingForDownload = false;
                isLoading = false;
                return false;
            }
        }
        
        /// <summary>
        /// Legacy method for compatibility - converts to PlaySong
        /// </summary>
        public async Task<bool> PlayTrack(int trackIndex)
        {
            // This method is now deprecated since we work with individual songs
            // It's kept for compatibility but doesn't have a playlist context
            LoggingSystem.Warning("PlayTrack(int) called but YouTubeStreamingController now works with individual songs", "YouTubeStreaming");
            return false;
        }
        
        /// <summary>
        /// Load and play a song from cache (Unity-safe)
        /// </summary>
        private IEnumerator LoadAndPlayCachedSongCoroutine(SongDetails songDetails)
        {
            if (string.IsNullOrEmpty(songDetails.cachedFilePath))
            {
                LoggingSystem.Error($"Cached file path is null for: {songDetails.title}", "YouTubeStreaming");
                isLoading = false;
                yield break;
            }
            
            // Verify file exists before attempting to load
            if (!File.Exists(songDetails.cachedFilePath))
            {
                LoggingSystem.Error($"Cached file not found: {songDetails.cachedFilePath}", "YouTubeStreaming");
                isLoading = false;
                yield break;
            }
            
            LoggingSystem.Debug($"Loading audio from cached file: {songDetails.cachedFilePath}", "YouTubeStreaming");
            
            // Load the audio clip from the cached file using coroutine-based loading
            AudioClip? audioClip = null;
            bool loadingComplete = false;
            Exception? loadException = null;
            
            // Start loading on a separate coroutine
            MelonCoroutines.Start(LoadAudioFileCoroutine(songDetails.cachedFilePath, 
                (clip, error) => {
                    audioClip = clip;
                    loadException = error;
                    loadingComplete = true;
                }));
            
            // Wait for loading to complete
            while (!loadingComplete)
            {
                yield return null;
            }
            
            if (loadException != null)
            {
                LoggingSystem.Error($"AudioHelper failed to load file: {loadException.Message}", "YouTubeStreaming");
                isLoading = false;
                yield break;
            }
            
            if (audioClip == null)
            {
                LoggingSystem.Error($"Failed to load audio from cached file: {songDetails.title}", "YouTubeStreaming");
                isLoading = false;
                yield break;
            }
            
            // Set up the audio on the main thread (Unity-safe)
            if (audioSource != null)
            {
                // These operations are now guaranteed to be on the main thread
                audioSource.clip = audioClip;
                audioSource.volume = volume;
                audioSource.Play();
                isPlaying = true;
                isLoading = false;
                
                // Store current playback info
                currentAudioClip = audioClip;
                currentSong = songDetails;
                
                LoggingSystem.Info($"Successfully started YouTube playback: {songDetails.title} ({audioClip.length:F1}s)", "YouTubeStreaming");
                OnTrackChanged?.Invoke();
            }
            else
            {
                LoggingSystem.Error($"AudioSource is null, cannot play: {songDetails.title}", "YouTubeStreaming");
                isLoading = false;
            }
        }
        
        /// <summary>
        /// Unity-safe coroutine for loading audio files
        /// </summary>
        private IEnumerator LoadAudioFileCoroutine(string filePath, Action<AudioClip?, Exception?> onComplete)
        {
            AudioClip? result = null;
            Exception? error = null;
            bool completed = false;
            
            // Start async loading on background thread
            var loadTask = Task.Run(async () => {
                try
                {
                    result = await AudioHelper.LoadAudioFileAsync(filePath);
                }
                catch (Exception ex)
                {
                    error = ex;
                }
                finally
                {
                    completed = true;
                }
            });
            
            // Wait for completion on main thread
            while (!completed)
            {
                yield return null;
            }
            
            // Call completion callback on main thread
            onComplete?.Invoke(result, error);
        }

        /// <summary>
        /// Load and play a song from cache (wrapper for coroutine)
        /// </summary>
        private async Task<bool> LoadAndPlayCachedSong(SongDetails songDetails)
        {
            // Start the Unity-safe coroutine
            MelonCoroutines.Start(LoadAndPlayCachedSongCoroutine(songDetails));
            
            // Wait a frame to let the coroutine start
            await Task.Delay(100);
            
            // Return true if loading started successfully
            return !isLoading || isPlaying;
        }

        // Download event handlers
        private void OnSongDownloadStarted(SongDetails song)
        {
            LoggingSystem.Debug($"Download started: {song.title}", "YouTubeStreaming");
            // OnTrackChanged?.Invoke();
        }
        
        private void OnSongDownloadProgress(SongDetails song)
        {
            LoggingSystem.Debug($"Download progress called for {song.title} with progress: {song.downloadProgress}", "YouTubeStreaming");
            
            // If we're waiting for this download, update UI or state
            if (isWaitingForDownload && pendingPlaySong != null && 
                pendingPlaySong.url == song.url)
            {
                // Update the download progress
                pendingPlaySong.downloadProgress = song.downloadProgress;
                LoggingSystem.Debug($"Download progress for {pendingPlaySong.title}: {pendingPlaySong.downloadProgress}", "YouTubeStreaming");
                
                // Update UI or notify listeners about download progress
                OnTrackChanged?.Invoke();
            }
        }
        
        private void OnSongDownloadCompleted(SongDetails song)
        {
            LoggingSystem.Info($"✅ Download completed: {song.title}", "YouTubeStreaming");

            // Check if this is the song we're waiting to play
            if (isWaitingForDownload && pendingPlaySong != null &&
                pendingPlaySong.url == song.url)
            {
                LoggingSystem.Info($"🚀 Auto-playing completed download: {song.title}", "YouTubeStreaming");

                // Clear waiting state
                pendingPlaySong = null;
                isWaitingForDownload = false;
                currentSong = song;

                // Notify UI components that track has changed
                OnTrackChanged?.Invoke();

                // Auto-play the song using Unity-safe coroutine
                MelonCoroutines.Start(AutoPlayDownloadedSongCoroutine(song));
            }
        }

        /// <summary>
        /// Unity-safe coroutine for auto-playing downloaded songs
        /// </summary>
        private IEnumerator AutoPlayDownloadedSongCoroutine(SongDetails song)
        {
            // Use the Unity-safe coroutine method
            yield return LoadAndPlayCachedSongCoroutine(song);

            if (!isPlaying)
            {
                LoggingSystem.Error($"❌ Auto-play failed for: {song.title}", "YouTubeStreaming");
                isLoading = false;
            }
        }
        
        private void OnSongDownloadFailed(SongDetails song)
        {
            LoggingSystem.Warning($"❌ Download failed: {song.title}", "YouTubeStreaming");
            
            // Check if this was the song we're waiting to play
            if (isWaitingForDownload && pendingPlaySong != null && 
                pendingPlaySong.url == song.url)
            {
                LoggingSystem.Error($"🚫 Auto-play cancelled - download failed for: {song.title}", "YouTubeStreaming");
                
                // Clear waiting state
                pendingPlaySong = null;
                isWaitingForDownload = false;
                isLoading = false;
            }
            
            OnTrackChanged?.Invoke();
        }
        
        public void Play()
        {
            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.Play();
                isPlaying = true;
                LoggingSystem.Debug("YouTube playback resumed", "YouTubeStreaming");
            }
        }
        
        public void Pause()
        {
            if (audioSource != null)
            {
                audioSource.Pause();
                isPlaying = false;
                LoggingSystem.Debug("YouTube playback paused", "YouTubeStreaming");
            }
        }
        
        public void Stop()
        {
            if (audioSource != null)
            {
                audioSource.Stop();
                isPlaying = false;
                LoggingSystem.Debug("YouTube playback stopped", "YouTubeStreaming");
            }
        }
        
        public void SetVolume(float newVolume)
        {
            volume = Mathf.Clamp01(newVolume);
            if (audioSource != null)
            {
                audioSource.volume = volume;
            }
        }
        
        public void SeekToTime(float time)
        {
            if (audioSource != null && audioSource.clip != null)
            {
                time = Mathf.Clamp(time, 0f, audioSource.clip.length);
                audioSource.time = time;
            }
        }
        
        public void SeekToProgress(float progress)
        {
            if (audioSource != null && audioSource.clip != null)
            {
                progress = Mathf.Clamp01(progress);
                audioSource.time = progress * audioSource.clip.length;
            }
        }
        
        /// <summary>
        /// Update the streaming controller (call from main update loop)
        /// </summary>
        public void Update()
        {
            if (!isPlaying || audioSource == null || currentAudioClip == null) return;
            
            // Check if current track ended
            if (!audioSource.isPlaying && isPlaying)
            {
                LoggingSystem.Debug("YouTube track ended naturally", "YouTubeStreaming");
                
                if (repeatMode == RepeatMode.RepeatOne)
                {
                    // Restart current track
                    LoggingSystem.Debug("Repeating current YouTube track", "YouTubeStreaming");
                    audioSource.Play();
                }
                else
                {
                    // Mark as finished - let the session manager handle next track
                    isPlaying = false;
                    LoggingSystem.Info("YouTube track completed - session manager should handle next track", "YouTubeStreaming");
                    
                    // Trigger track changed event to notify session manager
                    OnTrackChanged?.Invoke();
                }
            }
        }
        
        // Properties
        public bool IsPlaying => isPlaying && !isLoading;
        public bool IsLoading => isLoading;
        public bool IsWaitingForDownload => isWaitingForDownload;
        public float CurrentVolume => volume;
        public float CurrentTime => audioSource?.time ?? 0f;
        public float TotalTime => audioSource?.clip?.length ?? 0f;
        public float Progress => TotalTime > 0 ? CurrentTime / TotalTime : 0f;
        public bool IsAudioReady => audioSource != null && !isLoading;
        public RepeatMode RepeatMode { get => repeatMode; set => repeatMode = value; }

        public bool IsDownloadInProgress()
        {
            if (isWaitingForDownload && pendingPlaySong != null)
            {
                return pendingPlaySong.isDownloading;
            }
            return false;
        }
        public string GetDownloadProgress()
        {
            // Get download progress for the current song
            if (pendingPlaySong != null)
            {
                return pendingPlaySong.downloadProgress ?? "0%";
            }
            return "0%";
        }
        
        public string GetCurrentTrackInfo()
        {
            if (isWaitingForDownload && pendingPlaySong != null)
                return $"{pendingPlaySong.title} (downloading...)";
            return currentSong?.title ?? "No track selected";
        }
        
        public string GetCurrentArtistInfo()
        {
            if (isWaitingForDownload && pendingPlaySong != null)
                return pendingPlaySong.GetArtist();
            return currentSong?.GetArtist() ?? "Unknown artist";
        }
        
        public void Reset()
        {
            Stop();
            currentAudioClip = null;
            currentSong = null;
            isLoading = false;
            
            LoggingSystem.Info("YouTubeStreamingController reset", "YouTubeStreaming");
        }
    }
} 