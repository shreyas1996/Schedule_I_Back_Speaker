using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Il2Cpp
{
#if IL2CPP
    public class Il2CppPlayer : IPlayer
    {
        private readonly Il2CppScheduleOne.PlayerScripts.Player player;
        public Il2CppPlayer(Il2CppScheduleOne.PlayerScripts.Player player)
        {
            this.player = player;
        }
        public Transform Transform => player.transform;
        public GameObject GameObject => player.gameObject;
        public string Name => player.name;
        public IAvatar? Avatar => player.Avatar != null ? new Il2CppAvatar(player.Avatar) : null;
    }
#endif
}
