using MelonLoader;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.UI;
using BackSpeakerMod.Configuration;

[assembly: MelonInfo(typeof(BackSpeakerMod.BackSpeakerModMain), "Back Speaker Mod", "1.0.0", "Shreyas")]
[assembly: MelonGame("TVGS", "Schedule I")]
[assembly: MelonColor(0, 255, 0, 255)]

namespace BackSpeakerMod
{
    public class BackSpeakerModMain : MelonMod
    {
        public static BackSpeakerManager? SpeakerManager { get; private set; }
        public static BackSpeakerApp? SpeakerApp { get; private set; }

        public override void OnInitializeMelon()
        {
            // Initialize logging system first with build-specific settings
            LoggingSystem.Initialize();
            LoggingConfig.ApplyToLoggingSystem();
            
            LoggingSystem.Info("Back Speaker Mod initialized!", "Mod");
            LoggingSystem.Info($"Build Configuration: {LoggingSystem.GetBuildInfo()}", "Mod");
            
            // Show detailed config in debug builds
            #if DEBUG || VERBOSE_LOGGING
                LoggingSystem.Info(LoggingConfig.GetConfigSummary(), "Mod");
            #endif
            
            SpeakerManager = BackSpeakerManager.Instance;
            
            // Load headphone bundle at startup (only once)
            InitializeHeadphoneBundle();
        }
        
        /// <summary>
        /// Initialize headphone bundle once at mod startup
        /// </summary>
        private void InitializeHeadphoneBundle()
        {
            try
            {
                LoggingSystem.Info("Loading headphone bundle at mod startup", "Mod");
                var headphoneManager = SpeakerManager?.GetHeadphoneManager();
                if (headphoneManager != null)
                {
                    SpeakerManager.InitializeHeadphoneAssets();
                    LoggingSystem.Info("✓ Headphone bundle loaded at startup", "Mod");
                }
                else
                {
                    LoggingSystem.Warning("HeadphoneManager not available during startup", "Mod");
                }
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to load headphone bundle at startup: {ex}", "Mod");
            }
        }

        public override void OnLateInitializeMelon()
        {
            LoggingSystem.Info("Starting Il2Cpp type registration...", "Mod");
            
            // Register each component individually with detailed error reporting
            // Note: The "Assembly not registered" warning is normal and benign
            try
            {
                LoggingSystem.Info("Registering BackSpeakerScreen...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<BackSpeakerScreen>();
                LoggingSystem.Info("✓ BackSpeakerScreen registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register BackSpeakerScreen: {ex}", "Mod");
            }
            
            try
            {
                LoggingSystem.Info("Registering TabBarComponent...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.TabBarComponent>();
                LoggingSystem.Info("✓ TabBarComponent registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register TabBarComponent: {ex}", "Mod");
            }
            
            try
            {
                LoggingSystem.Info("Registering ContentAreaComponent...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.ContentAreaComponent>();
                LoggingSystem.Info("✓ ContentAreaComponent registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register ContentAreaComponent: {ex}", "Mod");
            }
            
            try
            {
                LoggingSystem.Info("Registering TrackInfoComponent...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.TrackInfoComponent>();
                LoggingSystem.Info("✓ TrackInfoComponent registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register TrackInfoComponent: {ex}", "Mod");
            }
            
            try
            {
                LoggingSystem.Info("Registering ProgressBarComponent...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.ProgressBarComponent>();
                LoggingSystem.Info("✓ ProgressBarComponent registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register ProgressBarComponent: {ex}", "Mod");
            }
            
            try
            {
                LoggingSystem.Info("Registering ControlsComponent...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.ControlsComponent>();
                LoggingSystem.Info("✓ ControlsComponent registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register ControlsComponent: {ex}", "Mod");
            }
            
            try
            {
                LoggingSystem.Info("Registering ActionButtonsComponent...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.ActionButtonsComponent>();
                LoggingSystem.Info("✓ ActionButtonsComponent registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register ActionButtonsComponent: {ex}", "Mod");
            }
            
            try
            {
                LoggingSystem.Info("Registering PlaylistToggleComponent...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.PlaylistToggleComponent>();
                LoggingSystem.Info("✓ PlaylistToggleComponent registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register PlaylistToggleComponent: {ex}", "Mod");
            }
            
            try
            {
                LoggingSystem.Info("Registering HelpTextComponent...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.HelpTextComponent>();
                LoggingSystem.Info("✓ HelpTextComponent registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register HelpTextComponent: {ex}", "Mod");
            }

            try {
                LoggingSystem.Info("Registering YouTubeMusicProvider...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<Core.Modules.YouTubeMusicProvider>();
                LoggingSystem.Info("✓ YouTubeMusicProvider registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register YouTubeMusicProvider: {ex}", "Mod");
            }

            try {
                LoggingSystem.Info("Registering YouTubePopupComponent...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.YouTubePopupComponent>();
                LoggingSystem.Info("✓ YouTubePopupComponent registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register YouTubePopupComponent: {ex}", "Mod");
            }

            try {
                LoggingSystem.Info("Registering LocalFolderMusicProvider...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<Core.Modules.LocalFolderMusicProvider>();
                LoggingSystem.Info("✓ LocalFolderMusicProvider registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register LocalFolderMusicProvider: {ex}", "Mod");
            }

            // try {
            //     LoggingSystem.Info("Registering TrackLoader...", "Mod");
            //     ClassInjector.RegisterTypeInIl2Cpp<Core.Modules.TrackLoader>();
            //     LoggingSystem.Info("✓ TrackLoader registered successfully", "Mod");
            // }
            // catch (System.Exception ex)
            // {
            //     LoggingSystem.Error($"✗ Failed to register TrackLoader: {ex}", "Mod");
            // }

            LoggingSystem.Info("Il2Cpp type registration completed", "Mod");
        }

        public override void OnUpdate()
        {
            // Update app state tracking if app exists
            SpeakerApp?.Update();
            
            // Update music manager for auto-advance functionality
            SpeakerManager?.Update();
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            LoggingSystem.Debug($"Scene loaded: {sceneName} (buildIndex: {buildIndex})", "Mod");
            
            // Handle main scene initialization
            if (IsMainGameScene(sceneName))
            {
                LoggingSystem.Info("Main scene loaded - preparing for initialization", "Mod");
                // Player detection will trigger the actual app creation
            }
            else if (sceneName.Contains("Menu"))
            {
                LoggingSystem.Info("Menu scene loaded - cleaning up if needed", "Mod");
                CleanupMainSceneComponents();
            }
        }
        
        /// <summary>
        /// Check if the scene is a main game scene
        /// </summary>
        private bool IsMainGameScene(string sceneName)
        {
            return sceneName.Contains("Main") || 
                   sceneName.Contains("Game") || 
                   sceneName.Contains("Level") ||
                   sceneName == "SampleScene";
        }
        
        /// <summary>
        /// Initialize main scene components after player is ready
        /// </summary>
        public static void InitializeMainSceneComponents()
        {
            try
            {
                LoggingSystem.Info("Initializing main scene components", "Mod");
                
                // Clear any old event subscriptions before creating new app
                if (SpeakerManager != null)
                {
                    SpeakerManager.ClearEventSubscriptions();
                }
                
                // Create BackSpeakerApp if it doesn't exist
                if (SpeakerApp == null && SpeakerManager != null)
                {
                    LoggingSystem.Info("Creating BackSpeakerApp for main scene", "Mod");
                    SpeakerApp = new BackSpeakerApp(SpeakerManager);
                    if (SpeakerApp.Create())
                    {
                        LoggingSystem.Info("✓ BackSpeakerApp created successfully", "Mod");
                    }
                    else
                    {
                        LoggingSystem.Error("Failed to create BackSpeakerApp", "Mod");
                        SpeakerApp = null;
                    }
                }
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to initialize main scene components: {ex}", "Mod");
            }
        }
        
        /// <summary>
        /// Cleanup main scene components when leaving main scene
        /// </summary>
        public static void CleanupMainSceneComponents()
        {
            try
            {
                LoggingSystem.Info("Cleaning up main scene components", "Mod");
                
                // Destroy BackSpeakerApp
                if (SpeakerApp != null)
                {
                    LoggingSystem.Info("Destroying BackSpeakerApp", "Mod");
                    SpeakerApp.Destroy();
                    SpeakerApp = null;
                }
                
                LoggingSystem.Info("✓ Main scene components cleaned up", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to cleanup main scene components: {ex}", "Mod");
            }
        }
    }
} 