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
            ClassInjector.RegisterTypeInIl2Cpp<BackSpeakerScreen>();
            ClassInjector.RegisterTypeInIl2Cpp<UI.Components.DisplayPanel>();
            ClassInjector.RegisterTypeInIl2Cpp<UI.Components.MusicControlPanel>();
            ClassInjector.RegisterTypeInIl2Cpp<UI.Components.VolumeControl>();
        }

        public override void OnUpdate()
        {
            // Update app state tracking like Drones does
            SpeakerApp?.Update();
        }
    }
} 