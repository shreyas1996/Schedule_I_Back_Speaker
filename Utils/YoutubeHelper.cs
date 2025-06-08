// Helper class for YouTube related operations with ytdlp

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackSpeakerMod.Utils;
using System.Diagnostics;
using UnityEngine;
using System.IO;
using BackSpeakerMod.Core.System;
using Newtonsoft.Json.Linq;
using System.Web;
using BackSpeakerMod.Core.Features.Audio;
using MelonLoader;
using System.Collections;
using System.Text.RegularExpressions;

namespace BackSpeakerMod.Utils
{
    public static class YoutubeHelper
    {
        /// <summary>
        /// Get the YouTube cache directory path (public for access from other modules)
        /// </summary>
        public static string GetYouTubeCacheDirectory()
        {
            try
            {
                // Use the game directory for cache
                var gameDirectory = Directory.GetCurrentDirectory();
                var cacheDirectory = Path.Combine(gameDirectory, "Mods", "BackSpeaker", "Cache", "YouTube");
                
                // Ensure directory exists
                if (!Directory.Exists(cacheDirectory))
                {
                    Directory.CreateDirectory(cacheDirectory);
                    LoggingSystem.Info($"Created YouTube cache directory: {cacheDirectory}", "YoutubeHelper");
                }
                
                return cacheDirectory;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error setting up YouTube cache directory: {ex.Message}", "YoutubeHelper");
                
                // Fallback to temp directory
                var tempDir = Path.Combine(Path.GetTempPath(), "BackSpeakerMod_Cache", "YouTube");
                try
                {
                    if (!Directory.Exists(tempDir))
                    {
                        Directory.CreateDirectory(tempDir);
                    }
                    LoggingSystem.Warning($"Using fallback cache directory: {tempDir}", "YoutubeHelper");
                    return tempDir;
                }
                catch
                {
                    LoggingSystem.Error("Failed to create fallback cache directory", "YoutubeHelper");
                    return "";
                }
            }
        }

        /// <summary>
        /// Get song details from YouTube URL using MelonCoroutines (safe for Unity)
        /// </summary>
        public static void GetSongDetails(string url, Action<List<SongDetails>> onComplete)
        {
            if (string.IsNullOrEmpty(url))
            {
                LoggingSystem.Error("URL is null or empty", "YoutubeHelper");
                onComplete?.Invoke(new List<SongDetails>());
                return;
            }

            LoggingSystem.Info("=== STARTING YOUTUBE SONG DETAILS FETCH ===", "YoutubeHelper");
            LoggingSystem.Info($"URL: {url}", "YoutubeHelper");

            // Start the coroutine for safe Unity execution
            MelonCoroutines.Start(GetSongDetailsCoroutine(url, onComplete));
        }

        /// <summary>
        /// Coroutine to get song details from YouTube URL (Unity-safe)
        /// </summary>
        private static IEnumerator GetSongDetailsCoroutine(string url, Action<List<SongDetails>> onComplete)
        {
            LoggingSystem.Info($"=== STARTING YOUTUBE SONG DETAILS FETCH ===", "YoutubeHelper");
            LoggingSystem.Info($"URL: {url}", "YoutubeHelper");

            var cacheDir = GetYouTubeCacheDirectory();
            if (string.IsNullOrEmpty(cacheDir))
            {
                LoggingSystem.Error("Failed to get cache directory", "YoutubeHelper");
                onComplete?.Invoke(new List<SongDetails>());
                yield break;
            }

            // Check if yt-dlp is available
            if (!EmbeddedYtDlpLoader.IsYtDlpAvailable())
            {
                LoggingSystem.Error("yt-dlp is not available", "YoutubeHelper");
                onComplete?.Invoke(new List<SongDetails>());
                yield break;
            }

            var ytDlpPath = EmbeddedYtDlpLoader.GetYtDlpPath();
            if (string.IsNullOrEmpty(ytDlpPath))
            {
                LoggingSystem.Error("Could not get yt-dlp path", "YoutubeHelper");
                onComplete?.Invoke(new List<SongDetails>());
                yield break;
            }

            // Build command arguments
            var arguments = BuildYtDlpArguments(url, cacheDir);
            LoggingSystem.Debug($"yt-dlp command: {ytDlpPath} {arguments}", "YoutubeHelper");

            // Execute yt-dlp process
            string processOutput = null;
            int exitCode = -1;
            bool processCompleted = false;

            // Start the process execution coroutine
            yield return MelonCoroutines.Start(ExecuteYtDlpProcessCoroutine(ytDlpPath, arguments, (output, code) => {
                processOutput = output;
                exitCode = code;
                processCompleted = true;
            }));

            // Wait for process completion
            while (!processCompleted)
            {
                yield return new UnityEngine.WaitForSeconds(0.1f);
            }

            // Process the results
            if (exitCode == 0 && !string.IsNullOrEmpty(processOutput))
            {
                LoggingSystem.Info("yt-dlp process completed successfully", "YoutubeHelper");
                var songDetails = ProcessYtDlpJsonOutput(processOutput);
                LoggingSystem.Info($"Processed {songDetails.Count} song details", "YoutubeHelper");
                onComplete?.Invoke(songDetails);
            }
            else
            {
                LoggingSystem.Error($"yt-dlp process failed with exit code: {exitCode}", "YoutubeHelper");
                LoggingSystem.Error($"Process output: {processOutput}", "YoutubeHelper");
                onComplete?.Invoke(new List<SongDetails>());
            }
        }

        /// <summary>
        /// Execute yt-dlp process using coroutines (Unity-safe)
        /// </summary>
        private static IEnumerator ExecuteYtDlpProcessCoroutine(string ytDlpPath, string arguments, Action<string?, int> onComplete)
        {
            LoggingSystem.Info("Starting yt-dlp process execution", "YoutubeHelper");
            
            Process? process = null;
            string output = "";
            string error = "";
            bool processStarted = false;
            bool processCompleted = false;
            int exitCode = -1;

            // Start the process
            try
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = ytDlpPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = GetYouTubeCacheDirectory()
                };

                process = new Process { StartInfo = processStartInfo };
                
                // Set up event handlers
                process.OutputDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        output += e.Data + Environment.NewLine;
                    }
                };
                
                process.ErrorDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        error += e.Data + Environment.NewLine;
                    }
                };
                
                process.Exited += (sender, e) => {
                    exitCode = process.ExitCode;
                    processCompleted = true;
                };

                process.EnableRaisingEvents = true;
                processStarted = process.Start();
                
                if (processStarted)
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    LoggingSystem.Info("yt-dlp process started successfully", "YoutubeHelper");
                }
                else
                {
                    LoggingSystem.Error("Failed to start yt-dlp process", "YoutubeHelper");
                    onComplete?.Invoke(null, -1);
                    yield break;
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error starting yt-dlp process: {ex.Message}", "YoutubeHelper");
                onComplete?.Invoke(null, -1);
                yield break;
            }

            // Wait for process completion
            float timeout = 60f; // 60 seconds timeout
            float elapsed = 0f;
            
            while (!processCompleted && elapsed < timeout)
            {
                yield return new UnityEngine.WaitForSeconds(0.5f);
                elapsed += 0.5f;
            }

            // Handle completion or timeout
            if (!processCompleted)
            {
                LoggingSystem.Warning("yt-dlp process timed out, killing process", "YoutubeHelper");
                try
                {
                    if (process != null && !process.HasExited)
                    {
                        process.Kill();
                    }
                }
                catch (Exception ex)
                {
                    LoggingSystem.Error($"Error killing timed out process: {ex.Message}", "YoutubeHelper");
                }
                onComplete?.Invoke(null, -1);
            }
            else
            {
                LoggingSystem.Info($"yt-dlp process completed with exit code: {exitCode}", "YoutubeHelper");
                
                // Combine output and error for processing
                var fullOutput = output;
                if (!string.IsNullOrEmpty(error))
                {
                    LoggingSystem.Warning($"yt-dlp stderr: {error}", "YoutubeHelper");
                    fullOutput += Environment.NewLine + error;
                }
                
                onComplete?.Invoke(fullOutput, exitCode);
            }

            // Cleanup
            try
            {
                process?.Dispose();
            }
            catch (Exception ex)
            {
                LoggingSystem.Warning($"Error disposing process: {ex.Message}", "YoutubeHelper");
            }
        }

        /// <summary>
        /// Process yt-dlp JSON output to handle both single objects and arrays
        /// </summary>
        private static List<SongDetails> ProcessYtDlpJsonOutput(string rawOutput)
        {
            try
            {
                if (string.IsNullOrEmpty(rawOutput))
                {
                    LoggingSystem.Warning("Raw output is null or empty", "YoutubeHelper");
                    return new List<SongDetails>();
                }
                
                rawOutput = rawOutput.Trim();
                LoggingSystem.Debug($"Processing JSON output (first 500 chars): {rawOutput.Substring(0, Math.Min(500, rawOutput.Length))}...", "YoutubeHelper");

                var songs = new List<SongDetails>();
                var lines = rawOutput.Split('\n');
                
                LoggingSystem.Debug($"Processing {lines.Length} lines of output", "YoutubeHelper");
                
                foreach (var line in lines)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            LoggingSystem.Debug("Skipping empty line", "YoutubeHelper");
                            continue;
                        }
                        
                        var trimmedLine = line.Trim();
                        LoggingSystem.Debug($"Processing line: {trimmedLine}", "YoutubeHelper");
                        
                        var fields = trimmedLine.Split('|');
                        if (fields.Length < 5)
                        {
                            LoggingSystem.Warning($"Line has insufficient fields ({fields.Length}/5): {trimmedLine}", "YoutubeHelper");
                            continue;
                        }
                        
                        // Create song with validation
                        var song = new SongDetails();
                        
                        // Title (field 0)
                        song.title = !string.IsNullOrEmpty(fields[0]) ? fields[0].Trim() : "Unknown Title";
                        
                        // Artist (field 1)
                        song.artist = !string.IsNullOrEmpty(fields[1]) ? fields[1].Trim() : "Unknown Artist";
                        
                        // Duration (field 2)
                        if (int.TryParse(fields[2], out var duration))
                        {
                            song.duration = duration;
                        }
                        else
                        {
                            LoggingSystem.Debug($"Could not parse duration: {fields[2]}", "YoutubeHelper");
                            song.duration = 0;
                        }
                        
                        // Thumbnail (field 3)
                        song.thumbnail = !string.IsNullOrEmpty(fields[3]) ? fields[3].Trim() : "";
                        
                        // URL (field 4)
                        song.url = !string.IsNullOrEmpty(fields[4]) ? fields[4].Trim() : "";
                        
                        if (string.IsNullOrEmpty(song.url))
                        {
                            LoggingSystem.Warning($"Song has no URL, skipping: {song.title}", "YoutubeHelper");
                            continue;
                        }
                        
                        // Extract video ID for caching
                        song.videoId = song.GetVideoId();
                        
                        songs.Add(song);
                        LoggingSystem.Debug($"Successfully processed song: {song.title} by {song.artist} ({song.GetFormattedDuration()})", "YoutubeHelper");
                    }
                    catch (Exception lineEx)
                    {
                        LoggingSystem.Warning($"Error processing line '{line}': {lineEx.Message}", "YoutubeHelper");
                        continue; // Skip this line and continue with others
                    }
                }
                
                LoggingSystem.Info($"Successfully processed {songs.Count} songs from {lines.Length} lines", "YoutubeHelper");
                return songs;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error processing yt-dlp JSON: {ex.Message}", "YoutubeHelper");
                LoggingSystem.Error($"Stack trace: {ex.StackTrace}", "YoutubeHelper");
                LoggingSystem.Debug($"Raw output that failed parsing: {rawOutput}", "YoutubeHelper");
                return new List<SongDetails>();
            }
        }

        public static async Task<string> GetStreamableUrl(string url)
        {
            // get stream url using yt-dlp -g command
            var ytDlpPath = EmbeddedYtDlpLoader.YtDlpExtractedPath;
            var command = "";
            if (url.Contains("list="))
            {
                if (url.Contains("index="))
                {
                    // get only the index from the url. Make sure we dont capture the &
                    var index = url.Split(new string[] { "index=" }, StringSplitOptions.None)[1].Split('&')[0];
                    command = $"-g --playlist-items {index} \"{url}\"";
                }
                else
                {
                    LoggingSystem.Error("Playlist url not supported", "YoutubeHelper");
                    return "";
                }
            }
            else
            {
                command = $"-g \"{url}\"";
            }
            var localCookies = Path.Combine(GetYouTubeCacheDirectory(), "cookies.txt");
            if(File.Exists(localCookies))
            {
                LoggingSystem.Debug("Using local cookies", "YoutubeHelper");
                command = $"-f bestaudio[ext=webm]/bestaudio[ext=mp3]/bestaudio --cookies \"{localCookies}\" " + command;
            }
            else
            {
                LoggingSystem.Debug("Using cookies from browser", "YoutubeHelper");
                command = "-f bestaudio[ext=webm]/bestaudio[ext=mp3]/bestaudio --cookies-from-browser chrome " + command;
            }
            var result = await Task.Run(() => {
                using (var process = new Process())
                {
                    process.StartInfo.FileName = ytDlpPath;
                    process.StartInfo.Arguments = command;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    process.Start();
                    process.WaitForExit(30000);
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();
                    LoggingSystem.Info($"yt-dlp stream url: {output}", "YoutubeHelper");
                    LoggingSystem.Info($"yt-dlp stream url error: {error}", "YoutubeHelper");
                    if(!string.IsNullOrEmpty(error) && error.Contains("ERROR"))
                    {
                        LoggingSystem.Error("Error getting stream url", "YoutubeHelper");
                        return "";
                    }
                    if(string.IsNullOrEmpty(output))
                    {
                        LoggingSystem.Error("No output from yt-dlp", "YoutubeHelper");
                        return "";
                    }
                    return output;
                }
            });
            return result;
        }

        /// <summary>
        /// Download a song from YouTube using MelonCoroutines (safe for Unity)
        /// </summary>
        public static void DownloadSong(SongDetails songDetails, Action<bool>? onComplete = null)
        {
            if (string.IsNullOrEmpty(songDetails?.url))
            {
                LoggingSystem.Error("Song URL is null or empty", "YoutubeHelper");
                onComplete?.Invoke(false);
                return;
            }

            LoggingSystem.Info($"Starting download for: {songDetails.title}", "YoutubeHelper");
            MelonCoroutines.Start(DownloadSongCoroutine(songDetails, onComplete));
        }

        /// <summary>
        /// Download song coroutine using MelonCoroutines (Unity-safe)
        /// </summary>
        private static IEnumerator DownloadSongCoroutine(SongDetails songDetails, Action<bool>? onComplete)
        {
            LoggingSystem.Info($"🎵 Starting download for: {songDetails.title} by {songDetails.GetArtist()}", "YoutubeHelper");

            var cacheDir = GetYouTubeCacheDirectory();
            if (string.IsNullOrEmpty(cacheDir))
            {
                LoggingSystem.Error("Failed to get cache directory", "YoutubeHelper");
                onComplete?.Invoke(false);
                yield break;
            }

            // Check if yt-dlp is available
            if (!EmbeddedYtDlpLoader.IsYtDlpAvailable())
            {
                LoggingSystem.Error("yt-dlp is not available", "YoutubeHelper");
                onComplete?.Invoke(false);
                yield break;
            }

            var ytDlpPath = EmbeddedYtDlpLoader.GetYtDlpPath();
            if (string.IsNullOrEmpty(ytDlpPath))
            {
                LoggingSystem.Error("Could not get yt-dlp path", "YoutubeHelper");
                onComplete?.Invoke(false);
                yield break;
            }

            var videoId = ExtractVideoId(songDetails.url ?? "");
            if (string.IsNullOrEmpty(videoId))
            {
                LoggingSystem.Error($"Could not extract video ID from URL: {songDetails.url}", "YoutubeHelper");
                onComplete?.Invoke(false);
                yield break;
            }

            // Build download arguments
            var arguments = BuildDownloadArguments(songDetails.url ?? "", cacheDir, videoId);
            LoggingSystem.Debug($"yt-dlp download command: {ytDlpPath} {arguments}", "YoutubeHelper");

            // Execute yt-dlp process
            string? processOutput = null;
            int exitCode = -1;
            bool processCompleted = false;

            // Start the process execution coroutine
            yield return MelonCoroutines.Start(ExecuteYtDlpProcessCoroutine(ytDlpPath, arguments, (output, code) => {
                processOutput = output;
                exitCode = code;
                processCompleted = true;
            }));

            // Wait for process completion
            while (!processCompleted)
            {
                yield return new UnityEngine.WaitForSeconds(0.1f);
            }

            // Check download success
            if (exitCode == 0)
            {
                // Verify the downloaded file exists
                var expectedFile = Path.Combine(cacheDir, $"{videoId}.mp3");
                if (File.Exists(expectedFile))
                {
                    LoggingSystem.Info($"✅ Successfully downloaded: {songDetails.title}", "YoutubeHelper");
                    onComplete?.Invoke(true);
                }
                else
                {
                    LoggingSystem.Warning($"Download reported success but file not found: {expectedFile}", "YoutubeHelper");
                    onComplete?.Invoke(false);
                }
            }
            else
            {
                LoggingSystem.Error($"❌ Download failed with exit code: {exitCode}", "YoutubeHelper");
                LoggingSystem.Error($"Process output: {processOutput}", "YoutubeHelper");
                onComplete?.Invoke(false);
            }
        }

        /// <summary>
        /// Build yt-dlp command arguments for downloading songs
        /// </summary>
        private static string BuildDownloadArguments(string url, string cacheDir, string videoId)
        {
            // Use video ID as filename for consistency with cache detection
            var outputTemplate = Path.Combine(cacheDir, $"{videoId}.%(ext)s");
            
            var command = $"-f \"bestaudio[ext=m4a]/bestaudio/best\" " +
                         $"--extract-audio --audio-format mp3 --audio-quality 0 " +
                         $"--output \"{outputTemplate}\" " +
                         $"--no-playlist \"{url}\"";

            // Handle cookies safely
            try
            {
                var localCookies = Path.Combine(cacheDir, "cookies.txt");
                if (File.Exists(localCookies))
                {
                    LoggingSystem.Debug("Using local cookies for download", "YoutubeHelper");
                    command = $"--cookies \"{localCookies}\" " + command;
                }
                else
                {
                    LoggingSystem.Debug("Using cookies from browser for download", "YoutubeHelper");
                    command = "--cookies-from-browser chrome " + command;
                }
            }
            catch (Exception cookieEx)
            {
                LoggingSystem.Warning($"Cookie setup failed for download, proceeding without: {cookieEx.Message}", "YoutubeHelper");
            }

            return command;
        }

        public static async Task<AudioClip> LoadSong(string url)
        {
            try
            {
                // Load the song from the cache folder
                var cacheFolder = GetYouTubeCacheDirectory();
                var songPath = Path.Combine(cacheFolder, url.Split('/').Last() + ".mp3");
                
                if (!File.Exists(songPath))
                {
                    LoggingSystem.Warning($"Song not found: {songPath}", "YoutubeHelper");
                    return null;
                }
                
                var song = await AudioHelper.LoadAudioFileAsync(songPath);
                return song;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error loading song: {ex.Message}", "YoutubeHelper");
                return null;
            }
        }

        /// <summary>
        /// Find downloaded MP3 file based on URL or video ID
        /// </summary>
        public static string FindDownloadedFile(string url)
        {
            try
            {
                var cacheDir = GetYouTubeCacheDirectory();
                if (!Directory.Exists(cacheDir))
                    return "";

                // Extract video ID from URL
                var videoId = ExtractVideoId(url);
                if (string.IsNullOrEmpty(videoId) || videoId == "unknown")
                {
                    LoggingSystem.Warning($"Could not extract video ID from URL: {url}", "YoutubeHelper");
                    return "";
                }

                LoggingSystem.Debug($"Searching for downloaded file with video ID: {videoId}", "YoutubeHelper");

                // Look for exact video ID match first (most reliable) - this should be the primary method now
                var exactFilename = $"{videoId}.mp3";
                var exactFilePath = Path.Combine(cacheDir, exactFilename);
                
                if (File.Exists(exactFilePath))
                {
                    LoggingSystem.Debug($"Found exact video ID match: {exactFilename}", "YoutubeHelper");
                    return exactFilePath;
                }

                // Fallback: search all MP3 files for video ID match (for legacy files)
                var mp3Files = Directory.GetFiles(cacheDir, "*.mp3");
                
                if (mp3Files.Length == 0)
                {
                    LoggingSystem.Debug("No MP3 files found in cache directory", "YoutubeHelper");
                    return "";
                }

                LoggingSystem.Debug($"Exact match not found, searching {mp3Files.Length} files for video ID pattern", "YoutubeHelper");

                // Look for files containing the video ID (for backward compatibility)
                var containsMatch = mp3Files.FirstOrDefault(f => 
                    Path.GetFileNameWithoutExtension(f).Contains(videoId));
                
                if (!string.IsNullOrEmpty(containsMatch))
                {
                    LoggingSystem.Debug($"Found file containing video ID: {Path.GetFileName(containsMatch)}", "YoutubeHelper");
                    return containsMatch;
                }

                LoggingSystem.Debug($"No cached file found for video ID: {videoId}", "YoutubeHelper");
                return "";
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error finding downloaded file: {ex.Message}", "YoutubeHelper");
                return "";
            }
        }
        
        /// <summary>
        /// Extract YouTube video ID from URL
        /// </summary>
        public static string ExtractVideoId(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                    return "";

                // Handle different YouTube URL formats
                var uri = new Uri(url);
                
                // Standard youtube.com/watch?v=VIDEO_ID
                if (uri.Host.Contains("youtube.com") && url.Contains("watch"))
                {
                    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    var videoId = query["v"];
                    if (!string.IsNullOrEmpty(videoId))
                        return videoId;
                }
                
                // Short youtu.be/VIDEO_ID format
                if (uri.Host.Contains("youtu.be"))
                {
                    var videoId = uri.AbsolutePath.TrimStart('/').Split('?')[0];
                    if (!string.IsNullOrEmpty(videoId))
                        return videoId;
                }
                
                // Try to extract 11-character video ID pattern from anywhere in URL
                var match = System.Text.RegularExpressions.Regex.Match(url, @"[a-zA-Z0-9_-]{11}");
                if (match.Success)
                    return match.Value;
                
                return "";
            }
            catch (Exception ex)
            {
                LoggingSystem.Warning($"Error extracting video ID from {url}: {ex.Message}", "YoutubeHelper");
                return "";
            }
        }

        /// <summary>
        /// Build yt-dlp command arguments for getting song details
        /// </summary>
        private static string BuildYtDlpArguments(string url, string cacheDir)
        {
            var command = "";
            
            // Check if the url is a playlist
            if (url.Contains("list="))
            {
                // Check if the url has index in it
                if (url.Contains("index="))
                {
                    // Get the index from the url. Make sure we dont capture the &
                    var index = url.Split(new string[] { "index=" }, StringSplitOptions.None)[1].Split('&')[0];
                    command = "--playlist-items " + index + " --print \"%(title)s|%(uploader)s|%(duration)s|%(thumbnail)s|%(webpage_url)s\" \"" + url + "\"";
                }
                else
                {
                    command = "--flat-playlist --print \"%(title)s|%(uploader)s|%(duration)s|%(thumbnail)s|%(webpage_url)s\" \"" + url + "\"";
                }
            }
            else
            {
                command = "--print \"%(title)s|%(uploader)s|%(duration)s|%(thumbnail)s|%(webpage_url)s\" \"" + url + "\"";
            }
            
            // Handle cookies safely
            try
            {
                var localCookies = Path.Combine(cacheDir, "cookies.txt");
                if (File.Exists(localCookies))
                {
                    LoggingSystem.Debug("Using local cookies", "YoutubeHelper");
                    command = $"--cookies \"{localCookies}\" " + command;
                }
                else
                {
                    LoggingSystem.Debug("Using cookies from browser", "YoutubeHelper");
                    command = "--cookies-from-browser chrome " + command;
                }
            }
            catch (Exception cookieEx)
            {
                LoggingSystem.Warning($"Cookie setup failed, proceeding without: {cookieEx.Message}", "YoutubeHelper");
            }
            
            return command;
        }

        /// <summary>
        /// Get the cache directory path
        /// </summary>
        private static string GetCacheDirectory()
        {
            return GetYouTubeCacheDirectory();
        }
    }
}