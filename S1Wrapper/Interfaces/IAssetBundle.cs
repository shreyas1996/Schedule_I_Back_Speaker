using UnityEngine;

namespace BackSpeakerMod.S1Wrapper.Interfaces
{
    public interface IAssetBundle
    {
        T? LoadAsset<T>(string name) where T : Object;
        Object[] LoadAllAssets();
        void Unload(bool unloadAllLoadedObjects);
    }
}
