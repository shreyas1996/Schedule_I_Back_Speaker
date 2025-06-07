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

namespace BackSpeakerMod.Utils
{
    public class YoutubeHelper
    {
        private static string GetYouTubeCacheDirectory()
        {
            var gameDirectory = Directory.GetCurrentDirectory();
            var cacheDir = Path.Combine(gameDirectory, "Mods", "BackSpeaker", "Cache", "YouTube");
            
            if (!Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
                LoggingSystem.Info($"Created YouTube cache directory: {cacheDir}", "YoutubeHelper");
            }
            
            return cacheDir;
        }

        public static async void GetSongDetails(string url, Action<List<SongDetails>> onComplete)
        {
            try
            {
                LoggingSystem.Info("=== STARTING YOUTUBE SONG DETAILS FETCH ===", "YoutubeHelper");
                LoggingSystem.Info($"URL: {url}", "YoutubeHelper");
                
                // Run the yt-dlp process on a background thread with real-time logging
                var result = await Task.Run(() => {
                    try
                    {
                        // Ensure yt-dlp is available
                        if (!EmbeddedYtDlpLoader.EnsureYtDlpPresent() || !EmbeddedYtDlpLoader.EnsureFFMPEGPresent())
                        {
                            LoggingSystem.Error("yt-dlp not available", "YoutubeHelper");
                            return new List<SongDetails>();
                        }

                        var ytDlpPath = EmbeddedYtDlpLoader.YtDlpExtractedPath;
                        var command = $"--print \"%(title)s|%(uploader)s|%(duration)s|%(thumbnail)s|%(webpage_url)s\" \"{url}\"";
                        
                        LoggingSystem.Info($"=== EXECUTING YT-DLP COMMAND ===", "YoutubeHelper");
                        LoggingSystem.Info($"Executable: {ytDlpPath}", "YoutubeHelper");
                        LoggingSystem.Info($"Arguments: {command}", "YoutubeHelper");
                        LoggingSystem.Info($"Full command: \"{ytDlpPath}\" {command}", "YoutubeHelper");

                        using (var process = new Process())
                        {
                            process.StartInfo.FileName = ytDlpPath;
                            process.StartInfo.Arguments = command;
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.RedirectStandardOutput = true;
                            process.StartInfo.RedirectStandardError = true;
                            process.StartInfo.CreateNoWindow = true;
                            
                            var outputBuilder = new StringBuilder();
                            var errorBuilder = new StringBuilder();
                            
                            // Real-time output logging
                            process.OutputDataReceived += (sender, e) => {
                                if (!string.IsNullOrEmpty(e.Data))
                                {
                                    LoggingSystem.Info($"[yt-dlp stdout] {e.Data}", "YoutubeHelper");
                                    outputBuilder.AppendLine(e.Data);
                                }
                            };
                            
                            process.ErrorDataReceived += (sender, e) => {
                                if (!string.IsNullOrEmpty(e.Data))
                                {
                                    LoggingSystem.Info($"[yt-dlp stderr] {e.Data}", "YoutubeHelper");
                                    errorBuilder.AppendLine(e.Data);
                                }
                            };

                            LoggingSystem.Info("Starting yt-dlp process...", "YoutubeHelper");
                            process.Start();
                            
                            // Begin async reading
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();
                            
                            // Wait for completion with timeout
                            bool finished = process.WaitForExit(30000); // 30 second timeout
                            
                            if (!finished)
                            {
                                LoggingSystem.Error("yt-dlp process timed out after 30 seconds", "YoutubeHelper");
                                try { process.Kill(); } catch { }
                                return new List<SongDetails>();
                            }

                            var output = outputBuilder.ToString();
                            var error = errorBuilder.ToString();
                            
                            LoggingSystem.Info($"yt-dlp process completed with exit code: {process.ExitCode}", "YoutubeHelper");
                            
                            if (!string.IsNullOrEmpty(error))
                                LoggingSystem.Warning($"yt-dlp stderr output: {error}", "YoutubeHelper");

                            if (string.IsNullOrEmpty(output) || output.Trim() == "")
                            {
                                LoggingSystem.Error("No output from yt-dlp", "YoutubeHelper");
                                return new List<SongDetails>();
                            }

                            LoggingSystem.Info($"Raw yt-dlp output length: {output.Length} characters", "YoutubeHelper");
                            
                            // Handle both single objects and arrays from yt-dlp
                            var processedJson = ProcessYtDlpJsonOutput(output);
                            LoggingSystem.Info("=== YOUTUBE SONG DETAILS FETCH COMPLETED ===", "YoutubeHelper");
                            return processedJson;
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingSystem.Error($"Error getting song details: {ex.Message}", "YoutubeHelper");
                        LoggingSystem.Error($"Stack trace: {ex.StackTrace}", "YoutubeHelper");
                        return new List<SongDetails>();
                    }
                });
                
                onComplete?.Invoke(result);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error in async song details: {ex.Message}", "YoutubeHelper");
                onComplete?.Invoke(new List<SongDetails>());
            }
        }

        /// <summary>
        /// Process yt-dlp JSON output to handle both single objects and arrays
        /// </summary>
        private static List<SongDetails> ProcessYtDlpJsonOutput(string rawOutput)
        {
            try
            {
                rawOutput = rawOutput.Trim();
                LoggingSystem.Debug($"Processing JSON output (first 500 chars): {rawOutput.Substring(0, Math.Min(500, rawOutput.Length))}...", "YoutubeHelper");

                var songs = new List<SongDetails>();
                foreach (var line in rawOutput.Split('\n'))
                {
                    var fields = line.Split('|');
                    if (fields.Length < 5) continue;
                    var song = new SongDetails
                    {
                        title = fields[0],
                        artist = fields[1],
                        duration = int.TryParse(fields[2], out var d) ? d : 0,
                        thumbnail = fields[3],
                        url = fields[4]
                    };
                    songs.Add(song);
                }
                return songs;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error processing yt-dlp JSON: {ex.Message}", "YoutubeHelper");
                LoggingSystem.Debug($"Raw output that failed parsing: {rawOutput}", "YoutubeHelper");
                return new List<SongDetails>();
            }
        }

        public static async void DownloadSong(string url, Action<string> onComplete)
        {
            try
            {
                LoggingSystem.Info("=== STARTING YOUTUBE SONG DOWNLOAD ===", "YoutubeHelper");
                LoggingSystem.Info($"URL: {url}", "YoutubeHelper");
                
                // Run the yt-dlp download process on a background thread with real-time logging
                var result = await Task.Run(() => {
                    try
                    {
                        // Ensure yt-dlp is available
                        if (!EmbeddedYtDlpLoader.EnsureYtDlpPresent() || !EmbeddedYtDlpLoader.EnsureFFMPEGPresent())
                        {
                            LoggingSystem.Error("yt-dlp not available", "YoutubeHelper");
                            return "";
                        }

                        var cacheDir = GetYouTubeCacheDirectory();
                        var outputTemplate = Path.Combine(cacheDir, "%(title)s.%(ext)s");
                        var ytDlpPath = EmbeddedYtDlpLoader.YtDlpExtractedPath;
                        var command = $"-f bestaudio --extract-audio --audio-format mp3 -o \"{outputTemplate}\" \"{url}\"";
                        
                        LoggingSystem.Info($"=== EXECUTING YT-DLP DOWNLOAD COMMAND ===", "YoutubeHelper");
                        LoggingSystem.Info($"Executable: {ytDlpPath}", "YoutubeHelper");
                        LoggingSystem.Info($"Arguments: {command}", "YoutubeHelper");
                        LoggingSystem.Info($"Full command: \"{ytDlpPath}\" {command}", "YoutubeHelper");
                        LoggingSystem.Info($"Output directory: {cacheDir}", "YoutubeHelper");
                        
                        using (var process = new Process())
                        {
                            process.StartInfo.FileName = ytDlpPath;
                            process.StartInfo.Arguments = command;
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.RedirectStandardOutput = true;
                            process.StartInfo.RedirectStandardError = true;
                            process.StartInfo.CreateNoWindow = true;
                            
                            var outputBuilder = new StringBuilder();
                            var errorBuilder = new StringBuilder();
                            
                            // Real-time output logging
                            process.OutputDataReceived += (sender, e) => {
                                if (!string.IsNullOrEmpty(e.Data))
                                {
                                    LoggingSystem.Info($"[yt-dlp download] {e.Data}", "YoutubeHelper");
                                    outputBuilder.AppendLine(e.Data);
                                }
                            };
                            
                            process.ErrorDataReceived += (sender, e) => {
                                if (!string.IsNullOrEmpty(e.Data))
                                {
                                    LoggingSystem.Info($"[yt-dlp download stderr] {e.Data}", "YoutubeHelper");
                                    errorBuilder.AppendLine(e.Data);
                                }
                            };

                            LoggingSystem.Info("Starting yt-dlp download process...", "YoutubeHelper");
                            process.Start();
                            
                            // Begin async reading
                            process.BeginOutputReadLine();
                            process.BeginErrorReadLine();
                            
                            // Wait for completion with longer timeout for downloads
                            bool finished = process.WaitForExit(120000); // 2 minute timeout
                            
                            if (!finished)
                            {
                                LoggingSystem.Error("yt-dlp download process timed out after 2 minutes", "YoutubeHelper");
                                try { process.Kill(); } catch { }
                                return "";
                            }

                            var output = outputBuilder.ToString();
                            var error = errorBuilder.ToString();
                            
                            LoggingSystem.Info($"yt-dlp download completed with exit code: {process.ExitCode}", "YoutubeHelper");
                            
                            if (!string.IsNullOrEmpty(error))
                                LoggingSystem.Warning($"yt-dlp download stderr: {error}", "YoutubeHelper");

                            LoggingSystem.Info($"yt-dlp download output: {output}", "YoutubeHelper");
                            
                            // Check if any files were actually downloaded
                            var downloadedFiles = Directory.GetFiles(cacheDir, "*.mp3");
                            LoggingSystem.Info($"Found {downloadedFiles.Length} MP3 files in cache after download", "YoutubeHelper");
                            
                            foreach (var file in downloadedFiles)
                            {
                                var fileInfo = new FileInfo(file);
                                LoggingSystem.Info($"Downloaded file: {Path.GetFileName(file)} ({fileInfo.Length} bytes)", "YoutubeHelper");
                            }
                            
                            LoggingSystem.Info("=== YOUTUBE SONG DOWNLOAD COMPLETED ===", "YoutubeHelper");
                            return output;
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggingSystem.Error($"Error downloading song: {ex.Message}", "YoutubeHelper");
                        LoggingSystem.Error($"Stack trace: {ex.StackTrace}", "YoutubeHelper");
                        return "";
                    }
                });
                
                onComplete?.Invoke(result);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error in async download: {ex.Message}", "YoutubeHelper");
                onComplete?.Invoke("");
            }
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

                // Get all MP3 files in cache directory
                var mp3Files = Directory.GetFiles(cacheDir, "*.mp3");
                
                if (mp3Files.Length == 0)
                    return "";

                // Return the most recent file (simple approach)
                // In a production system, you'd want to match by video ID or title
                var mostRecent = mp3Files.OrderByDescending(f => File.GetLastWriteTime(f)).FirstOrDefault();
                return mostRecent ?? "";
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error finding downloaded file: {ex.Message}", "YoutubeHelper");
                return "";
            }
        }
    }
}