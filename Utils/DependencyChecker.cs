using System;
using System.IO;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.Utils
{
    /// <summary>
    /// Checks for external dependencies and provides graceful error handling
    /// Replaces embedded dependency system to avoid licensing and antivirus issues
    /// </summary>
    public static class DependencyChecker
    {
        // Expected dependency locations
        private static readonly string ToolsDirectory = Path.Combine("Mods", "BackSpeaker", "Tools");
        private static readonly string LibsDirectory = Path.Combine("Mods", "BackSpeaker", "Libs");
        
        // MelonLoader UserLibs directory (preferred location for AudioImportLib.dll)
        private static readonly string UserLibsDirectory = "UserLibs";
        
        // Dependency file names
        public static readonly string YtDlpFileName = "yt-dlp.exe";
        public static readonly string FFMpegFileName = "ffmpeg.exe";
        public static readonly string FFProbeFileName = "ffprobe.exe";
        public static readonly string AudioImportLibFileName = "AudioImportLib.dll";
        
        // Full paths
        public static string YtDlpPath => Path.Combine(ToolsDirectory, YtDlpFileName);
        public static string FFMpegPath => Path.Combine(ToolsDirectory, FFMpegFileName);
        public static string FFProbePath => Path.Combine(ToolsDirectory, FFProbeFileName);
        
        // AudioImportLib paths (check UserLibs first, then Libs directory)
        public static string AudioImportLibUserLibsPath => Path.Combine(UserLibsDirectory, AudioImportLibFileName);
        public static string AudioImportLibLibsPath => Path.Combine(LibsDirectory, AudioImportLibFileName);
        
        // Primary AudioImportLib path (for backward compatibility)
        public static string AudioImportLibPath => AudioImportLibUserLibsPath;
        
        /// <summary>
        /// Check if all required dependencies are available
        /// </summary>
        public static DependencyStatus CheckAllDependencies()
        {
            var status = new DependencyStatus();
            
            // Create directories if they don't exist
            EnsureDirectoriesExist();
            
            // Check each dependency
            status.YtDlpAvailable = CheckYtDlp();
            status.FFMpegAvailable = CheckFFMpeg();
            status.FFProbeAvailable = CheckFFProbe();
            status.AudioImportLibAvailable = CheckAudioImportLib();
            
            // Log results
            LogDependencyStatus(status);
            
            return status;
        }
        
        /// <summary>
        /// Check if yt-dlp is available (file or system PATH)
        /// </summary>
        public static bool CheckYtDlp()
        {
            // Check local file first
            if (File.Exists(YtDlpPath))
            {
                LoggingSystem.Debug($"yt-dlp found at: {YtDlpPath}", "DependencyChecker");
                return true;
            }
            
            // Check system PATH
            if (IsCommandAvailable("yt-dlp"))
            {
                LoggingSystem.Debug("yt-dlp found in system PATH", "DependencyChecker");
                return true;
            }
            
            LoggingSystem.Warning($"yt-dlp not found. Expected location: {YtDlpPath}", "DependencyChecker");
            return false;
        }
        
        /// <summary>
        /// Check if ffmpeg is available (file or system PATH)
        /// </summary>
        public static bool CheckFFMpeg()
        {
            // Check local file first
            if (File.Exists(FFMpegPath))
            {
                LoggingSystem.Debug($"ffmpeg found at: {FFMpegPath}", "DependencyChecker");
                return true;
            }
            
            // Check system PATH
            if (IsCommandAvailable("ffmpeg"))
            {
                LoggingSystem.Debug("ffmpeg found in system PATH", "DependencyChecker");
                return true;
            }
            
            LoggingSystem.Warning($"ffmpeg not found. Expected location: {FFMpegPath}", "DependencyChecker");
            return false;
        }
        
        /// <summary>
        /// Check if ffprobe is available (file or system PATH)
        /// </summary>
        public static bool CheckFFProbe()
        {
            // Check local file first
            if (File.Exists(FFProbePath))
            {
                LoggingSystem.Debug($"ffprobe found at: {FFProbePath}", "DependencyChecker");
                return true;
            }
            
            // Check system PATH
            if (IsCommandAvailable("ffprobe"))
            {
                LoggingSystem.Debug("ffprobe found in system PATH", "DependencyChecker");
                return true;
            }
            
            LoggingSystem.Warning($"ffprobe not found. Expected location: {FFProbePath}", "DependencyChecker");
            return false;
        }
        
        /// <summary>
        /// Check if AudioImportLib.dll is available
        /// </summary>
        public static bool CheckAudioImportLib()
        {
            // Check UserLibs directory first (preferred location for MelonLoader)
            if (File.Exists(AudioImportLibUserLibsPath))
            {
                LoggingSystem.Debug($"AudioImportLib.dll found in UserLibs at: {AudioImportLibUserLibsPath}", "DependencyChecker");
                return true;
            }
            
            // Check Libs directory as fallback
            if (File.Exists(AudioImportLibLibsPath))
            {
                LoggingSystem.Debug($"AudioImportLib.dll found in Libs at: {AudioImportLibLibsPath}", "DependencyChecker");
                return true;
            }
            
            LoggingSystem.Warning($"AudioImportLib.dll not found. Preferred location: {AudioImportLibUserLibsPath}", "DependencyChecker");
            LoggingSystem.Warning($"Alternative location: {AudioImportLibLibsPath}", "DependencyChecker");
            return false;
        }
        
        /// <summary>
        /// Get the appropriate executable path (local file or system command)
        /// </summary>
        public static string GetYtDlpExecutablePath()
        {
            if (File.Exists(YtDlpPath)) return YtDlpPath;
            if (IsCommandAvailable("yt-dlp")) return "yt-dlp";
            return "";
        }
        
        public static string GetFFMpegExecutablePath()
        {
            if (File.Exists(FFMpegPath)) return FFMpegPath;
            if (IsCommandAvailable("ffmpeg")) return "ffmpeg";
            return "";
        }
        
        public static string GetFFProbeExecutablePath()
        {
            if (File.Exists(FFProbePath)) return FFProbePath;
            if (IsCommandAvailable("ffprobe")) return "ffprobe";
            return "";
        }
        
        /// <summary>
        /// Get the actual AudioImportLib.dll path that exists
        /// </summary>
        public static string GetAudioImportLibPath()
        {
            if (File.Exists(AudioImportLibUserLibsPath)) return AudioImportLibUserLibsPath;
            if (File.Exists(AudioImportLibLibsPath)) return AudioImportLibLibsPath;
            return AudioImportLibUserLibsPath; // Return preferred path even if it doesn't exist
        }
        
        /// <summary>
        /// Create setup instructions for missing dependencies
        /// </summary>
        public static string GetSetupInstructions(DependencyStatus status)
        {
            var instructions = "BackSpeaker Setup Instructions:\n\n";
            
            if (!status.HasAllDependencies())
            {
                instructions += "Some dependencies are missing. Please follow these steps:\n\n";
                
                if (!status.YtDlpAvailable)
                {
                    instructions += $"1. Download yt-dlp.exe and place it at: {YtDlpPath}\n";
                    instructions += "   Download from: https://github.com/yt-dlp/yt-dlp/releases\n\n";
                }
                
                if (!status.FFMpegAvailable)
                {
                    instructions += $"2. Download ffmpeg.exe and place it at: {FFMpegPath}\n";
                    instructions += "   Download from: https://ffmpeg.org/download.html\n\n";
                }
                
                if (!status.FFProbeAvailable)
                {
                    instructions += $"3. Download ffprobe.exe and place it at: {FFProbePath}\n";
                    instructions += "   (Usually included with ffmpeg)\n\n";
                }
                
                if (!status.AudioImportLibAvailable)
                {
                    instructions += $"4. Download AudioImportLib.dll and place it at: {AudioImportLibUserLibsPath}\n";
                    instructions += "   (Preferred: MelonLoader will automatically load it from UserLibs)\n";
                    instructions += $"   Alternative location: {AudioImportLibLibsPath}\n";
                    instructions += "   Contact mod author for this dependency\n\n";
                }
                
                instructions += "After placing the files, restart the game.\n";
                instructions += "The mod will work with limited functionality until all dependencies are available.";
            }
            else
            {
                instructions += "All dependencies are available! ✓";
            }
            
            return instructions;
        }
        
        /// <summary>
        /// Ensure required directories exist
        /// </summary>
        private static void EnsureDirectoriesExist()
        {
            try
            {
                if (!Directory.Exists(ToolsDirectory))
                {
                    Directory.CreateDirectory(ToolsDirectory);
                    LoggingSystem.Info($"Created Tools directory: {ToolsDirectory}", "DependencyChecker");
                }
                
                if (!Directory.Exists(LibsDirectory))
                {
                    Directory.CreateDirectory(LibsDirectory);
                    LoggingSystem.Info($"Created Libs directory: {LibsDirectory}", "DependencyChecker");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to create dependency directories: {ex.Message}", "DependencyChecker");
            }
        }
        
        /// <summary>
        /// Check if a command is available in system PATH
        /// </summary>
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
                LoggingSystem.Debug($"Command '{command}' not available in PATH: {ex.Message}", "DependencyChecker");
                return false;
            }
        }
        
        /// <summary>
        /// Log dependency status
        /// </summary>
        private static void LogDependencyStatus(DependencyStatus status)
        {
            LoggingSystem.Info("=== Dependency Check Results ===", "DependencyChecker");
            LoggingSystem.Info($"yt-dlp: {(status.YtDlpAvailable ? "✓" : "✗")}", "DependencyChecker");
            LoggingSystem.Info($"ffmpeg: {(status.FFMpegAvailable ? "✓" : "✗")}", "DependencyChecker");
            LoggingSystem.Info($"ffprobe: {(status.FFProbeAvailable ? "✓" : "✗")}", "DependencyChecker");
            LoggingSystem.Info($"AudioImportLib: {(status.AudioImportLibAvailable ? "✓" : "✗")}", "DependencyChecker");
            LoggingSystem.Info($"All dependencies: {(status.HasAllDependencies() ? "✓" : "✗")}", "DependencyChecker");
            LoggingSystem.Info("================================", "DependencyChecker");
        }
    }
    
    /// <summary>
    /// Represents the status of all dependencies
    /// </summary>
    public class DependencyStatus
    {
        public bool YtDlpAvailable { get; set; }
        public bool FFMpegAvailable { get; set; }
        public bool FFProbeAvailable { get; set; }
        public bool AudioImportLibAvailable { get; set; }
        
        public bool HasAllDependencies() => YtDlpAvailable && FFMpegAvailable && FFProbeAvailable && AudioImportLibAvailable;
        public bool HasYouTubeDependencies() => YtDlpAvailable && FFMpegAvailable && FFProbeAvailable;
        public bool HasAudioDependencies() => AudioImportLibAvailable;
    }
} 