#if IL2CPP
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Il2Cpp
{
    public class Il2CppAudioManager : IAudioManager
    {
        private readonly Il2CppScheduleOne.Audio.AudioManager manager;
        public Il2CppAudioManager(Il2CppScheduleOne.Audio.AudioManager manager)
        {
            this.manager = manager;
        }
        public float GetVolume(S1AudioType type, bool scaled) => manager.GetVolume(Map(type), scaled);
        public void SetVolume(S1AudioType type, float volume) => manager.SetVolume(Map(type), volume);

        private Il2CppScheduleOne.Audio.EAudioType Map(S1AudioType type) => type switch
        {
            S1AudioType.Music => Il2CppScheduleOne.Audio.EAudioType.Music,
            S1AudioType.FX => Il2CppScheduleOne.Audio.EAudioType.FX,
            S1AudioType.Ambient => Il2CppScheduleOne.Audio.EAudioType.Ambient,
            S1AudioType.UI => Il2CppScheduleOne.Audio.EAudioType.UI,
            _ => Il2CppScheduleOne.Audio.EAudioType.Voice
        };
    }
}
#endif
