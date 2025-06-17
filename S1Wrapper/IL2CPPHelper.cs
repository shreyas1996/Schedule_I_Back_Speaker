using System;

namespace BackSpeakerMod.S1Wrapper
{
    public static class IL2CPPHelper
    {
        public static void RegisterIl2CppType<T>()
        {
#if IL2CPP
            if (S1Environment.IsIl2Cpp)
            {
                Il2CppInterop.Runtime.Injection.ClassInjector.RegisterTypeInIl2Cpp<T>();
            }
#endif
        }
    }
}
