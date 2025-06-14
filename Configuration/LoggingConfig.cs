using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.Configuration
{
    /// <summary>
    /// Logging configuration with build-time defaults and runtime overrides
    /// </summary>
    public static class LoggingConfig
    {
        /// <summary>
        /// Default log level based on build configuration
        /// </summary>
        public static LogLevel DefaultLogLevel
        {
            get
            {
                #if DEBUG
                    return LogLevel.Debug;
                #elif RELEASE && MINIMAL_LOGGING
                    return LogLevel.Error;
                #elif RELEASE
                    return LogLevel.Warning;
                #elif VERBOSE_LOGGING
                    return LogLevel.Debug;
                #else
                    return LogLevel.Info;
                #endif
            }
        }

        /// <summary>
        /// Whether to enable performance logging
        /// </summary>
        public static bool EnablePerformanceLogging
        {
            get
            {
                #if DEBUG
                    return true;
                #else
                    return false;
                #endif
            }
        }

        /// <summary>
        /// Whether to enable verbose/debug output
        /// </summary>
        public static bool EnableVerboseLogging
        {
            get
            {
                #if DEBUG || VERBOSE_LOGGING
                    return true;
                #else
                    return false;
                #endif
            }
        }

        /// <summary>
        /// Whether to suppress info messages
        /// </summary>
        public static bool SuppressInfoMessages
        {
            get
            {
                #if MINIMAL_LOGGING
                    return true;
                #else
                    return false;
                #endif
            }
        }

        /// <summary>
        /// Get build-specific configuration summary
        /// </summary>
        public static string GetConfigSummary()
        {
            var summary = new System.Text.StringBuilder();
            summary.AppendLine("Logging Configuration:");
            summary.AppendLine($"  Build Type: {GetBuildType()}");
            summary.AppendLine($"  Default Log Level: {DefaultLogLevel}");
            summary.AppendLine($"  Performance Logging: {EnablePerformanceLogging}");
            summary.AppendLine($"  Verbose Logging: {EnableVerboseLogging}");
            summary.AppendLine($"  Suppress Info: {SuppressInfoMessages}");
            return summary.ToString();
        }

        /// <summary>
        /// Get the current build type
        /// </summary>
        private static string GetBuildType()
        {
            #if DEBUG
                return "DEBUG";
            #elif RELEASE && MINIMAL_LOGGING
                return "RELEASE (Minimal)";
            #elif RELEASE && VERBOSE_LOGGING
                return "RELEASE (Verbose)";
            #elif RELEASE
                return "RELEASE";
            #elif IL2CPP
                return "IL2CPP";
            #else
                return "UNKNOWN";
            #endif
        }

        /// <summary>
        /// Apply configuration to logging system
        /// </summary>
        public static void ApplyToLoggingSystem()
        {
            LoggingSystem.MinLevel = DefaultLogLevel;
            
            // Log the configuration that was applied
            if (EnableVerboseLogging)
            {
                LoggingSystem.Info(GetConfigSummary(), "Config");
            }
        }
    }
} 