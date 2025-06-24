using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper
{
    /// <summary>
    /// Static wrapper for Schedule I HomeScreen - mirrors HomeScreen.cs exactly
    /// </summary>
    public static class S1HomeScreen
    {
        public static IHomeScreen? Instance
        {
            get
            {
                #if IL2CPP
                    if (S1Environment.IsIl2Cpp)
                    {
                        var homeScreen = Il2CppScheduleOne.DevUtilities.PlayerSingleton<Il2CppScheduleOne.UI.Phone.HomeScreen>.instance;
                        return homeScreen != null ? new Il2Cpp.Il2CppHomeScreen(homeScreen) : null;
                    }
                    return null;
                #else
                    var homeScreen = ScheduleOne.DevUtilities.PlayerSingleton<ScheduleOne.UI.Phone.HomeScreen>.Instance;
                    return homeScreen != null ? new Mono.MonoHomeScreen(homeScreen) : null;
                #endif
            }
        }
    }
} 