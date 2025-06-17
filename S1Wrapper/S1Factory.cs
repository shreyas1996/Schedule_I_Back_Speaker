using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper
{
    public static class S1Factory
    {
        public static IPlayer? GetLocalPlayer()
        {
#if IL2CPP
            if (S1Environment.IsIl2Cpp)
            {
                var player = Il2CppScheduleOne.PlayerScripts.Player.Local;
                return player != null ? new Il2Cpp.Il2CppPlayer(player) : null;
            }
            return null;
#else
            var player = ScheduleOne.PlayerScripts.Player.Local;
            return player != null ? new Mono.MonoPlayer(player) : null;
#endif
        }

        public static IAudioManager? FindAudioManager()
        {
#if IL2CPP
            if (S1Environment.IsIl2Cpp)
            {
                var mgr = UnityEngine.Object.FindObjectOfType<Il2CppScheduleOne.Audio.AudioManager>();
                return mgr != null ? new Il2Cpp.Il2CppAudioManager(mgr) : null;
            }
            return null;
#else
            var mgr = UnityEngine.Object.FindObjectOfType<ScheduleOne.Audio.AudioManager>();
            return mgr != null ? new Mono.MonoAudioManager(mgr) : null;
#endif
        }

        public static IMusicPlayer? FindMusicPlayer()
        {
#if IL2CPP
            if (S1Environment.IsIl2Cpp)
            {
                var player = UnityEngine.Object.FindObjectOfType<Il2CppScheduleOne.Audio.MusicPlayer>();
                return player != null ? new Il2Cpp.Il2CppMusicPlayer(player) : null;
            }
            return null;
#else
            var player = UnityEngine.Object.FindObjectOfType<ScheduleOne.Audio.MusicPlayer>();
            return player != null ? new Mono.MonoMusicPlayer(player) : null;
#endif
        }

        public static IPlayerCamera? GetPlayerCamera()
        {
#if IL2CPP
            if (S1Environment.IsIl2Cpp)
            {
                var cam = Il2CppScheduleOne.PlayerScripts.PlayerCamera.Instance;
                return cam != null ? new Il2Cpp.Il2CppPlayerCamera(cam) : null;
            }
            return null;
#else
            var cam = ScheduleOne.PlayerScripts.PlayerCamera.Instance;
            return cam != null ? new Mono.MonoPlayerCamera(cam) : null;
#endif
        }
    }
}
