#if IL2CPP
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Il2Cpp
{
    public class Il2CppPlayerCamera : IPlayerCamera
    {
        private readonly Il2CppScheduleOne.PlayerScripts.PlayerCamera camera;
        public Il2CppPlayerCamera(Il2CppScheduleOne.PlayerScripts.PlayerCamera camera)
        {
            this.camera = camera;
        }
        public S1CameraMode CameraMode => Map(camera.CameraMode);
        public bool FreeCamEnabled => camera.FreeCamEnabled;
        public bool ViewingAvatar => camera.ViewingAvatar;

        private S1CameraMode Map(Il2CppScheduleOne.PlayerScripts.PlayerCamera.ECameraMode mode) => mode switch
        {
            Il2CppScheduleOne.PlayerScripts.PlayerCamera.ECameraMode.Vehicle => S1CameraMode.Vehicle,
            Il2CppScheduleOne.PlayerScripts.PlayerCamera.ECameraMode.Skateboard => S1CameraMode.Skateboard,
            _ => S1CameraMode.Default
        };
    }
}
#endif
