using UnityEngine;

namespace BackSpeakerMod.Core.Features.Testing.Components
{
    /// <summary>
    /// Simple rotation component for test spheres
    /// </summary>
    public class TestSphereRotator : MonoBehaviour
    {
        public float rotationSpeed = 50f;

        void Update()
        {
            transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }
} 