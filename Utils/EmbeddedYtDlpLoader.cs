using System;
using System.IO;
using System.Reflection;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.Utils
{
    /// <summary>
    /// External dependency loader for yt-dlp and ffmpeg tools
    /// No longer embeds executables - checks for external files instead
    /// </summary>
    public static class EmbeddedYtDlpLoader
    {
        // Delegate to DependencyChecker for consistency
        public static string YtDlpFileName => DependencyChecker.YtDlpFileName;
        public static string YtDlpExtractedPath => DependencyChecker.GetYtDlpExecutablePath();
        public static string FFMPEGFileName => DependencyChecker.FFMpegFileName;
        public static string FFMPEGPath => DependencyChecker.GetFFMpegExecutablePath();
        public static string FFProbeFileName => DependencyChecker.FFProbeFileName;
        public static string FFProbePath => DependencyChecker.GetFFProbeExecutablePath();
        
        /// <summary>
        /// Check if yt-dlp is available (external file or system PATH)
        /// </summary>
        public static bool EnsureYtDlpPresent()
        {
            bool available = DependencyChecker.CheckYtDlp();
            
            if (!available)
            {
                LoggingSystem.Warning("yt-dlp not available. YouTube functionality will be limited.", "EmbeddedYtDlpLoader");
                LoggingSystem.Info($"To enable YouTube features, place yt-dlp.exe at: {DependencyChecker.YtDlpPath}", "EmbeddedYtDlpLoader");
            }
            
            return available;
        }

        /// <summary>
        /// Check if ffmpeg is available (external file or system PATH)
        /// </summary>
        public static bool EnsureFFMPEGPresent()
        {
            bool available = DependencyChecker.CheckFFMpeg();
            
            if (!available)
            {
                LoggingSystem.Warning("ffmpeg not available. Audio processing will be limited.", "EmbeddedYtDlpLoader");
                LoggingSystem.Info($"To enable full audio features, place ffmpeg.exe at: {DependencyChecker.FFMpegPath}", "EmbeddedYtDlpLoader");
            }
            
            return available;
        }

        /// <summary>
        /// Check if ffprobe is available (external file or system PATH)
        /// </summary>
        public static bool EnsureFFProbePresent()
        {
            bool available = DependencyChecker.CheckFFProbe();
            
            if (!available)
            {
                LoggingSystem.Warning("ffprobe not available. Audio analysis will be limited.", "EmbeddedYtDlpLoader");
                LoggingSystem.Info($"To enable full audio features, place ffprobe.exe at: {DependencyChecker.FFProbePath}", "EmbeddedYtDlpLoader");
            }
            
            return available;
        }

        /// <summary>
        /// Check if yt-dlp is available
        /// </summary>
        public static bool IsYtDlpAvailable()
        {
            return DependencyChecker.CheckYtDlp();
        }

        /// <summary>
        /// Check if ffmpeg is available
        /// </summary>
        public static bool IsFfmpegAvailable()
        {
            return DependencyChecker.CheckFFMpeg();
        }

        /// <summary>
        /// Get the yt-dlp executable path
        /// </summary>
        public static string GetYtDlpPath()
        {
            var path = DependencyChecker.GetYtDlpExecutablePath();
            return string.IsNullOrEmpty(path) ? null : path;
        }
        
        /// <summary>
        /// Get setup instructions for missing dependencies
        /// </summary>
        public static string GetSetupInstructions()
        {
            var status = DependencyChecker.CheckAllDependencies();
            return DependencyChecker.GetSetupInstructions(status);
        }
    }
}
