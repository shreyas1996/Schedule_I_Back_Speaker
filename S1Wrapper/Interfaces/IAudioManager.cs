namespace BackSpeakerMod.S1Wrapper.Interfaces
{
    public enum S1AudioType
    {
        Music,
        FX,
        Ambient,
        UI,
        Voice
    }

    public interface IAudioManager
    {
        float GetVolume(S1AudioType type, bool scaled);
        void SetVolume(S1AudioType type, float volume);
    }
}
