using UnityEngine;

namespace BackSpeakerMod.S1Wrapper.Interfaces
{
    public interface IGameObject
    {
        GameObject GameObject { get; }
        Transform Transform { get; }
    }
}
