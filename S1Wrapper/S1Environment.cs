using System;

namespace BackSpeakerMod.S1Wrapper
{
    public static class S1Environment
    {
        private static bool? _isIl2Cpp;
        public static bool IsIl2Cpp
        {
            get
            {
                if (!_isIl2Cpp.HasValue)
                {
                    try
                    {
                        _isIl2Cpp = Type.GetType("Il2CppInterop.Runtime.Il2Cpp" + "String", throwOnError: false) != null;
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
