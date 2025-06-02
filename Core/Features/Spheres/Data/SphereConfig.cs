using UnityEngine;

namespace BackSpeakerMod.Core.Features.Spheres.Data
{
    /// <summary>
    /// Configuration settings for sphere attachment functionality
    /// </summary>
    public class SphereConfig
    {
        /// <summary>
        /// Position offset from player head
        /// </summary>
        public Vector3 PositionOffset { get; set; } = Vector3.zero;

        /// <summary>
        /// Rotation offset from player head
        /// </summary>
        public Vector3 RotationOffset { get; set; } = Vector3.zero;

        /// <summary>
        /// Scale multiplier for sphere model
        /// </summary>
        public Vector3 ScaleMultiplier { get; set; } = Vector3.one * 0.01f;

        /// <summary>
        /// Whether to use local position relative to parent
        /// </summary>
        public bool UseLocalPosition { get; set; } = true;

        /// <summary>
        /// Auto-attach sphere when player spawns
        /// </summary>
        public bool AutoAttachOnSpawn { get; set; } = true;

        /// <summary>
        /// Enable debug visualization
        /// </summary>
        public bool ShowDebugInfo { get; set; } = false;

        // === SPHERE SPECIFIC PROPERTIES ===
        
        /// <summary>
        /// Sphere radius
        /// </summary>
        public float Radius { get; set; } = 0.3f;

        /// <summary>
        /// Sphere color
        /// </summary>
        public Color Color { get; set; } = Color.cyan;

        /// <summary>
        /// Enable glow effect
        /// </summary>
        public bool EnableGlow { get; set; } = true;

        /// <summary>
        /// Glow intensity multiplier
        /// </summary>
        public float GlowIntensity { get; set; } = 2f;

        /// <summary>
        /// Enable sphere rotation
        /// </summary>
        public bool EnableRotation { get; set; } = true;

        /// <summary>
        /// Rotation speed in degrees per second
        /// </summary>
        public float RotationSpeed { get; set; } = 30f;

        /// <summary>
        /// Custom material override (optional)
        /// </summary>
        public Material CustomMaterial { get; set; } = null;
    }
} 