namespace BackSpeakerMod.S1Wrapper.Interfaces
{
    public enum S1CameraMode
    {
        Default,
        Vehicle,
        Skateboard
    }

    public interface IPlayerCamera
    {
        S1CameraMode CameraMode { get; }
        bool FreeCamEnabled { get; }
        bool ViewingAvatar { get; }
    }
}
