#if !IL2CPP
using BackSpeakerMod.S1Wrapper.Interfaces;
using UnityEngine;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoAssetBundleWrapper : IAssetBundle
    {
        private readonly AssetBundle bundle;
        public MonoAssetBundleWrapper(AssetBundle bundle)
        {
            this.bundle = bundle;
        }
        public T? LoadAsset<T>(string name) where T : Object => bundle.LoadAsset<T>(name);
        public Object[] LoadAllAssets() => bundle.LoadAllAssets();
        public void Unload(bool unloadAllLoadedObjects) => bundle.Unload(unloadAllLoadedObjects);
    }
}
#endif
