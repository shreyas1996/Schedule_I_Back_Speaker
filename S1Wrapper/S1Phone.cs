using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper
{
    public static class S1Phone
    {
        public static IPhone? Instance
        {
            get
            {
#if IL2CPP
                if (S1Environment.IsIl2Cpp)
                {
                    var phone = Il2CppScheduleOne.PlayerSingleton<Il2CppScheduleOne.UI.Phone.Phone>.instance;
                    return phone != null ? new Il2Cpp.Il2CppPhone(phone) : null;
                }
                return null;
#else
                var phone = ScheduleOne.PlayerSingleton<ScheduleOne.UI.Phone.Phone>.instance;
                return phone != null ? new Mono.MonoPhone(phone) : null;
#endif
            }
        }
    }
}
