using UnityEngine;
using System;

namespace BackSpeakerMod.Configuration
{
    /// <summary>
    /// Status of headphone attachment
    /// </summary>
    public enum HeadphoneAttachmentStatus
    {
        Detached,
        Attaching,
        Attached,
        Detaching,
        Error
    }

    /// <summary>
    /// Runtime state of headphone attachment
    /// </summary>
    public class HeadphoneState
    {
        /// <summary>
        /// The headphone object that is attached
        /// </summary>
        public GameObject? AttachedObject { get; set; }

        /// <summary>
        /// What the headphones are attached to (player)
        /// </summary>
        public Transform? AttachedTo { get; set; }

        /// <summary>
        /// Current attachment status
        /// </summary>
        public HeadphoneAttachmentStatus Status { get; set; }

        /// <summary>
        /// Initialize new headphone state
        /// </summary>
        public HeadphoneState()
        {
            AttachedObject = null;
            AttachedTo = null;
            Status = HeadphoneAttachmentStatus.Detached;
        }

        public bool IsAttached { get; set; }
        public Vector3 OriginalPosition { get; set; }
        public Quaternion OriginalRotation { get; set; }
        public Vector3 OriginalScale { get; set; }
        public float AttachmentTime { get; set; }

        public void Reset()
        {
            IsAttached = false;
            AttachedObject = null;
            AttachedTo = null;
            OriginalPosition = Vector3.zero;
            OriginalRotation = Quaternion.identity;
            OriginalScale = Vector3.one;
            AttachmentTime = 0f;
        }

        public string GetStatusString()
        {
            if (!IsAttached)
                return "Headphones not attached";
            
            if (AttachedObject == null)
                return "Headphones attached but object missing";
            
            var timeSince = Time.time - AttachmentTime;
            return $"Headphones attached for {timeSince:F1}s to {AttachedTo?.name ?? "unknown"}";
        }

        // /// <summary>
        // /// Event fired when headphones are attached
        // /// </summary>
        // public event Action<GameObject>? OnAttached;

        // /// <summary>
        // /// Event fired when headphones are detached
        // /// </summary>
        // public event Action? OnDetached;

        // /// <summary>
        // /// Event fired when attachment fails
        // /// </summary>
        // public event Action<string>? OnAttachmentFailed;

        // /// <summary>
        // /// Event fired when state changes
        // /// </summary>
        // public event Action<HeadphoneAttachmentStatus>? OnStateChanged;
    }

    /// <summary>
    /// Events related to headphone attachment
    /// </summary>
    public static class HeadphoneEvents
    {
        public static event Action<GameObject>? OnAttached;
        public static event Action? OnDetached;
        public static event Action<string>? OnAttachmentFailed;
        public static event Action<HeadphoneState>? OnStateChanged;

        public static void FireAttached(GameObject headphones)
        {
            OnAttached?.Invoke(headphones);
        }

        public static void FireDetached()
        {
            OnDetached?.Invoke();
        }

        public static void FireAttachmentFailed(string reason)
        {
            OnAttachmentFailed?.Invoke(reason);
        }

        public static void FireStateChanged(HeadphoneState state)
        {
            OnStateChanged?.Invoke(state);
        }
    }
} 