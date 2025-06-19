#if !IL2CPP
using System;
using UnityEngine;

namespace BackSpeakerMod.S1Wrapper
{
    public static class MonoHelper
    {
            public static AssetBundle? LoadFromMemory(byte[] data)
            {
                return AssetBundle.LoadFromMemory(data);
            }

            public static AssetBundle? LoadFromFile(string path)
            {
                return AssetBundle.LoadFromFile(path);
            }
    }
}
#endif