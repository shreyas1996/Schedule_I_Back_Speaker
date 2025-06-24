#if !IL2CPP
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoPlayerCamera : IPlayerCamera
    {
        private readonly ScheduleOne.PlayerScripts.PlayerCamera camera;
        public MonoPlayerCamera(ScheduleOne.PlayerScripts.PlayerCamera camera)
        {
            this.camera = camera;
        }
        public S1CameraMode CameraMode => Map(camera.CameraMode);
        public bool FreeCamEnabled => camera.FreeCamEnabled;
        public bool ViewingAvatar => camera.ViewingAvatar;

        private S1CameraMode Map(ScheduleOne.PlayerScripts.PlayerCamera.ECameraMode mode) => mode switch
        {
            ScheduleOne.PlayerScripts.PlayerCamera.ECameraMode.Vehicle => S1CameraMode.Vehicle,
            ScheduleOne.PlayerScripts.PlayerCamera.ECameraMode.Skateboard => S1CameraMode.Skateboard,
            _ => S1CameraMode.Default
        };
    }
}
#endif
