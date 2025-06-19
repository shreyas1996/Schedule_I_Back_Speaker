using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;

namespace BackSpeakerMod.NewBackend.Utils
{
    public static class NewYouTubeLoaderHelper
    {
        public static IEnumerator DownloadYouTubeAudio(string url, System.Action<bool, string> onComplete)
        {
            if (string.IsNullOrEmpty(url))
            {
                NewLoggingSystem.Warning("Empty YouTube URL provided", "NewYouTubeLoaderHelper");
                onComplete?.Invoke(false, null);
                yield break;
            }

            NewLoggingSystem.Debug($"Downloading YouTube audio: {url}", "NewYouTubeLoaderHelper");

            // Create NewSongDetails for the URL
            var songDetails = new NewSongDetails
            {
                url = url,
                title = "Unknown Title",
                artist = "Unknown Artist"
            };

            bool downloadCompleted = false;
            bool downloadSuccess = false;
            string downloadedFilePath = null;

            // Use NewBackend NewYoutubeHelper.DownloadSong method
            NewYoutubeHelper.DownloadSong(songDetails, null, (success) => {
                downloadCompleted = true;
                downloadSuccess = success;
                if (success)
                {
                    downloadedFilePath = NewYoutubeHelper.FindDownloadedFile(url);
                }
            });

            // Wait for download completion
            while (!downloadCompleted)
            {
                yield return new WaitForSeconds(0.5f);
            }

            if (downloadSuccess && !string.IsNullOrEmpty(downloadedFilePath))
            {
                NewLoggingSystem.Info($"✓ YouTube audio downloaded: {downloadedFilePath}", "NewYouTubeLoaderHelper");
            }
            else
            {
                NewLoggingSystem.Warning($"Failed to download YouTube audio from: {url}", "NewYouTubeLoaderHelper");
            }

            onComplete?.Invoke(downloadSuccess, downloadedFilePath);
        }

        public static IEnumerator GetYouTubeVideoInfo(string url, System.Action<NewSongDetails> onComplete)
        {
            if (string.IsNullOrEmpty(url))
            {
                NewLoggingSystem.Warning("Empty YouTube URL provided", "NewYouTubeLoaderHelper");
                onComplete?.Invoke(null);
                yield break;
            }

            NewLoggingSystem.Debug($"Getting YouTube video info: {url}", "NewYouTubeLoaderHelper");

            bool infoCompleted = false;
            List<NewSongDetails> songDetailsList = null;

            // Use NewBackend NewYoutubeHelper.GetSongDetails method
            NewYoutubeHelper.GetSongDetails(url, (results) => {
                infoCompleted = true;
                songDetailsList = results;
            });

            // Wait for info completion
            while (!infoCompleted)
            {
                yield return new WaitForSeconds(0.5f);
            }

            NewSongDetails songDetails = null;
            if (songDetailsList != null && songDetailsList.Count > 0)
            {
                songDetails = songDetailsList[0]; // Get first result
            }

            if (songDetails != null)
            {
                NewLoggingSystem.Info($"✓ YouTube video info retrieved: {songDetails.title}", "NewYouTubeLoaderHelper");
            }
            else
            {
                NewLoggingSystem.Warning($"Failed to get YouTube video info from: {url}", "NewYouTubeLoaderHelper");
            }

            onComplete?.Invoke(songDetails);
        }

        public static IEnumerator SearchYouTube(string query, int maxResults, System.Action<List<NewSongDetails>> onComplete)
        {
            if (string.IsNullOrEmpty(query))
            {
                NewLoggingSystem.Warning("Empty search query provided", "NewYouTubeLoaderHelper");
                onComplete?.Invoke(new List<NewSongDetails>());
                yield break;
            }

            NewLoggingSystem.Debug($"Searching YouTube: {query}", "NewYouTubeLoaderHelper");

            // Create search URL (ytsearch:query format for yt-dlp)
            string searchUrl = $"ytsearch{maxResults}:{query}";
            
            bool searchCompleted = false;
            List<NewSongDetails> searchResults = null;

            // Use NewBackend NewYoutubeHelper.GetSongDetails method with search URL
            NewYoutubeHelper.GetSongDetails(searchUrl, (results) => {
                searchCompleted = true;
                searchResults = results;
            });

            // Wait for search completion
            while (!searchCompleted)
            {
                yield return new WaitForSeconds(0.5f);
            }

            List<NewSongDetails> results = searchResults ?? new List<NewSongDetails>();
            NewLoggingSystem.Info($"✓ YouTube search completed: {results.Count} results", "NewYouTubeLoaderHelper");
            onComplete?.Invoke(results);
        }

        public static bool IsYtDlpAvailable()
        {
            return NewYtDlpLoader.IsYtDlpAvailable();
        }

        public static bool IsFfmpegAvailable()
        {
            return NewYtDlpLoader.IsFfmpegAvailable();
        }

        public static bool IsFfprobeAvailable()
        {
            // Check if ffprobe exists alongside ffmpeg
            return NewYtDlpLoader.IsFfmpegAvailable();
        }

        public static bool CheckAllDependencies()
        {
            return IsYtDlpAvailable() && IsFfmpegAvailable();
        }

        public static IEnumerator InitializeDependencies(System.Action<bool> onComplete)
        {
            NewLoggingSystem.Info("Initializing YouTube dependencies...", "NewYouTubeLoaderHelper");

            // Initialize yt-dlp
            bool ytDlpInitialized = NewYtDlpLoader.IsYtDlpAvailable();
            bool ffmpegInitialized = NewYtDlpLoader.IsFfmpegAvailable();

            bool success = ytDlpInitialized && ffmpegInitialized;

            if (success)
            {
                NewLoggingSystem.Info("✓ YouTube dependencies initialized successfully", "NewYouTubeLoaderHelper");
            }
            else
            {
                NewLoggingSystem.Warning($"Failed to initialize YouTube dependencies - yt-dlp: {ytDlpInitialized}, ffmpeg: {ffmpegInitialized}", "NewYouTubeLoaderHelper");
            }

            onComplete?.Invoke(success);
            yield break;
        }
    }
} 