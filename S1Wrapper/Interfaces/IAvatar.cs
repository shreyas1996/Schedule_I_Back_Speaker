using UnityEngine;

namespace BackSpeakerMod.S1Wrapper.Interfaces
{
    public interface IAvatar
    {
        Transform Transform { get; }
        
        void SetPosition(Vector3 position);
        Vector3 GetPosition();
        void SetRotation(Quaternion rotation);
        Quaternion GetRotation();
        
        // Legacy property for compatibility
        Transform? HeadBone { get; }
    }
}
