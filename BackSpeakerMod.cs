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
                LoggingSystem.Info("Registering LocalFolderMusicProvider...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<Core.Modules.LocalFolderMusicProvider>();
                LoggingSystem.Info("✓ LocalFolderMusicProvider registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register LocalFolderMusicProvider: {ex}", "Mod");
            }

            try {
                LoggingSystem.Info("Registering TrackLoader...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<Core.Modules.TrackLoader>();
                LoggingSystem.Info("✓ TrackLoader registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register TrackLoader: {ex}", "Mod");
            }

            LoggingSystem.Info("Il2Cpp type registration completed", "Mod");
        }

        public override void OnUpdate()
        {
            // Update app state tracking
            SpeakerApp?.Update();
            
            // Update music manager for auto-advance functionality
            SpeakerManager?.Update();
        }
    }
} 