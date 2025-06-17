using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Il2Cpp
{
#if IL2CPP
    public class Il2CppMusicPlayer : IMusicPlayer
    {
        private readonly Il2CppScheduleOne.Audio.MusicPlayer player;
        public Il2CppMusicPlayer(Il2CppScheduleOne.Audio.MusicPlayer player)
        {
            this.player = player;
        }
        public void Play(AudioClip clip) => player.PlayClip(clip);
        public void Stop() => player.StopAndDisableTracks();
    }
#endif
}
