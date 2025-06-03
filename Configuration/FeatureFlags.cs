using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.Configuration
{
    /// <summary>
    /// Backward compatibility wrapper for FeatureToggleSystem
    /// </summary>
    public static class FeatureFlags
    {
        /// <summary>
        /// Headphone attachment system features
        /// PRIMARY FEATURE - enabled and active
        /// </summary>
        public static class Headphones
        {
            public static bool Enabled = true; // Primary feature - enabled
            public static bool ShowDebugInfo = false;
            public static bool EnablePhysics = false;
            public static bool AutoAttachOnSpawn = true; // Auto-attach headphones
            public static bool EnableVisibilityDebugging = false;
            public static bool ApplyCustomMaterials = true; // Enable URP material application
            public static bool EnableMaterialDebugging = false; // Debug material properties
            public static bool ValidateMaterialShaders = true; // Validate URP shader compatibility
            public static bool EnableBoneDiscovery = true; // Enable runtime bone discovery for debugging
        }

        /// <summary>
        /// Audio system features
        /// </summary>
        public static class Audio
        {
            public static bool Enabled 
            { 
                get => FeatureToggleSystem.Audio.Enabled; 
                set => FeatureToggleSystem.Audio.Enabled = value; 
            }
            
            public static bool AutoLoadTracks 
            { 
                get => FeatureToggleSystem.Audio.AutoLoadTracks; 
                set => FeatureToggleSystem.Audio.AutoLoadTracks = value; 
            }
            
            public static bool ShowTrackInfo 
            { 
                get => FeatureToggleSystem.Audio.ShowTrackInfo; 
                set => FeatureToggleSystem.Audio.ShowTrackInfo = value; 
            }
            
            public static bool EnableDebugLogging 
            { 
                get => FeatureToggleSystem.Audio.EnableDebugLogging; 
                set => FeatureToggleSystem.Audio.EnableDebugLogging = value; 
            }
        }

        /// <summary>
        /// UI features
        /// </summary>
        public static class UI
        {
            public static bool ShowFeatureToggles = true;
            public static bool ShowDebugPanels = false;
            public static bool ShowStatusInformation = true;
        }

        /// <summary>
        /// Check if any feature is enabled
        /// </summary>
        public static bool AnyFeatureEnabled => 
            Headphones.Enabled || Audio.Enabled;

        /// <summary>
        /// Enable material debugging for troubleshooting
        /// </summary>
        public static void EnableMaterialDebugging()
        {
            Headphones.ShowDebugInfo = true;
            Headphones.EnableMaterialDebugging = true;
            Headphones.ValidateMaterialShaders = true;
            LoggingSystem.Info("Material debugging enabled", "FeatureFlags");
        }

        /// <summary>
        /// Disable material debugging for performance
        /// </summary>
        public static void DisableMaterialDebugging()
        {
            Headphones.EnableMaterialDebugging = false;
            // Keep ShowDebugInfo as-is since it controls other things too
            LoggingSystem.Info("Material debugging disabled", "FeatureFlags");
        }
    }
} 