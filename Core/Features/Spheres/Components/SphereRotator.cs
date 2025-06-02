using UnityEngine;

namespace BackSpeakerMod.Core.Features.Spheres.Components
{
    /// <summary>
    /// Component to handle sphere rotation animation
    /// </summary>
    public class SphereRotator : MonoBehaviour
    {
        private float rotationSpeed = 30f;

        /// <summary>
        /// Rotation speed in degrees per second
        /// </summary>
        public float RotationSpeed 
        { 
            get => rotationSpeed; 
            set => rotationSpeed = value; 
        }

        void Update()
        {
            if (rotationSpeed != 0f)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
            }
        }

        /// <summary>
        /// Set rotation axis (default is Y-axis up)
        /// </summary>
        public void SetRotationAxis(Vector3 axis)
        {
            // Future enhancement - for now just rotate on Y axis
        }

        /// <summary>
        /// Stop rotation
        /// </summary>
        public void StopRotation()
        {
            rotationSpeed = 0f;
        }

        /// <summary>
        /// Start rotation with specified speed
        /// </summary>
        public void StartRotation(float speed)
        {
            rotationSpeed = speed;
        }
    }
} 