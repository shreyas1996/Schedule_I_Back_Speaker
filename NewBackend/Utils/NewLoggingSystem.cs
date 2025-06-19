using MelonLoader;

namespace BackSpeakerMod.NewBackend.Utils
{
    /// <summary>
    /// Logging levels for filtering log output
    /// </summary>
    public enum NewLogLevel
    {
        Debug = 0,    // Detailed debug information
        Info = 1,     // Important information
        Warning = 2,  // Warnings and concerning events
        Error = 3     // Errors and critical issues
    }

    /// <summary>
    /// Centralized logging system with level filtering
    /// </summary>
    public static class NewLoggingSystem
    {
        /// <summary>
        /// Current minimum log level to display
        /// </summary>
        public static NewLogLevel MinLevel { get; set; } = GetDefaultLogLevel();

        /// <summary>
        /// Enable/disable logging entirely
        /// </summary>
        public static bool Enabled { get; set; } = true;

        /// <summary>
        /// Get default log level based on build configuration
        /// </summary>
        private static NewLogLevel GetDefaultLogLevel()
        {
#if DEBUG
            return NewLogLevel.Debug;  // Show all logs in debug builds
#elif RELEASE
            return NewLogLevel.Warning; // Only warnings and errors in release builds
#elif VERBOSE_LOGGING
            return NewLogLevel.Debug;  // Force verbose logging even in optimized builds
#elif MINIMAL_LOGGING
            return NewLogLevel.Error;  // Only show errors
#else
            return NewLogLevel.Info;   // Default fallback
#endif
        }

        /// <summary>
        /// Initialize logging system with build-appropriate settings
        /// </summary>
        public static void Initialize()
        {
            MinLevel = GetDefaultLogLevel();
            
#if DEBUG
            Info("NewLoggingSystem initialized in DEBUG mode - all logs enabled", "NewBackend");
            Info($"Build Configuration: {GetBuildInfo()}", "NewBackend");
#elif RELEASE
            Info("NewLoggingSystem initialized in RELEASE mode - minimal logging", "NewBackend");
#elif VERBOSE_LOGGING
            Info("NewLoggingSystem initialized in VERBOSE mode - debug logs enabled", "NewBackend");
#elif MINIMAL_LOGGING
            // Don't log anything in minimal mode unless it's an error
#else
            Info("NewLoggingSystem initialized in default mode", "NewBackend");
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
            Info($"Logging Level: {MinLevel}", "NewBackend");
            Info($"Logging Enabled: {Enabled}", "NewBackend");
            Info($"Build Info: {GetBuildInfo()}", "NewBackend");
#endif
        }

        /// <summary>
        /// Log debug information (hidden by default)
        /// </summary>
        public static void Debug(string message, string category = "General")
        {
#if !MINIMAL_LOGGING
            Log(NewLogLevel.Debug, message, category);
#endif
        }

        /// <summary>
        /// Log important information
        /// </summary>
        public static void Info(string message, string category = "General")
        {
#if !MINIMAL_LOGGING
            Log(NewLogLevel.Info, message, category);
#endif
        }

        /// <summary>
        /// Log warnings and concerning events
        /// </summary>
        public static void Warning(string message, string category = "General")
        {
            Log(NewLogLevel.Warning, message, category);  // Always log warnings
        }

        /// <summary>
        /// Log errors and critical issues
        /// </summary>
        public static void Error(string message, string category = "General")
        {
            Log(NewLogLevel.Error, message, category);  // Always log errors
        }

        /// <summary>
        /// Verbose debug logging (only in debug builds)
        /// </summary>
        public static void Verbose(string message, string category = "General")
        {
#if DEBUG || VERBOSE_LOGGING
            Log(NewLogLevel.Debug, $"[VERBOSE] {message}", category);
#endif
        }

        /// <summary>
        /// Performance logging (only in debug builds)
        /// </summary>
        public static void Performance(string message, string category = "Performance")
        {
#if DEBUG
            Log(NewLogLevel.Debug, $"[PERF] {message}", category);
#endif
        }

        /// <summary>
        /// Internal logging method with level filtering
        /// </summary>
        private static void Log(NewLogLevel level, string message, string category)
        {
            if (!Enabled || level < MinLevel) return;

            var prefix = GetLogPrefix(level, category);
            MelonLogger.Msg($"{prefix}{message}");
        }

        /// <summary>
        /// Get formatted log prefix based on level and category
        /// </summary>
        private static string GetLogPrefix(NewLogLevel level, string category)
        {
            var levelStr = level switch
            {
                NewLogLevel.Debug => "[DEBUG]",
                NewLogLevel.Info => "[INFO]",
                NewLogLevel.Warning => "[WARN]",
                NewLogLevel.Error => "[ERROR]",
                _ => "[LOG]"
            };

            return $"{levelStr}[{category}] ";
        }

        /// <summary>
        /// Set logging level for different scenarios
        /// </summary>
        public static void SetProductionMode() => MinLevel = NewLogLevel.Info;
        public static void SetDebugMode() => MinLevel = NewLogLevel.Debug;
        public static void SetQuietMode() => MinLevel = NewLogLevel.Warning;

        /// <summary>
        /// Export logs to file (placeholder implementation)
        /// </summary>
        public static bool ExportLogs()
        {
            try
            {
                Info("Log export requested - not yet implemented", "NewLoggingSystem");
                // TODO: Implement actual log export functionality
                return true;
            }
            catch (System.Exception ex)
            {
                Error($"Failed to export logs: {ex.Message}", "NewLoggingSystem");
                return false;
            }
        }

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