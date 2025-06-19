#if !IL2CPP
using System;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoAudioManager : IAudioManager
    {
        private readonly ScheduleOne.AudioManager _audioManager;

        public MonoAudioManager(ScheduleOne.AudioManager audioManager)
        {
            _audioManager = audioManager ?? throw new ArgumentNullException(nameof(audioManager));
        }

        public void PlaySound(string soundName)
        {
            _audioManager.PlaySound(soundName);
        }

        public void StopSound(string soundName)
        {
            _audioManager.StopSound(soundName);
        }

        public void SetVolume(float volume)
        {
            _audioManager.SetVolume(volume);
        }

        public float GetVolume()
        {
            return _audioManager.GetVolume();
        }

        // Legacy methods for compatibility
        public float GetVolume(S1AudioType type, bool scaled)
        {
            // For simplicity, just return the general volume
            return GetVolume();
        }

        public void SetVolume(S1AudioType type, float volume)
        {
            // For simplicity, just set the general volume
            SetVolume(volume);
        }
    }
}
#endif
