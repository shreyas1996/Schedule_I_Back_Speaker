using System;
using System.IO;
using System.Reflection;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.Utils
{
    public static class EmbeddedYtDlpLoader
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
                LoggingSystem.Info($"yt-dlp.exe found in Tools folder: {YtDlpExtractedPath}", "EmbeddedYtDlpLoader");
                return true;
            }

            // Check if yt-dlp exists in system PATH (try running it)
            if (IsCommandAvailable("yt-dlp"))
            {
                LoggingSystem.Info("yt-dlp found in system PATH", "EmbeddedYtDlpLoader");
                YtDlpExtractedPath = "yt-dlp"; // Use system command
                return true;
            }

            LoggingSystem.Debug("yt-dlp not found in system PATH or Tools folder", "EmbeddedYtDlpLoader");

            // Create Tools directory if it doesn't exist
            var directory = Path.GetDirectoryName(YtDlpExtractedPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                LoggingSystem.Info($"Created Tools directory: {directory}", "EmbeddedYtDlpLoader");
            }

            // Extract from embedded resources
            return ExtractEmbeddedExecutable("yt-dlp.exe", YtDlpExtractedPath);
        }

        public static bool EnsureFFMPEGPresent()
        {
            // Check if ffmpeg.exe is present in the Tools folder
            if (File.Exists(FFMPEGPath))
            {
                LoggingSystem.Info($"ffmpeg.exe found in Tools folder: {FFMPEGPath}", "EmbeddedYtDlpLoader");
                return true;
            }

            // Check if ffmpeg exists in system PATH
            if (IsCommandAvailable("ffmpeg"))
            {
                LoggingSystem.Info("ffmpeg found in system PATH", "EmbeddedYtDlpLoader");
                FFMPEGPath = "ffmpeg"; // Use system command
                return true;
            }

            // Create Tools directory if it doesn't exist
            var directory = Path.GetDirectoryName(FFMPEGPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                LoggingSystem.Info($"Created Tools directory: {directory}", "EmbeddedYtDlpLoader");
            }

            // Extract from embedded resources
            return ExtractEmbeddedExecutable("ffmpeg.exe", FFMPEGPath);
        }

        public static bool EnsureFFProbePresent()
        {
            // Check if ffprobe.exe is present in the Tools folder
            if (File.Exists(FFProbePath))
            {
                LoggingSystem.Info($"ffprobe.exe found in Tools folder: {FFProbePath}", "EmbeddedYtDlpLoader"); 
                return true;
            }

            // Check if ffprobe exists in system PATH
            if (IsCommandAvailable("ffprobe"))
            {
                LoggingSystem.Info("ffprobe found in system PATH", "EmbeddedYtDlpLoader");
                FFProbePath = "ffprobe"; // Use system command
                return true;
            }

            // Create Tools directory if it doesn't exist   
            var directory = Path.GetDirectoryName(FFProbePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                LoggingSystem.Info($"Created Tools directory: {directory}", "EmbeddedYtDlpLoader");
            }   

            // Extract from embedded resources
            return ExtractEmbeddedExecutable("ffprobe.exe", FFProbePath);
        }
        
        
        private static bool IsCommandAvailable(string command)
        {
            try
            {
                using (var process = new System.Diagnostics.Process())
                {
                    process.StartInfo.FileName = command;
                    process.StartInfo.Arguments = "--version";
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
                LoggingSystem.Debug($"Command '{command}' not available in PATH: {ex.Message}", "EmbeddedYtDlpLoader");
                return false;
            }
        }
        
        private static bool ExtractEmbeddedExecutable(string executableName, string targetPath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = $"BackSpeakerMod.EmbeddedResources.Libs.Binaries.{executableName}";
            
            // Check if resource exists with detailed logging
            var resourceNames = assembly.GetManifestResourceNames();
            LoggingSystem.Debug($"Looking for {executableName} in embedded resources", "EmbeddedYtDlpLoader");
            LoggingSystem.Debug($"Total embedded resources found: {resourceNames.Length}", "EmbeddedYtDlpLoader");
            
            bool resourceExists = false;
            foreach (var name in resourceNames)
            {
                LoggingSystem.Debug($"Available resource: {name}", "EmbeddedYtDlpLoader");
                if (name.EndsWith(executableName))
                {
                    resourceName = name;
                    resourceExists = true;
                    LoggingSystem.Info($"Found {executableName} resource: {name}", "EmbeddedYtDlpLoader");
                    break;
                }
            }

            if (!resourceExists)
            {
                LoggingSystem.Error($"{executableName} not found in embedded resources", "EmbeddedYtDlpLoader");
                return false;
            }

            LoggingSystem.Debug($"Extracting embedded resource: {resourceName}", "EmbeddedYtDlpLoader");

            try
            {
                using (var stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        LoggingSystem.Error($"Failed to get stream for resource: {resourceName}", "EmbeddedYtDlpLoader");
                        return false;
                    }
                    
                    LoggingSystem.Debug($"Copying resource stream to: {targetPath}", "EmbeddedYtDlpLoader");
                    using (var fs = File.Create(targetPath))
                    {
                        stream.CopyTo(fs);
                    }
                }
                LoggingSystem.Info($"✓ {executableName} extracted to Tools folder: {targetPath}", "EmbeddedYtDlpLoader");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to extract {executableName}: {ex.Message}", "EmbeddedYtDlpLoader");
                return false;
            }

            return File.Exists(targetPath);
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
