using BackSpeakerMod.S1Wrapper.Interfaces;
using UnityEngine;

namespace BackSpeakerMod.S1Wrapper
{
    public class S1MusicPlayer : IMusicPlayer
    {
        private readonly IMusicPlayer? _player;
        public static IMusicPlayer? Instance { get; private set; }
        private S1MusicPlayer()
        {
            if (Instance != null)
            {
                // LoggingSystem.Warning("S1MusicPlayer instance already exists, skipping initialization");
                return;
            }
            #if IL2CPP
                if (S1Environment.IsIl2Cpp)
                {
                    _player = new Il2Cpp.Il2CppMusicPlayer(UnityEngine.Object.FindObjectOfType<Il2CppScheduleOne.Audio.MusicPlayer>());
                }
            #else
                _player = new Mono.MonoMusicPlayer(UnityEngine.Object.FindObjectOfType<ScheduleOne.Audio.MusicPlayer>());
            #endif
            Instance = this;
        }
        public void Start() => _player?.Start();
        public void Stop() => _player?.Stop();
    }
}