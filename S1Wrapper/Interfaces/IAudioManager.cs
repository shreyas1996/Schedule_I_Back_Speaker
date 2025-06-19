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
        // Basic audio manager functionality
        void PlaySound(string soundName);
        void StopSound(string soundName);
        void SetVolume(float volume);
        float GetVolume();
        
        // Legacy methods for compatibility
        float GetVolume(S1AudioType type, bool scaled);
        void SetVolume(S1AudioType type, float volume);
    }
}
