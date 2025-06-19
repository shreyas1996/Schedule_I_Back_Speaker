using System;
using System.IO;
using System.Reflection;

namespace BackSpeakerMod.NewBackend.Utils
{
    public static class YtDlpLoader
    {
        public static string YtDlpFileName => "yt-dlp.exe";
        public static string YtDlpExtractedPath { get; private set; } = Path.Combine("Mods", "BackSpeaker", "Tools", YtDlpFileName);
        public static string FFMPEGFileName => "ffmpeg.exe";
        public static string FFMPEGPath { get; private set; } = Path.Combine("Mods", "BackSpeaker", "Tools", FFMPEGFileName);
        public static string FFProbeFileName => "ffprobe.exe";
        public static string FFProbePath { get; private set; } = Path.Combine("Mods", "BackSpeaker", "Tools", FFProbeFileName);
        
        public static bool EnsureYtDlpPresent()
        {
            // First check if we already extracted it to Tools folder
            if (File.Exists(YtDlpExtractedPath))
            {
                NewLoggingSystem.Info($"yt-dlp.exe found in Tools folder: {YtDlpExtractedPath}", "YtDlpLoader");
                return true;
            }

            // Check if yt-dlp exists in system PATH (try running it)
            if (IsCommandAvailable("yt-dlp", new string[] { "--version" }))
            {
                NewLoggingSystem.Info("yt-dlp found in system PATH", "YtDlpLoader");
                YtDlpExtractedPath = "yt-dlp"; // Use system command
                return true;
            }

            NewLoggingSystem.Debug("yt-dlp not found in system PATH or Tools folder", "YtDlpLoader");

            // Create Tools directory if it doesn't exist
            var directory = Path.GetDirectoryName(YtDlpExtractedPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                NewLoggingSystem.Info($"Created Tools directory: {directory}", "YtDlpLoader");
            }

            // Extract from embedded resources
            // return ExtractEmbeddedExecutable("yt-dlp.exe", YtDlpExtractedPath);
            return false;
        }

        public static bool EnsureFFMPEGPresent()
        {
            // Check if ffmpeg.exe is present in the Tools folder
            if (File.Exists(FFMPEGPath))
            {
                NewLoggingSystem.Info($"ffmpeg.exe found in Tools folder: {FFMPEGPath}", "YtDlpLoader");
                return true;
            }

            // Check if ffmpeg exists in system PATH
            if (IsCommandAvailable("ffmpeg", new string[] { "-version" }))
            {
                NewLoggingSystem.Info("ffmpeg found in system PATH", "YtDlpLoader");
                FFMPEGPath = "ffmpeg"; // Use system command
                return true;
            }

            // Create Tools directory if it doesn't exist
            var directory = Path.GetDirectoryName(FFMPEGPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                NewLoggingSystem.Info($"Created Tools directory: {directory}", "YtDlpLoader");
            }

            // Extract from embedded resources
            // return ExtractEmbeddedExecutable("ffmpeg.exe", FFMPEGPath);
            return false;
        }

        public static bool EnsureFFProbePresent()
        {
            // Check if ffprobe.exe is present in the Tools folder
            if (File.Exists(FFProbePath))
            {
                NewLoggingSystem.Info($"ffprobe.exe found in Tools folder: {FFProbePath}", "YtDlpLoader"); 
                return true;
            }

            // Check if ffprobe exists in system PATH
            if (IsCommandAvailable("ffprobe", new string[] { "-version" }))
            {
                NewLoggingSystem.Info("ffprobe found in system PATH", "YtDlpLoader");
                FFProbePath = "ffprobe"; // Use system command
                return true;
            }
    
            // Create Tools directory if it doesn't exist   
            var directory = Path.GetDirectoryName(FFProbePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                NewLoggingSystem.Info($"Created Tools directory: {directory}", "YtDlpLoader");
            }   
            // Extract from embedded resources
            // return ExtractEmbeddedExecutable("ffprobe.exe", FFProbePath);
            return false;
        }
        
        
        private static bool IsCommandAvailable(string command, string[] arguments)
        {
            try
            {
                using (var process = new System.Diagnostics.Process())
                {
                    process.StartInfo.FileName = command;
                    process.StartInfo.Arguments = string.Join(" ", arguments);
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;
                    
                    process.Start();
                    process.WaitForExit(5000); // 5 second timeout
                    
                    return process.ExitCode == 0;
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Debug($"Command '{command}' not available in PATH: {ex.Message}", "YtDlpLoader");
                return false;
            }
        }
        
        /// <summary>
        /// Check if yt-dlp is available
        /// </summary>
        public static bool IsYtDlpAvailable()
        {
            try
            {
                if (!EnsureYtDlpPresent()) return false;
                return !string.IsNullOrEmpty(YtDlpExtractedPath) && File.Exists(YtDlpExtractedPath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Check if ffmpeg is available
        /// </summary>
        public static bool IsFfmpegAvailable()
        {
            try
            {
                if (!EnsureFFMPEGPresent()) return false;
                return !string.IsNullOrEmpty(FFMPEGPath) && File.Exists(FFMPEGPath);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get the yt-dlp executable path
        /// </summary>
        public static string GetYtDlpPath()
        {
            if (!IsYtDlpAvailable()) return null;
            return YtDlpExtractedPath;
        }
    }
}
