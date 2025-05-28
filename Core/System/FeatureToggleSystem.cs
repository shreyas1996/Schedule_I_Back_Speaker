using System;

namespace BackSpeakerMod.Core.System
{
    /// <summary>
    /// System for enabling/disabling mod features
    /// </summary>
    public static class FeatureToggleSystem
    {
        /// <summary>
        /// Headphone-related features
        /// </summary>
        public static class Headphones
        {
            public static bool Enabled = true;
            public static bool AutoAttachOnSpawn = false;
            public static bool ShowDebugInfo = false;
            public static bool EnableVisibilityDebugging = false;
        }

        /// <summary>
        /// Placement system features
        /// </summary>
        public static class Placement
        {
            public static bool Enabled = true;
            public static bool ShowPreview = true;
            public static bool EnableRaycastDebugging = false;
            public static bool ShowPlacementInstructions = true;
        }

        /// <summary>
        /// Testing and debug features
        /// </summary>
        public static class Testing
        {
            public static bool Enabled = true;
            public static bool GlowingSphere = true;
            public static bool TestCube = true;
            public static bool LayerTesting = false;
            public static bool CameraDebugging = false;
        }

        /// <summary>
        /// Audio system features
        /// </summary>
        public static class Audio
        {
            public static bool Enabled = true;
            public static bool AutoLoadTracks = true;
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
            Headphones.Enabled || Placement.Enabled || Testing.Enabled || Audio.Enabled;

        /// <summary>
        /// Quick disable all features for performance testing
        /// </summary>
        public static void DisableAll()
        {
            Headphones.Enabled = false;
            Placement.Enabled = false;
            Testing.Enabled = false;
            Audio.Enabled = false;
        }

        /// <summary>
        /// Quick enable only essential features
        /// </summary>
        public static void EnableEssentialOnly()
        {
            DisableAll();
            Headphones.Enabled = true;
            Audio.Enabled = true;
        }
    }
} 