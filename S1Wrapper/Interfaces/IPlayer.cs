using UnityEngine;

namespace BackSpeakerMod.S1Wrapper.Interfaces
{
    public interface IPlayer
    {
        Transform Transform { get; }
        GameObject GameObject { get; }
        string Name { get; }
        IAvatar? Avatar { get; }
    }
}
