using System;

namespace BackSpeakerMod.S1Wrapper
{
    public static class S1Environment
    {
        private static bool? _isIl2Cpp;

        /// <summary>
        /// Returns <c>true</c> when the Il2CppInterop runtime is available.
        /// </summary>
        public static bool IsIl2Cpp
        {
            get
            {
                if (!_isIl2Cpp.HasValue)
                {
                    try
                    {
                        _isIl2Cpp = Type.GetType(
                            "Il2CppInterop.Runtime.Injection.ClassInjector, Il2CppInterop.Runtime",
                            throwOnError: false) != null;
                    }
                    catch
                    {
                        _isIl2Cpp = false;
                    }
                }
                return _isIl2Cpp.Value;
            }
        }
    }
}
