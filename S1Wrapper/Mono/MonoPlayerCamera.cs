#if !IL2CPP
using System;
using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoPlayerCamera : IPlayerCamera
    {
        private readonly ScheduleOne.PlayerCamera _playerCamera;

        public MonoPlayerCamera(ScheduleOne.PlayerCamera playerCamera)
        {
            _playerCamera = playerCamera ?? throw new ArgumentNullException(nameof(playerCamera));
        }

        public Transform Transform => _playerCamera.transform;
        public Camera Camera => _playerCamera.Camera;

        public Vector3 Position => _playerCamera.transform.position;
        public Quaternion Rotation => _playerCamera.transform.rotation;

        public void SetPosition(Vector3 position)
        {
            _playerCamera.transform.position = position;
        }

        public void SetRotation(Quaternion rotation)
        {
            _playerCamera.transform.rotation = rotation;
        }

        public void LookAt(Vector3 target)
        {
            _playerCamera.transform.LookAt(target);
        }

        public S1CameraMode CameraMode => Map(_playerCamera.CameraMode);
        public bool FreeCamEnabled => _playerCamera.FreeCamEnabled;
        public bool ViewingAvatar => _playerCamera.ViewingAvatar;

        private S1CameraMode Map(ScheduleOne.PlayerScripts.PlayerCamera.ECameraMode mode) => mode switch
        {
            ScheduleOne.PlayerScripts.PlayerCamera.ECameraMode.Vehicle => S1CameraMode.Vehicle,
            ScheduleOne.PlayerScripts.PlayerCamera.ECameraMode.Skateboard => S1CameraMode.Skateboard,
            _ => S1CameraMode.Default
        };
    }
}
#endif
