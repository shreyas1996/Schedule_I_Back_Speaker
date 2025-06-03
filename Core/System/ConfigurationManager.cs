using System;
using System.Collections.Generic;

namespace BackSpeakerMod.Core.System
{
    /// <summary>
    /// Settings management system
    /// </summary>
    public static class ConfigurationManager
    {
        private static readonly Dictionary<string, object> settings = new Dictionary<string, object>();
        private static bool isInitialized = false;

        /// <summary>
        /// Initialize configuration manager
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized) return;
            
            LoggingSystem.Info("ConfigurationManager: Initializing", "System");
            LoadDefaultSettings();
            isInitialized = true;
        }

        /// <summary>
        /// Get setting value
        /// </summary>
        public static T? GetSetting<T>(string key, T? defaultValue = default(T))
        {
            if (settings.TryGetValue(key, out var value))
            {
                try
                {
                    return (T)value;
                }
                catch (Exception ex)
                {
                    LoggingSystem.Warning($"Failed to cast setting '{key}': {ex.Message}", "System");
                }
            }
            
            return defaultValue;
        }

        /// <summary>
        /// Set setting value
        /// </summary>
        public static void SetSetting<T>(string key, T value)
        {
            try {
                if (settings.ContainsKey(key))
                {
                    LoggingSystem.Warning($"Setting '{key}' already exists", "System");
                    return;
                }

                settings[key] = value ?? throw new ArgumentNullException(nameof(value));
                LoggingSystem.Debug($"Setting '{key}' set to '{value}'", "System");
            } 
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to set setting '{key}': {ex.Message}", "System");
            }
        }

        /// <summary>
        /// Load default settings
        /// </summary>
        private static void LoadDefaultSettings()
        {
            SetSetting("Audio.Enabled", true);
            SetSetting("Audio.Volume", 0.5f);
            SetSetting("Headphones.Enabled", true);
        }
    }
} 