#if IL2CPP
using System;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Il2Cpp
{
    public class Il2CppAudioManager : IAudioManager
    {
        private readonly object _audioManager;

        public Il2CppAudioManager(object audioManager)
        {
            _audioManager = audioManager ?? throw new ArgumentNullException(nameof(audioManager));
        }

        public void PlaySound(string soundName)
        {
            try
            {
                var method = _audioManager.GetType().GetMethod("PlaySound");
                method?.Invoke(_audioManager, new object[] { soundName });
            }
            catch (Exception)
            {
                // Handle reflection failures silently
            }
        }

        public void StopSound(string soundName)
        {
            try
            {
                var method = _audioManager.GetType().GetMethod("StopSound");
                method?.Invoke(_audioManager, new object[] { soundName });
            }
            catch (Exception)
            {
                // Handle reflection failures silently
            }
        }

        public void SetVolume(float volume)
        {
            try
            {
                var method = _audioManager.GetType().GetMethod("SetVolume");
                method?.Invoke(_audioManager, new object[] { volume });
            }
            catch (Exception)
            {
                // Handle reflection failures silently
            }
        }

        public float GetVolume()
        {
            try
            {
                var method = _audioManager.GetType().GetMethod("GetVolume");
                var result = method?.Invoke(_audioManager, null);
                return result is float volume ? volume : 0f;
            }
            catch (Exception)
            {
                // Handle reflection failures silently
                return 0f;
            }
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
