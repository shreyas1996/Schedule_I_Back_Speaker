#if !IL2CPP
using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoAvatar : IAvatar
    {
        private readonly ScheduleOne.PlayerScripts.Avatar avatar;
        public MonoAvatar(ScheduleOne.PlayerScripts.Avatar avatar)
        {
            this.avatar = avatar;
        }
        public Transform? HeadBone => avatar.HeadBone;
    }
}
#endif
