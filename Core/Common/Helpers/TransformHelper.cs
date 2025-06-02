using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;

namespace BackSpeakerMod.Core.Common.Helpers
{
    /// <summary>
    /// Transform position/rotation/scale operations
    /// </summary>
    public static class TransformHelper
    {
        /// <summary>
        /// Reset transform to identity
        /// </summary>
        public static void ResetToIdentity(Transform transform)
        {
            if (transform == null) return;
            
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
            LoggingSystem.Debug($"Reset transform to identity: {transform.name}", "Helper");
        }

        /// <summary>
        /// Copy transform values
        /// </summary>
        public static void CopyTransform(Transform source, Transform target, bool useLocal = true)
        {
            if (source == null || target == null) return;
            
            if (useLocal)
            {
                target.localPosition = source.localPosition;
                target.localRotation = source.localRotation;
                target.localScale = source.localScale;
            }
            else
            {
                target.position = source.position;
                target.rotation = source.rotation;
                target.localScale = source.localScale;
            }
            
            LoggingSystem.Debug($"Copied transform from {source.name} to {target.name}", "Helper");
        }

        /// <summary>
        /// Set transform parent with position preservation
        /// </summary>
        public static void SetParentWithPosition(Transform child, Transform parent, bool worldPositionStays = true)
        {
            if (child == null) return;
            
            child.SetParent(parent, worldPositionStays);
            LoggingSystem.Debug($"Set parent of {child.name} to {parent?.name ?? "null"}", "Helper");
        }

        /// <summary>
        /// Apply position offset
        /// </summary>
        public static void ApplyPositionOffset(Transform transform, Vector3 offset, bool useLocal = true)
        {
            if (transform == null) return;
            
            if (useLocal)
                transform.localPosition += offset;
            else
                transform.position += offset;
                
            LoggingSystem.Debug($"Applied position offset {offset} to {transform.name}", "Helper");
        }

        /// <summary>
        /// Apply rotation offset
        /// </summary>
        public static void ApplyRotationOffset(Transform transform, Vector3 eulerOffset, bool useLocal = true)
        {
            if (transform == null) return;
            
            var rotationOffset = Quaternion.Euler(eulerOffset);
            if (useLocal)
                transform.localRotation *= rotationOffset;
            else
                transform.rotation *= rotationOffset;
                
            LoggingSystem.Debug($"Applied rotation offset {eulerOffset} to {transform.name}", "Helper");
        }
    }
} 