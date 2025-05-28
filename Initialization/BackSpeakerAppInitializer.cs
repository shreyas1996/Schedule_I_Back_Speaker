using HarmonyLib;
using Il2CppScheduleOne.PlayerScripts;
using MelonLoader;
using UnityEngine;
using BackSpeakerMod.Core;
using BackSpeakerMod.Utils;
using BackSpeakerMod.Core.System;
namespace BackSpeakerMod.Initialization
{
    [HarmonyPatch(typeof(Player))]
    internal static class BackSpeakerAppInitializer
    {
        private static int retryAppCreateDelay = 60;
        private static int appCreateRetries = 0;
        private static bool appCreated = false;
        public static BackSpeakerApp? AppInstance { get; private set; }

        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        private static void Update(Player __instance)
        {
            if (AppInstance == null && !appCreated && appCreateRetries > -1)
            {
                if (appCreateRetries == 10)
                {
                    // LoggerUtil.Error("Tried to create BackSpeaker app 10 times, but failed. AppIcons doesn't exist or UI creation failed?");
                    appCreateRetries = -1;
                    appCreated = true;
                }
                else if (--retryAppCreateDelay <= 0)
                {
                    retryAppCreateDelay = 60;
                    appCreateRetries++;
                    if (GameObject.Find("AppIcons") != null)
                    {
                        // LoggerUtil.Info($"Attempt {appCreateRetries}: Trying to create BackSpeakerApp");
                        try
                        {
                            AppInstance = new BackSpeakerApp(BackSpeakerModMain.SpeakerManager);
                            if (AppInstance != null && AppInstance.Create())
                            {
                                appCreated = true;
                                // LoggerUtil.Info("BackSpeakerApp successfully created.");
                            }
                            else
                            {
                                // LoggerUtil.Warn("BackSpeakerApp creation failed. Will retry.");
                                AppInstance = null;
                            }
                        }
                        catch (global::System.Exception ex)
                        {
                            LoggingSystem.Error($"Failed to initialize BackSpeaker app: {ex.Message}", "Initialization");
                            // Just continue without returning anything since this is a void method
                        }
                    }
                }
            }
        }
    }
} 