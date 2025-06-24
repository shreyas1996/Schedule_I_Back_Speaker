#if !IL2CPP
using System;
using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoAvatar : IAvatar
    {
        private readonly object _avatar;

        public MonoAvatar(object avatar)
        {
            _avatar = avatar ?? throw new ArgumentNullException(nameof(avatar));
        }

        public Transform Transform
        {
            get
            {
                try
                {
                    var property = _avatar.GetType().GetProperty("transform");
                    return property?.GetValue(_avatar) as Transform;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        public Transform? HeadBone 
        { 
            get 
            { 
                try
                {
                    // In Mono, HeadBone is a field, not a property
                    var field = _avatar.GetType().GetField("HeadBone");
                    return field?.GetValue(_avatar) as Transform;
                }
                catch (Exception)
                {
                    return null;
                }
            } 
        }
        
        public void SetPosition(Vector3 position)
        {
            try
            {
                var transform = Transform;
                if (transform != null)
                {
                    transform.position = position;
                }
            }
            catch (Exception)
            {
                // Handle reflection failures silently
            }
        }
        
        public Vector3 GetPosition()
        {
            try
            {
                var transform = Transform;
                return transform != null ? transform.position : Vector3.zero;
            }
            catch (Exception)
            {
                return Vector3.zero;
            }
        }
        
        public void SetRotation(Quaternion rotation)
        {
            try
            {
                var transform = Transform;
                if (transform != null)
                {
                    transform.rotation = rotation;
                }
            }
            catch (Exception)
            {
                // Handle reflection failures silently
            }
        }
        
        public Quaternion GetRotation()
        {
            try
            {
                var transform = Transform;
                return transform != null ? transform.rotation : Quaternion.identity;
            }
            catch (Exception)
            {
                return Quaternion.identity;
            }
        }
    }
}
#endif
