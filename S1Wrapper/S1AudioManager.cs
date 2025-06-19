using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper
{
    public class S1AudioManager : IAudioManager
    {
        private static IAudioManager? _manager;
        public static IAudioManager? Instance { get; private set; }
        public S1AudioManager()
        {
            if (_manager != null)
            {
                // LoggingSystem.Warning("S1AudioManager instance already exists, skipping initialization");
                return;
            }
            #if IL2CPP
                if (S1Environment.IsIl2Cpp)
                {
                    _manager = new Il2Cpp.Il2CppAudioManager(UnityEngine.Object.FindObjectOfType<Il2CppScheduleOne.Audio.AudioManager>());
                }
            #else
                _manager = new Mono.MonoAudioManager(UnityEngine.Object.FindObjectOfType<ScheduleOne.Audio.AudioManager>());
            #endif
            Instance = this;
            // LoggingSystem.Info("S1AudioManager instance created");
        }
        
        // Basic audio manager functionality
        public void PlaySound(string soundName) => _manager?.PlaySound(soundName);
        public void StopSound(string soundName) => _manager?.StopSound(soundName);
        public void SetVolume(float volume) => _manager?.SetVolume(volume);
        public float GetVolume() => _manager?.GetVolume() ?? 0f;
        
        // Legacy methods for compatibility
        public float GetVolume(S1AudioType type, bool scaled) => _manager?.GetVolume(type, scaled) ?? 0f;
        public void SetVolume(S1AudioType type, float volume) => _manager?.SetVolume(type, volume);
    }
}