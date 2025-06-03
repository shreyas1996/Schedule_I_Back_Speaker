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
        public static LogLevel MinLevel { get; set; } = GetDefaultLogLevel();

        /// <summary>
        /// Enable/disable logging entirely
        /// </summary>
        public static bool Enabled { get; set; } = true;

        /// <summary>
        /// Get default log level based on build configuration
        /// </summary>
        private static LogLevel GetDefaultLogLevel()
        {
#if DEBUG
            return LogLevel.Debug;  // Show all logs in debug builds
#elif RELEASE
            return LogLevel.Warning; // Only warnings and errors in release builds
#elif VERBOSE_LOGGING
            return LogLevel.Debug;  // Force verbose logging even in optimized builds
#elif MINIMAL_LOGGING
            return LogLevel.Error;  // Only show errors
#else
            return LogLevel.Info;   // Default fallback
#endif
        }

        /// <summary>
        /// Initialize logging system with build-appropriate settings
        /// </summary>
        public static void Initialize()
        {
            MinLevel = GetDefaultLogLevel();
            
#if DEBUG
            Info("LoggingSystem initialized in DEBUG mode - all logs enabled", "System");
            Info($"Build Configuration: {GetBuildInfo()}", "System");
#elif RELEASE
            Info("LoggingSystem initialized in RELEASE mode - minimal logging", "System");
#elif VERBOSE_LOGGING
            Info("LoggingSystem initialized in VERBOSE mode - debug logs enabled", "System");
#elif MINIMAL_LOGGING
            // Don't log anything in minimal mode unless it's an error
#else
            Info("LoggingSystem initialized in default mode", "System");
#endif
            
            // Additional conditional features
#if DEBUG || VERBOSE_LOGGING
            LogSystemInfo();
#endif
        }

        /// <summary>
        /// Log system information (only in debug/verbose builds)
        /// </summary>
        private static void LogSystemInfo()
        {
#if DEBUG || VERBOSE_LOGGING
            Info($"Logging Level: {MinLevel}", "System");
            Info($"Logging Enabled: {Enabled}", "System");
            Info($"Build Info: {GetBuildInfo()}", "System");
#endif
        }

        /// <summary>
        /// Log debug information (hidden by default)
        /// </summary>
        public static void Debug(string message, string category = "General")
        {
#if !MINIMAL_LOGGING
            Log(LogLevel.Debug, message, category);
#endif
        }

        /// <summary>
        /// Log important information
        /// </summary>
        public static void Info(string message, string category = "General")
        {
#if !MINIMAL_LOGGING
            Log(LogLevel.Info, message, category);
#endif
        }

        /// <summary>
        /// Log warnings and concerning events
        /// </summary>
        public static void Warning(string message, string category = "General")
        {
            Log(LogLevel.Warning, message, category);  // Always log warnings
        }

        /// <summary>
        /// Log errors and critical issues
        /// </summary>
        public static void Error(string message, string category = "General")
        {
            Log(LogLevel.Error, message, category);  // Always log errors
        }

        /// <summary>
        /// Verbose debug logging (only in debug builds)
        /// </summary>
        public static void Verbose(string message, string category = "General")
        {
#if DEBUG || VERBOSE_LOGGING
            Log(LogLevel.Debug, $"[VERBOSE] {message}", category);
#endif
        }

        /// <summary>
        /// Performance logging (only in debug builds)
        /// </summary>
        public static void Performance(string message, string category = "Performance")
        {
#if DEBUG
            Log(LogLevel.Debug, $"[PERF] {message}", category);
#endif
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

        /// <summary>
        /// Get current build configuration info
        /// </summary>
        public static string GetBuildInfo()
        {
#if DEBUG
            return "DEBUG";
#elif RELEASE
            return "RELEASE";
#else
            return "UNKNOWN";
#endif
        }
    }
} 