using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Il2Cpp
{
#if IL2CPP
    public class Il2CppGameObject : IGameObject
    {
        private readonly GameObject obj;
        public Il2CppGameObject(GameObject obj)
        {
            this.obj = obj;
        }
        public GameObject GameObject => obj;
        public Transform Transform => obj.transform;
    }
#endif
}
