using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
#if !IL2CPP
    public class MonoGameObject : IGameObject
    {
        private readonly GameObject obj;
        public MonoGameObject(GameObject obj)
        {
            this.obj = obj;
        }
        public GameObject GameObject => obj;
        public Transform Transform => obj.transform;
    }
#endif
}
