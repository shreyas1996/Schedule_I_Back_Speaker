#if IL2CPP
using BackSpeakerMod.S1Wrapper.Interfaces;
using UnityEngine;

namespace BackSpeakerMod.S1Wrapper.Il2Cpp
{
    public class Il2CppAssetBundleWrapper : IAssetBundle
    {
        private readonly Il2CppAssetBundle bundle;
        public Il2CppAssetBundleWrapper(Il2CppAssetBundle bundle)
        {
            this.bundle = bundle;
        }
        public T? LoadAsset<T>(string name) where T : Object => bundle.LoadAsset<T>(name);
        public Object[] LoadAllAssets() => bundle.LoadAllAssets();
        public void Unload(bool unloadAllLoadedObjects) => bundle.Unload(unloadAllLoadedObjects);
    }
}
#endif
