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
    /// Simplified YouTube streaming controller - only plays downloaded songs
    /// No auto-download logic - downloads are managed separately
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

        // Events
        public Action? OnTrackChanged;

        public YouTubeStreamingController()
        {
            downloadCache = new YouTubeDownloadCache();
        }
        
        public void Initialize(AudioSource audioSource)
        {
            this.audioSource = audioSource;
            LoggingSystem.Info("YouTubeStreamingController initialized (download-first mode)", "YouTubeStreaming");
        }
        
        /// <summary>
        /// Play a specific song - only works if song is already downloaded
        /// </summary>
        public async Task<bool> PlaySong(SongDetails songDetails)
        {
            if (songDetails == null)
                return false;
                
            // Only play if song is already downloaded
            if (!downloadCache.IsSongCached(songDetails) || !songDetails.IsReadyToPlay())
            {
                LoggingSystem.Warning($"Cannot play song - not downloaded: {songDetails.title}", "YouTubeStreaming");
                return false;
            }
            
            currentSong = songDetails;
            isLoading = true;
            LoggingSystem.Info($"🎵 Playing downloaded YouTube song: {songDetails.title} by {songDetails.GetArtist()}", "YouTubeStreaming");
            
            try
            {
                return await LoadAndPlayCachedSong(songDetails);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error playing YouTube track {songDetails.title}: {ex.Message}", "YouTubeStreaming");
                isLoading = false;
                return false;
            }
        }
        
        /// <summary>
        /// Check if a song is ready to play (downloaded and cached)
        /// </summary>
        public bool IsSongReadyToPlay(SongDetails songDetails)
        {
            return songDetails != null && 
                   downloadCache.IsSongCached(songDetails) && 
                   songDetails.IsReadyToPlay();
        }
        
        /// <summary>
        /// Legacy method for compatibility - converts to PlaySong
        /// </summary>
        public async Task<bool> PlayTrack(int trackIndex)
        {
            // This method is kept for compatibility but should not be used
            // in the new download-first approach
            LoggingSystem.Warning("PlayTrack(int) called - this method is deprecated in download-first mode", "YouTubeStreaming");
            return false;
        }
        
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
                
                // Notify UI that track changed
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
        public float CurrentVolume => volume;
        public float CurrentTime => audioSource?.time ?? 0f;
        public float TotalTime => audioSource?.clip?.length ?? 0f;
        public float Progress => TotalTime > 0 ? CurrentTime / TotalTime : 0f;
        public bool IsAudioReady => audioSource != null && !isLoading;
        public RepeatMode RepeatMode { get => repeatMode; set => repeatMode = value; }
        
        public string GetCurrentTrackInfo()
        {
            return currentSong?.title ?? "No track selected";
        }
        
        public string GetCurrentArtistInfo()
        {
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