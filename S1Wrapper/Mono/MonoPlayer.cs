using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
#if !IL2CPP
    public class MonoPlayer : IPlayer
    {
        private readonly ScheduleOne.PlayerScripts.Player player;
        public MonoPlayer(ScheduleOne.PlayerScripts.Player player)
        {
            this.player = player;
        }
        public Transform Transform => player.transform;
        public GameObject GameObject => player.gameObject;
        public string Name => player.name;
        public IAvatar? Avatar => player.Avatar != null ? new MonoAvatar(player.Avatar) : null;
    }
#endif
}
