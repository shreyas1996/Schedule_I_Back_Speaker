using BackSpeakerMod.S1Wrapper.Interfaces;
using UnityEngine;

namespace BackSpeakerMod.S1Wrapper
{
    public static class S1AssetBundleLoader
    {
        public static IAssetBundle? LoadFromFile(string path)
        {
#if IL2CPP
            if (S1Environment.IsIl2Cpp)
            {
                var bundle = Il2CppAssetBundleManager.LoadFromFile(path);
                return bundle != null ? new Il2Cpp.Il2CppAssetBundleWrapper(bundle) : null;
            }
            return null;
#else
            var bundle = AssetBundle.LoadFromFile(path);
            return bundle != null ? new Mono.MonoAssetBundleWrapper(bundle) : null;
#endif
        }
    }
}
