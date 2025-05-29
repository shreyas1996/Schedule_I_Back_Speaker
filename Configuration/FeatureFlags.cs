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
        /// DEPRECATED: Being replaced by spheres
        /// </summary>
        public static class Headphones
        {
            public static bool Enabled = false; // Disabled - replaced by spheres
            public static bool ShowDebugInfo = false;
            public static bool EnablePhysics = false;
            public static bool AutoAttachOnSpawn = false;
            public static bool EnableVisibilityDebugging = false; // Deprecated
        }

        /// <summary>
        /// Sphere attachment system features
        /// This replaces the headphone system
        /// </summary>
        public static class Spheres
        {
            public static bool Enabled = true; // Main feature - enabled by default
            public static bool ShowDebugInfo = false;
            public static bool EnableGlowEffect = true; // Spheres glow by default
            public static bool AutoAttachOnSpawn = false;
            public static bool EnableRotation = true; // Spheres rotate by default
        }

        /// <summary>
        /// Placement system features
        /// </summary>
        public static class Placement
        {
            public static bool Enabled 
            { 
                get => FeatureToggleSystem.Placement.Enabled; 
                set => FeatureToggleSystem.Placement.Enabled = value; 
            }
            
            public static bool ShowPreview 
            { 
                get => FeatureToggleSystem.Placement.ShowPreview; 
                set => FeatureToggleSystem.Placement.ShowPreview = value; 
            }
            
            public static bool EnableRaycastDebugging 
            { 
                get => FeatureToggleSystem.Placement.EnableRaycastDebugging; 
                set => FeatureToggleSystem.Placement.EnableRaycastDebugging = value; 
            }
            
            public static bool ShowPlacementInstructions 
            { 
                get => FeatureToggleSystem.Placement.ShowPlacementInstructions; 
                set => FeatureToggleSystem.Placement.ShowPlacementInstructions = value; 
            }
        }

        /// <summary>
        /// Testing and debug features
        /// </summary>
        public static class Testing
        {
            public static bool Enabled 
            { 
                get => FeatureToggleSystem.Testing.Enabled; 
                set => FeatureToggleSystem.Testing.Enabled = value; 
            }
            
            public static bool GlowingSphere 
            { 
                get => FeatureToggleSystem.Testing.GlowingSphere; 
                set => FeatureToggleSystem.Testing.GlowingSphere = value; 
            }
            
            public static bool TestCube 
            { 
                get => FeatureToggleSystem.Testing.TestCube; 
                set => FeatureToggleSystem.Testing.TestCube = value; 
            }
            
            public static bool LayerTesting 
            { 
                get => FeatureToggleSystem.Testing.LayerTesting; 
                set => FeatureToggleSystem.Testing.LayerTesting = value; 
            }
            
            public static bool CameraDebugging 
            { 
                get => FeatureToggleSystem.Testing.CameraDebugging; 
                set => FeatureToggleSystem.Testing.CameraDebugging = value; 
            }
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
            public static bool ShowFeatureToggles 
            { 
                get => FeatureToggleSystem.UI.ShowFeatureToggles; 
                set => FeatureToggleSystem.UI.ShowFeatureToggles = value; 
            }
            
            public static bool ShowDebugPanels 
            { 
                get => FeatureToggleSystem.UI.ShowDebugPanels; 
                set => FeatureToggleSystem.UI.ShowDebugPanels = value; 
            }
            
            public static bool ShowStatusInformation 
            { 
                get => FeatureToggleSystem.UI.ShowStatusInformation; 
                set => FeatureToggleSystem.UI.ShowStatusInformation = value; 
            }
        }

        /// <summary>
        /// Check if any feature is enabled
        /// </summary>
        public static bool AnyFeatureEnabled => FeatureToggleSystem.AnyFeatureEnabled;

        /// <summary>
        /// Quick disable all features for performance testing
        /// </summary>
        public static void DisableAll() => FeatureToggleSystem.DisableAll();

        /// <summary>
        /// Quick enable only essential features
        /// </summary>
        public static void EnableEssentialOnly() => FeatureToggleSystem.EnableEssentialOnly();
    }
} 