using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
#if !IL2CPP
    public class MonoMusicPlayer : IMusicPlayer
    {
        private readonly ScheduleOne.Audio.MusicPlayer player;
        public MonoMusicPlayer(ScheduleOne.Audio.MusicPlayer player)
        {
            this.player = player;
        }
        public void Play(AudioClip clip) => player.PlayClip(clip);
        public void Stop() => player.StopAndDisableTracks();
    }
#endif
}
