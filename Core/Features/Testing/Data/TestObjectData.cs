using UnityEngine;
using System;

namespace BackSpeakerMod.Core.Features.Testing.Data
{
    /// <summary>
    /// State enumeration for test objects
    /// </summary>
    public enum TestObjectStatus
    {
        Inactive,
        Active,
        Rotating,
        Glowing,
        Error
    }

    /// <summary>
    /// State tracking for test objects
    /// </summary>
    public class TestObjectState
    {
        public bool IsActive { get; set; } = false;
        public GameObject GameObject { get; set; } = null;
        public Vector3 SpawnPosition { get; set; } = Vector3.zero;
        public Quaternion SpawnRotation { get; set; } = Quaternion.identity;
        public float SpawnTime { get; set; } = 0f;
        public object Config { get; set; } = null;
        public TestObjectStatus Status { get; set; } = TestObjectStatus.Inactive;

        public void Reset()
        {
            IsActive = false;
            if (GameObject != null)
            {
                UnityEngine.Object.Destroy(GameObject);
                GameObject = null;
            }
            SpawnPosition = Vector3.zero;
            SpawnRotation = Quaternion.identity;
            SpawnTime = 0f;
            Config = null;
            Status = TestObjectStatus.Inactive;
        }

        public string GetStatusString()
        {
            return $"Status: {Status}, Active: {IsActive}, SpawnTime: {SpawnTime:F2}";
        }
    }

    /// <summary>
    /// Configuration for glowing sphere test objects
    /// </summary>
    public class GlowingSphereConfig
    {
        public string Name { get; set; } = "GlowingSphere";
        public int Layer { get; set; } = 0;
        public Color PrimaryColor { get; set; } = Color.yellow;
        public Color EmissionColor { get; set; } = Color.yellow;
        public float EmissionIntensity { get; set; } = 1.0f;
        public float GlowIntensity { get; set; } = 1.0f;
        public Color GlowColor { get; set; } = Color.yellow;
        public float RotationSpeed { get; set; } = 45.0f;
        public float PulseSpeed { get; set; } = 2.0f;
        public bool EnablePulsing { get; set; } = true;
        public bool EnableRotation { get; set; } = true;
        public Vector3 Scale { get; set; } = Vector3.one;
    }

    /// <summary>
    /// Configuration for test cube objects
    /// </summary>
    public class TestCubeConfig
    {
        public string Name { get; set; } = "TestCube";
        public int Layer { get; set; } = 0;
        public Color PrimaryColor { get; set; } = Color.blue;
        public Color CubeColor { get; set; } = Color.blue;
        public Vector3 Scale { get; set; } = Vector3.one;
        public bool EnablePhysics { get; set; } = true;
        public float Mass { get; set; } = 1.0f;
    }
}