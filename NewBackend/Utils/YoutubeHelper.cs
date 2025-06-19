using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using MelonLoader;
using Newtonsoft.Json.Linq;

namespace BackSpeakerMod.NewBackend.Utils
{
    public static class NewYoutubeHelper
    {
        public static string GetYouTubeCacheDirectory()
        {
            try
            {
                var gameDirectory = Directory.GetCurrentDirectory();
                var cacheDirectory = Path.Combine(gameDirectory, "Mods", "BackSpeaker", "Cache", "YouTube");

                if (!Directory.Exists(cacheDirectory))
                {
                    Directory.CreateDirectory(cacheDirectory);
                    NewLoggingSystem.Info($"Created YouTube cache directory: {cacheDirectory}", "NewYoutubeHelper");
                }

                return cacheDirectory;
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error setting up YouTube cache directory: {ex.Message}", "NewYoutubeHelper");
                var tempDir = Path.Combine(Path.GetTempPath(), "BackSpeakerMod_Cache", "YouTube");
                try
                {
                    if (!Directory.Exists(tempDir))
                    {
                        Directory.CreateDirectory(tempDir);
                    }
                    NewLoggingSystem.Warning($"Using fallback cache directory: {tempDir}", "NewYoutubeHelper");
                    return tempDir;
                }
                catch
                {
                    NewLoggingSystem.Error("Failed to create fallback cache directory", "NewYoutubeHelper");
                    return "";
                }
            }
        }

        public static void GetSongDetails(string url, System.Action<List<NewSongDetails>> onComplete)
        {
            if (string.IsNullOrEmpty(url))
            {
                NewLoggingSystem.Error("URL is null or empty", "NewYoutubeHelper");
                onComplete?.Invoke(new List<NewSongDetails>());
                return;
            }

            NewLoggingSystem.Info("=== STARTING YOUTUBE SONG DETAILS FETCH ===", "NewYoutubeHelper");
            NewLoggingSystem.Info($"URL: {url}", "NewYoutubeHelper");

            MelonCoroutines.Start(GetSongDetailsCoroutine(url, onComplete));
        }

        private static IEnumerator GetSongDetailsCoroutine(string url, System.Action<List<NewSongDetails>> onComplete)
        {
            var cacheDir = GetYouTubeCacheDirectory();
            if (string.IsNullOrEmpty(cacheDir))
            {
                NewLoggingSystem.Error("Failed to get cache directory", "NewYoutubeHelper");
                onComplete?.Invoke(new List<NewSongDetails>());
                yield break;
            }

            if (!NewYtDlpLoader.IsYtDlpAvailable())
            {
                NewLoggingSystem.Error("yt-dlp is not available", "NewYoutubeHelper");
                onComplete?.Invoke(new List<NewSongDetails>());
                yield break;
            }

            var ytDlpPath = NewYtDlpLoader.GetYtDlpPath();
            if (string.IsNullOrEmpty(ytDlpPath))
            {
                NewLoggingSystem.Error("Could not get yt-dlp path", "NewYoutubeHelper");
                onComplete?.Invoke(new List<NewSongDetails>());
                yield break;
            }

            var arguments = BuildYtDlpArguments(url, cacheDir);
            NewLoggingSystem.Debug($"yt-dlp command: {ytDlpPath} {arguments}", "NewYoutubeHelper");

            string processOutput = null;
            int exitCode = -1;
            bool processCompleted = false;

            yield return MelonCoroutines.Start(ExecuteYtDlpProcessCoroutine(ytDlpPath, arguments, (output, code) => {
                processOutput = output;
                exitCode = code;
                processCompleted = true;
            }));

            while (!processCompleted)
            {
                yield return new WaitForSeconds(0.1f);
            }

            if (exitCode == 0 && !string.IsNullOrEmpty(processOutput))
            {
                NewLoggingSystem.Info("yt-dlp process completed successfully", "NewYoutubeHelper");
                var songDetails = ProcessYtDlpJsonOutput(processOutput);
                NewLoggingSystem.Info($"Processed {songDetails.Count} song details", "NewYoutubeHelper");
                onComplete?.Invoke(songDetails);
            }
            else
            {
                NewLoggingSystem.Error($"yt-dlp process failed with exit code: {exitCode}", "NewYoutubeHelper");
                onComplete?.Invoke(new List<NewSongDetails>());
            }
        }

        public static void DownloadSong(NewSongDetails songDetails, System.Action<string> onDownloadProgressChanged, System.Action<bool> onComplete)
        {
            if (songDetails == null || string.IsNullOrEmpty(songDetails.url))
            {
                NewLoggingSystem.Error("Invalid song details for download", "NewYoutubeHelper");
                onComplete?.Invoke(false);
                return;
            }

            MelonCoroutines.Start(DownloadSongCoroutine(songDetails, onDownloadProgressChanged, onComplete));
        }

        public static string FindDownloadedFile(string url)
        {
            try
            {
                var cacheDir = GetYouTubeCacheDirectory();
                var videoId = ExtractVideoId(url);
                
                if (string.IsNullOrEmpty(videoId))
                    return "";

                var possibleExtensions = new[] { ".mp3", ".m4a", ".webm", ".opus" };
                
                foreach (var ext in possibleExtensions)
                {
                    var filePath = Path.Combine(cacheDir, $"{videoId}{ext}");
                    if (File.Exists(filePath))
                    {
                        return filePath;
                    }
                }

                if (Directory.Exists(cacheDir))
                {
                    var files = Directory.GetFiles(cacheDir, $"{videoId}*");
                    foreach (var file in files)
                    {
                        var ext = Path.GetExtension(file).ToLower();
                        if (possibleExtensions.Contains(ext))
                        {
                            return file;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error finding downloaded file: {ex}", "NewYoutubeHelper");
            }

            return "";
        }

        public static string ExtractVideoId(string url)
        {
            try
            {
                if (string.IsNullOrEmpty(url)) return "";

                if (url.Contains("youtube.com/watch?v="))
                {
                    var startIndex = url.IndexOf("v=") + 2;
                    var endIndex = url.IndexOf("&", startIndex);
                    if (endIndex == -1) endIndex = url.Length;
                    return url.Substring(startIndex, endIndex - startIndex);
                }
                else if (url.Contains("youtu.be/"))
                {
                    var startIndex = url.LastIndexOf("/") + 1;
                    var endIndex = url.IndexOf("?", startIndex);
                    if (endIndex == -1) endIndex = url.Length;
                    return url.Substring(startIndex, endIndex - startIndex);
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error extracting video ID from URL: {ex}", "NewYoutubeHelper");
            }

            return "";
        }

        private static IEnumerator ExecuteYtDlpProcessCoroutine(string ytDlpPath, string arguments, System.Action<string, int> onComplete)
        {
            Process process = null;
            string output = "";
            bool processCompleted = false;
            int exitCode = -1;

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
                
                process.OutputDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        output += e.Data + Environment.NewLine;
                    }
                };

                process.Exited += (sender, e) => {
                    exitCode = process.ExitCode;
                    processCompleted = true;
                };

                process.EnableRaisingEvents = true;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                NewLoggingSystem.Info("yt-dlp process started", "NewYoutubeHelper");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to start yt-dlp process: {ex}", "NewYoutubeHelper");
                processCompleted = true;
                exitCode = -1;
            }

            while (!processCompleted)
            {
                yield return new WaitForSeconds(0.1f);
            }

            try
            {
                process?.Close();
                process?.Dispose();
            }
            catch { }

            onComplete?.Invoke(output, exitCode);
        }

        private static IEnumerator DownloadSongCoroutine(NewSongDetails songDetails, System.Action<string> onDownloadProgressChanged, System.Action<bool> onComplete)
        {
            var cacheDir = GetYouTubeCacheDirectory();
            var ytDlpPath = NewYtDlpLoader.GetYtDlpPath();
            var videoId = ExtractVideoId(songDetails.url);
            var arguments = BuildDownloadArguments(songDetails.url, cacheDir, videoId);

            bool downloadCompleted = false;
            bool downloadSuccess = false;

            yield return MelonCoroutines.Start(ExecuteDownloadProcessCoroutine(ytDlpPath, arguments, onDownloadProgressChanged, (success) => {
                downloadCompleted = true;
                downloadSuccess = success;
            }));

            while (!downloadCompleted)
            {
                yield return new WaitForSeconds(0.1f);
            }

            if (downloadSuccess)
            {
                var downloadedFile = FindDownloadedFile(songDetails.url);
                if (!string.IsNullOrEmpty(downloadedFile))
                {
                    songDetails.cachedFilePath = downloadedFile;
                    songDetails.isDownloaded = true;
                }
            }

            onComplete?.Invoke(downloadSuccess);
        }

        private static IEnumerator ExecuteDownloadProcessCoroutine(string ytDlpPath, string arguments, System.Action<string> onProgress, System.Action<bool> onComplete)
        {
            Process process = null;
            bool processCompleted = false;
            int exitCode = -1;

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
                
                process.OutputDataReceived += (sender, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        if (e.Data.Contains("[download]") && e.Data.Contains("%"))
                        {
                            onProgress?.Invoke(e.Data);
                        }
                    }
                };

                process.Exited += (sender, e) => {
                    exitCode = process.ExitCode;
                    processCompleted = true;
                };

                process.EnableRaisingEvents = true;
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to start download process: {ex}", "NewYoutubeHelper");
                processCompleted = true;
                exitCode = -1;
            }

            while (!processCompleted)
            {
                yield return new WaitForSeconds(0.1f);
            }

            try
            {
                process?.Close();
                process?.Dispose();
            }
            catch { }

            onComplete?.Invoke(exitCode == 0);
        }

        private static string BuildYtDlpArguments(string url, string cacheDir)
        {
            return $"--dump-json --no-download --flat-playlist \"{url}\"";
        }

        private static string BuildDownloadArguments(string url, string cacheDir, string videoId)
        {
            return $"-x --audio-format mp3 --audio-quality 0 -o \"{Path.Combine(cacheDir, "%(id)s.%(ext)s")}\" \"{url}\"";
        }

        private static List<NewSongDetails> ProcessYtDlpJsonOutput(string rawOutput)
        {
            var songDetailsList = new List<NewSongDetails>();

            try
            {
                var lines = rawOutput.Split('\n');
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    try
                    {
                        var json = JObject.Parse(line);
                        var songDetails = new NewSongDetails
                        {
                            title = json["title"]?.ToString() ?? "Unknown Title",
                            artist = json["uploader"]?.ToString() ?? "Unknown Artist",
                            url = json["webpage_url"]?.ToString() ?? json["url"]?.ToString() ?? "",
                            duration = (int)(json["duration"]?.ToObject<double>() ?? 0),
                            isDownloaded = false
                        };

                        if (!string.IsNullOrEmpty(songDetails.url))
                        {
                            songDetailsList.Add(songDetails);
                        }
                    }
                    catch (Exception ex)
                    {
                        NewLoggingSystem.Debug($"Failed to parse JSON line: {ex.Message}", "NewYoutubeHelper");
                    }
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error processing yt-dlp JSON output: {ex}", "NewYoutubeHelper");
            }

            return songDetailsList;
        }
    }
} 