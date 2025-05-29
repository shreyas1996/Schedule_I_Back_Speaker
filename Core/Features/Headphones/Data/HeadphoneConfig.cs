using UnityEngine;

namespace BackSpeakerMod.Core.Features.Headphones.Data
{
    /// <summary>
    /// Configuration settings for headphone functionality
    /// </summary>
    public class HeadphoneConfig
    {
        /// <summary>
        /// Asset bundle name containing headphone assets
        /// </summary>
        public string AssetBundleName { get; set; } = "scheduleoneheadphones";

        /// <summary>
        /// Embedded resource name for headphone assets
        /// </summary>
        public string EmbeddedResourceName { get; set; } = "scheduleoneheadphones";

        /// <summary>
        /// Asset name within the bundle
        /// </summary>
        public string AssetName { get; set; } = "ScheduleOneHeadphones";

        /// <summary>
        /// Position offset from player head
        /// </summary>
        public Vector3 PositionOffset { get; set; } = Vector3.zero;

        /// <summary>
        /// Rotation offset from player head
        /// </summary>
        public Vector3 RotationOffset { get; set; } = Vector3.zero;

        /// <summary>
        /// Scale multiplier for headphone model
        /// </summary>
        public Vector3 ScaleMultiplier { get; set; } = Vector3.one;

        /// <summary>
        /// Whether to use local position relative to parent
        /// </summary>
        public bool UseLocalPosition { get; set; } = true;

        /// <summary>
        /// Auto-attach headphones when player spawns
        /// </summary>
        public bool AutoAttachOnSpawn { get; set; } = true;

        /// <summary>
        /// Enable debug visualization
        /// </summary>
        public bool ShowDebugInfo { get; set; } = false;
    }
} 