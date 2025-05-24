using MelonLoader;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using BackSpeakerMod.Core;
using BackSpeakerMod.Utils;
using BackSpeakerMod.UI;

[assembly: MelonInfo(typeof(BackSpeakerMod.BackSpeakerModMain), "Back Speaker Mod", "1.0.0", "Shreyas")]
[assembly: MelonGame("TVGS", "Schedule I")]
[assembly: MelonColor(0, 255, 0, 255)]

namespace BackSpeakerMod
{
    public class BackSpeakerModMain : MelonMod
    {
        public static BackSpeakerManager SpeakerManager;
        public static BackSpeakerApp SpeakerApp;

        public override void OnInitializeMelon()
        {
            LoggerUtil.Info("Back Speaker Mod initialized!");
            SpeakerManager = new BackSpeakerManager();
        }

        public override void OnLateInitializeMelon()
        {
            LoggerUtil.Info("Starting Il2Cpp type registration...");
            
            // Register each component individually with detailed error reporting
            try
            {
                LoggerUtil.Info("Registering BackSpeakerScreen...");
                ClassInjector.RegisterTypeInIl2Cpp<BackSpeakerScreen>();
                LoggerUtil.Info("✓ BackSpeakerScreen registered successfully");
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"✗ Failed to register BackSpeakerScreen: {ex}");
            }
            
            try
            {
                LoggerUtil.Info("Registering DisplayPanel...");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.DisplayPanel>();
                LoggerUtil.Info("✓ DisplayPanel registered successfully");
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"✗ Failed to register DisplayPanel: {ex}");
            }
            
            try
            {
                LoggerUtil.Info("Registering MusicControlPanel...");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.MusicControlPanel>();
                LoggerUtil.Info("✓ MusicControlPanel registered successfully");
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"✗ Failed to register MusicControlPanel: {ex}");
            }
            
            try
            {
                LoggerUtil.Info("Registering VolumeControl...");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.VolumeControl>();
                LoggerUtil.Info("✓ VolumeControl registered successfully");
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"✗ Failed to register VolumeControl: {ex}");
            }
            
            try
            {
                LoggerUtil.Info("Registering ProgressBar...");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.ProgressBar>();
                LoggerUtil.Info("✓ ProgressBar registered successfully");
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"✗ Failed to register ProgressBar: {ex}");
            }
            
            try
            {
                LoggerUtil.Info("Registering PlaylistPanel...");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.PlaylistPanel>();
                LoggerUtil.Info("✓ PlaylistPanel registered successfully");
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"✗ Failed to register PlaylistPanel: {ex}");
            }
            
            LoggerUtil.Info("Il2Cpp type registration completed");
        }

        public override void OnUpdate()
        {
            // Update app state tracking like Drones does
            SpeakerApp?.Update();
            
            // Update music manager for auto-advance functionality
            SpeakerManager?.Update();
        }
    }
} 