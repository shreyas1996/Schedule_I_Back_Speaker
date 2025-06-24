using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper
{
    /// <summary>
    /// Static wrapper for Schedule I AppsCanvas - mirrors AppsCanvas.cs exactly
    /// </summary>
    public static class S1AppsCanvas
    {
        public static IAppsCanvas? Instance
        {
            get
            {
                #if IL2CPP
                    if (S1Environment.IsIl2Cpp)
                    {
                        var appsCanvas = Il2CppScheduleOne.DevUtilities.PlayerSingleton<Il2CppScheduleOne.UI.Phone.AppsCanvas>.instance;
                        return appsCanvas != null ? new Il2Cpp.Il2CppAppsCanvas(appsCanvas) : null;
                    }
                    return null;
                #else
                    var appsCanvas = ScheduleOne.DevUtilities.PlayerSingleton<ScheduleOne.UI.Phone.AppsCanvas>.Instance;
                    return appsCanvas != null ? new Mono.MonoAppsCanvas(appsCanvas) : null;
                #endif
            }
        }
    }
} 