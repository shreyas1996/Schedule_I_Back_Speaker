#if !IL2CPP
using System;
using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoPlayer : IPlayer
    {
        private readonly ScheduleOne.Player _player;

        public MonoPlayer(ScheduleOne.Player player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public Transform Transform => _player.transform;
        public GameObject GameObject => _player.gameObject;

        public Vector3 Position => _player.transform.position;
        public Quaternion Rotation => _player.transform.rotation;

        public void SetPosition(Vector3 position)
        {
            _player.transform.position = position;
        }

        public void SetRotation(Quaternion rotation)
        {
            _player.transform.rotation = rotation;
        }

        public string Name => _player.name;
        public IAvatar? Avatar => _player.Avatar != null ? new MonoAvatar(_player.Avatar) : null;
    }
}
#endif
