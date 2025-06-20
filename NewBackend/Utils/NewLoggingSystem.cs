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
        /// 
        // change the categories to args that can be any number of strings with a default of "General"
        public static void Debug(string message, params string[] categories)
        {
#if !MINIMAL_LOGGING
            Log(NewLogLevel.Debug, message, categories);
#endif
        }

        /// <summary>
        /// Log important information
        /// </summary>
        public static void Info(string message, params string[] categories)
        {
#if !MINIMAL_LOGGING
            Log(NewLogLevel.Info, message, categories);
#endif
        }

        /// <summary>
        /// Log warnings and concerning events
        /// </summary>
        public static void Warning(string message, params string[] categories)
        {
            Log(NewLogLevel.Warning, message, categories);  // Always log warnings
        }

        /// <summary>
        /// Log errors and critical issues
        /// </summary>
        public static void Error(string message, params string[] categories)
        {
            Log(NewLogLevel.Error, message, categories);  // Always log errors
        }

        /// <summary>
        /// Verbose debug logging (only in debug builds)
        /// </summary>
        public static void Verbose(string message, params string[] categories)
        {
#if DEBUG || VERBOSE_LOGGING
            Log(NewLogLevel.Debug, $"[VERBOSE] {message}", categories);
#endif
        }

        /// <summary>
        /// Performance logging (only in debug builds)
        /// </summary>
        public static void Performance(string message, params string[] categories)
        {
#if DEBUG
            Log(NewLogLevel.Debug, $"[PERF] {message}", categories);
#endif
        }

        /// <summary>
        /// Internal logging method with level filtering
        /// </summary>
        private static void Log(NewLogLevel level, string message, params string[] categories)
        {
            if (!Enabled || level < MinLevel) return;

            var prefix = GetLogPrefix(level, categories);
            MelonLogger.Msg($"{prefix}{message}");
        }

        /// <summary>
        /// Get formatted log prefix based on level and category
        /// </summary>
        private static string GetLogPrefix(NewLogLevel level, params string[] categories)
        {
            var levelStr = level switch
            {
                NewLogLevel.Debug => "[DEBUG]",
                NewLogLevel.Info => "[INFO]",
                NewLogLevel.Warning => "[WARN]",
                NewLogLevel.Error => "[ERROR]",
                _ => "[LOG]"
            };
            // if no categories are provided, use "General"
            if(categories.Length == 0)
            {
                categories = new string[] { "General" };
            }

            // reverse the categories
            Array.Reverse(categories);
            // wrap each category in square brackets
            for(int i = 0; i < categories.Length; i++)
            {
                categories[i] = $"[{categories[i]}]";
            }

            return $"{levelStr} {string.Join(" ", categories)} ";
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