#if !IL2CPP
using System;
using BackSpeakerMod.S1Wrapper.Interfaces;
using UnityEngine;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoAvatar : IAvatar
    {
        private readonly ScheduleOne.Avatar _avatar;

        public MonoAvatar(ScheduleOne.Avatar avatar)
        {
            _avatar = avatar ?? throw new ArgumentNullException(nameof(avatar));
        }

        public Transform Transform => _avatar.transform;
        
        public void SetPosition(Vector3 position)
        {
            _avatar.transform.position = position;
        }
        
        public Vector3 GetPosition()
        {
            return _avatar.transform.position;
        }
        
        public void SetRotation(Quaternion rotation)
        {
            _avatar.transform.rotation = rotation;
        }
        
        public Quaternion GetRotation()
        {
            return _avatar.transform.rotation;
        }
    }
}
#endif
