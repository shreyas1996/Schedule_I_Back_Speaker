using MelonLoader;
using UnityEngine;
using Il2CppInterop.Runtime.Injection;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.UI;

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
            LoggingSystem.Info("Back Speaker Mod initialized!", "Mod");
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
                LoggingSystem.Info("Registering DisplayPanel...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.DisplayPanel>();
                LoggingSystem.Info("✓ DisplayPanel registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register DisplayPanel: {ex}", "Mod");
            }
            
            try
            {
                LoggingSystem.Info("Registering MusicControlPanel...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.MusicControlPanel>();
                LoggingSystem.Info("✓ MusicControlPanel registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register MusicControlPanel: {ex}", "Mod");
            }
            
            try
            {
                LoggingSystem.Info("Registering VolumeControl...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.VolumeControl>();
                LoggingSystem.Info("✓ VolumeControl registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register VolumeControl: {ex}", "Mod");
            }
            
            try
            {
                LoggingSystem.Info("Registering ProgressBar...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.ProgressBar>();
                LoggingSystem.Info("✓ ProgressBar registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register ProgressBar: {ex}", "Mod");
            }
            
            try
            {
                LoggingSystem.Info("Registering PlaylistPanel...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.PlaylistPanel>();
                LoggingSystem.Info("✓ PlaylistPanel registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register PlaylistPanel: {ex}", "Mod");
            }
            
            try
            {
                LoggingSystem.Info("Registering HeadphoneControlPanel...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<UI.Components.HeadphoneControlPanel>();
                LoggingSystem.Info("✓ HeadphoneControlPanel registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register HeadphoneControlPanel: {ex}", "Mod");
            }
            
            // Sphere components excluded from compilation - focusing on headphones
            /*
            try
            {
                LoggingSystem.Info("Registering TestSphereRotator...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<Core.Features.Testing.Components.TestSphereRotator>();
                LoggingSystem.Info("✓ TestSphereRotator registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register TestSphereRotator: {ex}", "Mod");
            }
            
            try
            {
                LoggingSystem.Info("Registering SphereRotator...", "Mod");
                ClassInjector.RegisterTypeInIl2Cpp<Core.Features.Spheres.Components.SphereRotator>();
                LoggingSystem.Info("✓ SphereRotator registered successfully", "Mod");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"✗ Failed to register SphereRotator: {ex}", "Mod");
            }
            */
            
            LoggingSystem.Info("Il2Cpp type registration completed", "Mod");
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