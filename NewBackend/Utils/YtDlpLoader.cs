using System;
using System.IO;
using System.Reflection;
using BackSpeakerMod.NewBackend.Utils;

namespace BackSpeakerMod.NewBackend.Utils
{
    public static class NewYtDlpLoader
    {
        private static string? _ytDlpPath;
        private static string? _ffmpegPath;
        private static bool _initialized = false;

        public static string GetYtDlpPath()
        {
            if (!_initialized)
            {
                Initialize();
            }
            return _ytDlpPath ?? "";
        }

        public static string GetFfmpegPath()
        {
            if (!_initialized)
            {
                Initialize();
            }
            return _ffmpegPath ?? "";
        }

        public static bool IsYtDlpAvailable()
        {
            if (!_initialized)
            {
                Initialize();
            }
            return !string.IsNullOrEmpty(_ytDlpPath) && File.Exists(_ytDlpPath);
        }

        public static bool IsFfmpegAvailable()
        {
            if (!_initialized)
            {
                Initialize();
            }
            return !string.IsNullOrEmpty(_ffmpegPath) && File.Exists(_ffmpegPath);
        }

        private static void Initialize()
        {
            try
            {
                // Get the mod directory
                var modDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (string.IsNullOrEmpty(modDirectory))
                {
                    NewLoggingSystem.Error("Could not determine mod directory", "NewYtDlpLoader");
                    _initialized = true;
                    return;
                }

                var binDirectory = Path.Combine(modDirectory, "bin");
                
                // Check for yt-dlp
                var ytDlpExe = Path.Combine(binDirectory, "yt-dlp.exe");
                if (File.Exists(ytDlpExe))
                {
                    _ytDlpPath = ytDlpExe;
                    NewLoggingSystem.Info($"Found yt-dlp at: {ytDlpExe}", "NewYtDlpLoader");
                }
                else
                {
                    NewLoggingSystem.Warning("yt-dlp.exe not found in bin directory", "NewYtDlpLoader");
                }

                // Check for ffmpeg
                var ffmpegExe = Path.Combine(binDirectory, "ffmpeg.exe");
                if (File.Exists(ffmpegExe))
                {
                    _ffmpegPath = ffmpegExe;
                    NewLoggingSystem.Info($"Found ffmpeg at: {ffmpegExe}", "NewYtDlpLoader");
                }
                else
                {
                    NewLoggingSystem.Warning("ffmpeg.exe not found in bin directory", "NewYtDlpLoader");
                }

                _initialized = true;
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error initializing NewYtDlpLoader: {ex}", "NewYtDlpLoader");
                _initialized = true;
            }
        }
    }
} 