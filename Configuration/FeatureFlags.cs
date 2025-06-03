using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.Configuration
{
    /// <summary>
    /// Feature flags for controlling mod functionality
    /// Consolidated from the previous FeatureToggleSystem
    /// </summary>
    public static class FeatureFlags
    {
        /// <summary>
        /// Headphone attachment system features
        /// PRIMARY FEATURE - enabled and active
        /// </summary>
        public static class Headphones
        {
            public static bool Enabled
            {
                get => SystemManager.Features.HeadphonesEnabled;
                set => SystemManager.Features.HeadphonesEnabled = value;
            }

            public static bool ShowDebugInfo
            {
                get => SystemManager.Features.ShowDebugInfo;
                set => SystemManager.Features.ShowDebugInfo = value;
            }

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
                get => SystemManager.Features.AudioEnabled; 
                set => SystemManager.Features.AudioEnabled = value; 
            }
            
            public static bool AutoLoadTracks 
            { 
                get => SystemManager.Features.AutoLoadTracks; 
                set => SystemManager.Features.AutoLoadTracks = value; 
            }
            
            public static bool ShowTrackInfo = true;
            public static bool EnableDebugLogging = false;
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