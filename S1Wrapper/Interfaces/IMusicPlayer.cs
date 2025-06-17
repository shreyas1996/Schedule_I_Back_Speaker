using UnityEngine;

namespace BackSpeakerMod.S1Wrapper.Interfaces
{
    public interface IMusicPlayer
    {
        void Play(AudioClip clip);
        void Stop();
    }
}
