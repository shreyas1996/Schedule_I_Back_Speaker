#if !IL2CPP
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoAudioManager : IAudioManager
    {
        private readonly ScheduleOne.Audio.AudioManager manager;
        public MonoAudioManager(ScheduleOne.Audio.AudioManager manager)
        {
            this.manager = manager;
        }
        public float GetVolume(S1AudioType type, bool scaled) => manager.GetVolume(Map(type), scaled);
        public void SetVolume(S1AudioType type, float volume) => manager.SetVolume(Map(type), volume);

        private ScheduleOne.Audio.EAudioType Map(S1AudioType type) => type switch
        {
            S1AudioType.Music => ScheduleOne.Audio.EAudioType.Music,
            S1AudioType.FX => ScheduleOne.Audio.EAudioType.FX,
            S1AudioType.Ambient => ScheduleOne.Audio.EAudioType.Ambient,
            S1AudioType.UI => ScheduleOne.Audio.EAudioType.UI,
            _ => ScheduleOne.Audio.EAudioType.Voice
        };
    }
}
#endif
