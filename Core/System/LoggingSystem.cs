using BackSpeakerMod.Utils;
using MelonLoader;

namespace BackSpeakerMod.Core.System
{
    /// <summary>
    /// Logging levels for filtering log output
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,    // Detailed debug information
        Info = 1,     // Important information
        Warning = 2,  // Warnings and concerning events
        Error = 3     // Errors and critical issues
    }

    /// <summary>
    /// Centralized logging system with level filtering
    /// </summary>
    public static class LoggingSystem
    {
        /// <summary>
        /// Current minimum log level to display
        /// </summary>
        public static LogLevel MinLevel { get; set; } = LogLevel.Debug;

        /// <summary>
        /// Enable/disable logging entirely
        /// </summary>
        public static bool Enabled { get; set; } = true;

        /// <summary>
        /// Log debug information (hidden by default)
        /// </summary>
        public static void Debug(string message, string category = "General")
        {
            Log(LogLevel.Debug, message, category);
        }

        /// <summary>
        /// Log important information
        /// </summary>
        public static void Info(string message, string category = "General")
        {
            Log(LogLevel.Info, message, category);
        }

        /// <summary>
        /// Log warnings and concerning events
        /// </summary>
        public static void Warning(string message, string category = "General")
        {
            Log(LogLevel.Warning, message, category);
        }

        /// <summary>
        /// Log errors and critical issues
        /// </summary>
        public static void Error(string message, string category = "General")
        {
            Log(LogLevel.Error, message, category);
        }

        /// <summary>
        /// Internal logging method with level filtering
        /// </summary>
        private static void Log(LogLevel level, string message, string category)
        {
            if (!Enabled || level < MinLevel) return;

            var prefix = GetLogPrefix(level, category);
            MelonLogger.Msg($"{prefix}{message}");
        }

        /// <summary>
        /// Get formatted log prefix based on level and category
        /// </summary>
        private static string GetLogPrefix(LogLevel level, string category)
        {
            var levelStr = level switch
            {
                LogLevel.Debug => "[DEBUG]",
                LogLevel.Info => "[INFO]",
                LogLevel.Warning => "[WARN]",
                LogLevel.Error => "[ERROR]",
                _ => "[LOG]"
            };

            return $"{levelStr}[{category}] ";
        }

        /// <summary>
        /// Set logging level for different scenarios
        /// </summary>
        public static void SetProductionMode() => MinLevel = LogLevel.Info;
        public static void SetDebugMode() => MinLevel = LogLevel.Debug;
        public static void SetQuietMode() => MinLevel = LogLevel.Warning;
    }
} 