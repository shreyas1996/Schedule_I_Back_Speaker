#if !IL2CPP
using System;
using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;
using ScheduleOne.AvatarFramework;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoAvatar : IAvatar
    {
        private readonly Avatar _avatar;

        public MonoAvatar(Avatar avatar)
        {
            _avatar = avatar ?? throw new ArgumentNullException(nameof(avatar));
        }

        public Transform Transform
        {
            get
            {
                return _avatar.transform;
            }
        }

        public Transform? HeadBone 
        { 
            get 
            { 
                return _avatar.HeadBone;
            } 
        }
        
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
