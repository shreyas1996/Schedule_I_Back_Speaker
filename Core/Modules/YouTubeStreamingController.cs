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
    /// Specialized controller for YouTube audio downloading and playback
    /// Handles smart downloading, caching, and background pre-loading
    /// </summary>
    public class YouTubeStreamingController
    {
        private AudioSource? audioSource;
        private YouTubePlaylist? currentPlaylist;
        private YouTubeDownloadCache downloadCache;
        private bool isPlaying = false;
        private bool isLoading = false;
        private float volume = 0.75f;
        private RepeatMode repeatMode = RepeatMode.None;
        
        // Current playback state
        private AudioClip? currentAudioClip;
        private SongDetails? currentSong;
        
        // Download waiting system
        private SongDetails? pendingPlaySong = null;
        private bool isWaitingForDownload = false;
        
        // Pre-loading system
        private bool isPreloadingNext = false;
        private const float PreloadTimeSeconds = 30f; // Start preloading 30 seconds before current track ends
        
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
        }
        
        public void Initialize(AudioSource audioSource)
        {
            this.audioSource = audioSource;
            LoggingSystem.Info("YouTubeStreamingController initialized with download-and-play system", "YouTubeStreaming");
        }
        
        public void SetPlaylist(YouTubePlaylist playlist)
        {
            currentPlaylist = playlist;
            currentPlaylist.OnPlaylistChanged += OnPlaylistUpdated;
            LoggingSystem.Info($"YouTube playlist set - {playlist.Count} tracks", "YouTubeStreaming");
            
            // Only queue the first song for immediate download to avoid downloading everything at once
            var allSongs = playlist.GetAllSongs();
            if (allSongs.Count > 0)
            {
                // Download only the first song immediately (the one likely to be played first)
                var firstSong = allSongs[0];
                downloadCache.QueueForBackgroundDownload(new List<SongDetails> { firstSong });
                LoggingSystem.Info($"Queued first song for immediate download: {firstSong.title}", "YouTubeStreaming");
                
                // If there are more songs, we'll download them based on proximity to current playback
                if (allSongs.Count > 1)
                {
                    LoggingSystem.Info($"Will download remaining {allSongs.Count - 1} songs on-demand as needed", "YouTubeStreaming");
                }
            }
            
            OnPlaylistChanged?.Invoke();
        }
        
        private void OnPlaylistUpdated()
        {
            LoggingSystem.Debug("YouTube playlist updated", "YouTubeStreaming");
            
            // Don't automatically queue all new songs for download - let them be downloaded on-demand
            // This prevents aggressive downloading when songs are added to playlist
            
            OnPlaylistChanged?.Invoke();
        }
        
        /// <summary>
        /// Play a specific track from the playlist
        /// </summary>
        public async Task<bool> PlayTrack(int trackIndex)
        {
            if (currentPlaylist == null || !currentPlaylist.SetCurrentTrack(trackIndex))
                return false;
                
            var songDetails = currentPlaylist.GetCurrentSong();
            if (songDetails == null)
                return false;
                
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
                
                // Start pre-loading next tracks
                isPreloadingNext = false; // Reset preloading flag
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
        
        /// <summary>
        /// Pre-load upcoming tracks in the playlist (conservative approach - only when near end of current track)
        /// </summary>
        private void PreloadUpcomingTracks()
        {
            if (isPreloadingNext || currentPlaylist == null || !isPlaying) return;
            
            // Only preload when current song is near the end
            var timeRemaining = TotalTime - CurrentTime;
            if (timeRemaining > PreloadTimeSeconds)
            {
                return; // Too early to preload
            }
            
            isPreloadingNext = true;
            
            try
            {
                var upcomingTracks = new List<SongDetails>();
                var currentIndex = currentPlaylist.CurrentTrackIndex;
                
                // Only preload the next 1-2 tracks (much more conservative)
                for (int i = 1; i <= 2; i++)
                {
                    var nextIndex = (currentIndex + i) % currentPlaylist.Count;
                    var nextSong = currentPlaylist.GetSong(nextIndex);
                    
                    if (nextSong != null && !downloadCache.IsSongCached(nextSong) && !nextSong.isDownloading)
                    {
                        upcomingTracks.Add(nextSong);
                        LoggingSystem.Info($"Queuing upcoming track for preload: {nextSong.title} (position {i})", "YouTubeStreaming");
                    }
                }
                
                if (upcomingTracks.Count > 0)
                {
                    LoggingSystem.Info($"Pre-loading {upcomingTracks.Count} upcoming tracks ({timeRemaining:F1}s remaining)", "YouTubeStreaming");
                    downloadCache.QueueForBackgroundDownload(upcomingTracks);
                }
                else
                {
                    LoggingSystem.Debug("No upcoming tracks need preloading", "YouTubeStreaming");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Warning($"Error pre-loading upcoming tracks: {ex.Message}", "YouTubeStreaming");
            }
            finally
            {
                isPreloadingNext = false;
            }
        }
        
        // Download event handlers
        private void OnSongDownloadStarted(SongDetails song)
        {
            LoggingSystem.Debug($"Download started: {song.title}", "YouTubeStreaming");
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
                
                // Auto-play the song using Unity-safe coroutine
                MelonCoroutines.Start(AutoPlayDownloadedSongCoroutine(song));
            }
            
            OnTrackChanged?.Invoke();
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
                
                // Try to play next track instead using Unity-safe coroutine
                MelonCoroutines.Start(NextTrackCoroutine());
            }
            
            OnTrackChanged?.Invoke();
        }
        
        /// <summary>
        /// Unity-safe coroutine for playing next track
        /// </summary>
        private IEnumerator NextTrackCoroutine()
        {
            LoggingSystem.Info("Attempting to skip to next track due to download failure", "YouTubeStreaming");
            
            if (currentPlaylist != null && currentPlaylist.NextTrack())
            {
                var nextSong = currentPlaylist.GetCurrentSong();
                if (nextSong != null)
                {
                    yield return LoadAndPlayCachedSongCoroutine(nextSong);
                }
                else
                {
                    LoggingSystem.Error("Next song is null", "YouTubeStreaming");
                }
            }
            else
            {
                LoggingSystem.Warning("No next track available", "YouTubeStreaming");
            }
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
        
        public bool NextTrack()
        {
            if (currentPlaylist == null) return false;
            
            if (currentPlaylist.NextTrack())
            {
                // Use Unity-safe coroutine for track switching
                MelonCoroutines.Start(LoadAndPlayCachedSongCoroutine(currentPlaylist.GetCurrentSong()));
                return true;
            }
            
            return false;
        }
        
        public bool PreviousTrack()
        {
            if (currentPlaylist == null) return false;
            
            if (currentPlaylist.PreviousTrack())
            {
                // Use Unity-safe coroutine for track switching
                MelonCoroutines.Start(LoadAndPlayCachedSongCoroutine(currentPlaylist.GetCurrentSong()));
                return true;
            }
            
            return false;
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
                LoggingSystem.Debug("YouTube track ended, advancing to next", "YouTubeStreaming");
                
                if (repeatMode == RepeatMode.RepeatOne)
                {
                    // Restart current track
                    audioSource.Play();
                }
                else
                {
                    // Auto advance to next track using Unity-safe method
                    NextTrack();
                }
            }
            
            // Check if we should start pre-loading upcoming tracks
            if (!isPreloadingNext && currentAudioClip != null && audioSource.clip != null)
            {
                float timeRemaining = audioSource.clip.length - audioSource.time;
                if (timeRemaining <= PreloadTimeSeconds && timeRemaining > 0)
                {
                    LoggingSystem.Debug($"Starting pre-load of upcoming tracks ({timeRemaining:F1}s remaining)", "YouTubeStreaming");
                    PreloadUpcomingTracks();
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
        
        public int GetTrackCount()
        {
            return currentPlaylist?.Count ?? 0;
        }
        
        public int CurrentTrackIndex
        {
            get => currentPlaylist?.CurrentTrackIndex ?? 0;
        }
        
        public List<(string title, string artist)> GetAllTracks()
        {
            return currentPlaylist?.GetAllTracksInfo() ?? new List<(string, string)>();
        }
        
        /// <summary>
        /// Get download and cache status
        /// </summary>
        public string GetCacheStatus()
        {
            return downloadCache.GetDownloadStatus();
        }
        
        public void Reset()
        {
            Stop();
            currentAudioClip = null;
            currentSong = null;
            isPreloadingNext = false;
            isLoading = false;
            
            if (currentPlaylist != null)
            {
                currentPlaylist.OnPlaylistChanged -= OnPlaylistUpdated;
            }
            
            LoggingSystem.Info("YouTubeStreamingController reset", "YouTubeStreaming");
        }
    }
} 