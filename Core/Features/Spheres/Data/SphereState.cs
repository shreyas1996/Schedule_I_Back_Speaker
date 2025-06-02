using UnityEngine;

namespace BackSpeakerMod.Core.Features.Spheres.Data
{
    /// <summary>
    /// Sphere attachment status
    /// </summary>
    public enum SphereAttachmentStatus
    {
        Detached,
        Attaching,
        Attached,
        DetachingFromPlayer,
        DetachingFromSurface,
        Failed
    }

    /// <summary>
    /// State management for sphere attachment
    /// </summary>
    public class SphereState
    {
        public SphereAttachmentStatus Status { get; set; } = SphereAttachmentStatus.Detached;
        public GameObject SphereObject { get; set; } = null;
        public Transform AttachedTo { get; set; } = null;
        public Vector3 AttachPosition { get; set; } = Vector3.zero;
        public Quaternion AttachRotation { get; set; } = Quaternion.identity;
        public bool IsAttachedToPlayer { get; set; } = false;
        public bool IsAttachedToSurface { get; set; } = false;
        public string LastError { get; set; } = "";
        public float AttachTime { get; set; } = 0f;

        /// <summary>
        /// Reset state to default values
        /// </summary>
        public void Reset()
        {
            Status = SphereAttachmentStatus.Detached;
            SphereObject = null;
            AttachedTo = null;
            AttachPosition = Vector3.zero;
            AttachRotation = Quaternion.identity;
            IsAttachedToPlayer = false;
            IsAttachedToSurface = false;
            LastError = "";
            AttachTime = 0f;
        }

        /// <summary>
        /// Check if sphere is currently attached
        /// </summary>
        public bool IsAttached => Status == SphereAttachmentStatus.Attached;

        /// <summary>
        /// Get attachment type description
        /// </summary>
        public string GetAttachmentType()
        {
            if (IsAttachedToPlayer) return "Player";
            if (IsAttachedToSurface) return "Surface";
            return "None";
        }

        /// <summary>
        /// Get status description
        /// </summary>
        public string GetStatusDescription()
        {
            return Status switch
            {
                SphereAttachmentStatus.Detached => "No sphere attached",
                SphereAttachmentStatus.Attaching => "Attaching sphere...",
                SphereAttachmentStatus.Attached => $"Sphere attached to {GetAttachmentType()}",
                SphereAttachmentStatus.DetachingFromPlayer => "Detaching from player...",
                SphereAttachmentStatus.DetachingFromSurface => "Detaching from surface...",
                SphereAttachmentStatus.Failed => $"Attachment failed: {LastError}",
                _ => "Unknown status"
            };
        }
    }

    /// <summary>
    /// Events for sphere attachment system
    /// </summary>
    public static class SphereEvents
    {
        public static event global::System.Action<SphereState> OnSphereAttached;
        public static event global::System.Action<SphereState> OnSphereDetached;
        public static event global::System.Action<string> OnSphereAttachmentFailed;
        public static event global::System.Action<SphereState> OnSphereStateChanged;

        public static void TriggerAttached(SphereState state) => OnSphereAttached?.Invoke(state);
        public static void TriggerDetached(SphereState state) => OnSphereDetached?.Invoke(state);
        public static void TriggerAttachmentFailed(string error) => OnSphereAttachmentFailed?.Invoke(error);
        public static void TriggerStateChanged(SphereState state) => OnSphereStateChanged?.Invoke(state);
    }
} 