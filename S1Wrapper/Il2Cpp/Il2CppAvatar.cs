#if IL2CPP
using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Il2Cpp
{
    public class Il2CppAvatar : IAvatar
    {
        private readonly Il2CppScheduleOne.PlayerScripts.Avatar avatar;
        public Il2CppAvatar(Il2CppScheduleOne.PlayerScripts.Avatar avatar)
        {
            this.avatar = avatar;
        }
        public Transform? HeadBone => avatar.HeadBone;
    }
}
#endif
