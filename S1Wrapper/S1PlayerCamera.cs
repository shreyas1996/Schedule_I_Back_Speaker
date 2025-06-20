using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper
{
    public class S1PlayerCamera : IPlayerCamera
    {
        private readonly IPlayerCamera? _camera;
        public S1PlayerCamera()
        {
            #if IL2CPP
                if (S1Environment.IsIl2Cpp)
                {
                    _camera = new Il2Cpp.Il2CppPlayerCamera(Il2CppScheduleOne.PlayerScripts.PlayerCamera.Instance);
                }
            #else
                _camera = new Mono.MonoPlayerCamera(ScheduleOne.PlayerScripts.PlayerCamera.Instance);
            #endif
        }

        public IPlayerCamera? GetCamera() => _camera;
        public S1CameraMode CameraMode => _camera?.CameraMode ?? S1CameraMode.Default;
        public bool FreeCamEnabled => _camera?.FreeCamEnabled ?? false;
        public bool ViewingAvatar => _camera?.ViewingAvatar ?? false;
    }
}