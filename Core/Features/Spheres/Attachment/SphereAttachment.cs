using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Features.Spheres.Data;
using BackSpeakerMod.Core.Features.Spheres.Loading;
using BackSpeakerMod.Core.Common.Helpers;
using System;

namespace BackSpeakerMod.Core.Features.Spheres.Attachment
{
    /// <summary>
    /// Handles sphere attachment to player head or surfaces
    /// </summary>
    public class SphereAttachment
    {
        private readonly SphereConfig config;
        private readonly SphereState state;
        private readonly PlayerHeadDetector headDetector;

        /// <summary>
        /// Initialize sphere attachment system
        /// </summary>
        public SphereAttachment(SphereConfig sphereConfig = null)
        {
            config = sphereConfig ?? new SphereConfig();
            state = new SphereState();
            headDetector = new PlayerHeadDetector();
            
            LoggingSystem.Info("SphereAttachment initialized", "SphereAttachment");
        }

        /// <summary>
        /// Current sphere state
        /// </summary>
        public SphereState State => state;

        /// <summary>
        /// Check if sphere is currently attached
        /// </summary>
        public bool IsAttached => state.IsAttached;

        /// <summary>
        /// Attach sphere to player's head
        /// </summary>
        public bool AttachToPlayer()
        {
            try
            {
                if (state.IsAttached)
                {
                    LoggingSystem.Warning("Sphere already attached", "SphereAttachment");
                    return true;
                }

                state.Status = SphereAttachmentStatus.Attaching;
                SphereEvents.TriggerStateChanged(state);

                // Find player head
                var headTransform = headDetector.FindPlayerHead();
                if (headTransform == null)
                {
                    var error = "Could not find player head for sphere attachment";
                    LoggingSystem.Error(error, "SphereAttachment");
                    state.Status = SphereAttachmentStatus.Failed;
                    state.LastError = error;
                    SphereEvents.TriggerAttachmentFailed(error);
                    return false;
                }

                // Create sphere
                var sphereObject = SphereAssetLoader.CreateSphere(config);
                if (sphereObject == null)
                {
                    var error = "Failed to create sphere object";
                    LoggingSystem.Error(error, "SphereAttachment");
                    state.Status = SphereAttachmentStatus.Failed;
                    state.LastError = error;
                    SphereEvents.TriggerAttachmentFailed(error);
                    return false;
                }

                // Attach to head
                sphereObject.transform.SetParent(headTransform, false);
                
                // Apply offsets
                if (config.UseLocalPosition)
                {
                    sphereObject.transform.localPosition = config.PositionOffset;
                    sphereObject.transform.localRotation = Quaternion.Euler(config.RotationOffset);
                }
                else
                {
                    sphereObject.transform.position = headTransform.position + config.PositionOffset;
                    sphereObject.transform.rotation = headTransform.rotation * Quaternion.Euler(config.RotationOffset);
                }

                // Update state
                state.Status = SphereAttachmentStatus.Attached;
                state.SphereObject = sphereObject;
                state.AttachedTo = headTransform;
                state.AttachPosition = sphereObject.transform.position;
                state.AttachRotation = sphereObject.transform.rotation;
                state.IsAttachedToPlayer = true;
                state.IsAttachedToSurface = false;
                state.AttachTime = Time.time;
                state.LastError = "";

                LoggingSystem.Info($"Sphere attached to player head at {state.AttachPosition}", "SphereAttachment");
                SphereEvents.TriggerAttached(state);
                SphereEvents.TriggerStateChanged(state);

                return true;
            }
            catch (Exception ex)
            {
                var error = $"Exception during sphere attachment: {ex.Message}";
                LoggingSystem.Error(error, "SphereAttachment");
                state.Status = SphereAttachmentStatus.Failed;
                state.LastError = error;
                SphereEvents.TriggerAttachmentFailed(error);
                return false;
            }
        }

        /// <summary>
        /// Attach sphere to a specific surface position
        /// </summary>
        public bool AttachToSurface(Vector3 position, Vector3 normal)
        {
            try
            {
                if (state.IsAttached)
                {
                    LoggingSystem.Warning("Sphere already attached", "SphereAttachment");
                    return true;
                }

                state.Status = SphereAttachmentStatus.Attaching;
                SphereEvents.TriggerStateChanged(state);

                // Create sphere
                var sphereObject = SphereAssetLoader.CreateSphere(config);
                if (sphereObject == null)
                {
                    var error = "Failed to create sphere object";
                    LoggingSystem.Error(error, "SphereAttachment");
                    state.Status = SphereAttachmentStatus.Failed;
                    state.LastError = error;
                    SphereEvents.TriggerAttachmentFailed(error);
                    return false;
                }

                // Position sphere on surface
                var surfaceOffset = normal * config.Radius; // Offset by sphere radius
                sphereObject.transform.position = position + surfaceOffset + config.PositionOffset;
                sphereObject.transform.rotation = Quaternion.LookRotation(-normal) * Quaternion.Euler(config.RotationOffset);

                // Update state
                state.Status = SphereAttachmentStatus.Attached;
                state.SphereObject = sphereObject;
                state.AttachedTo = null; // No parent for surface attachment
                state.AttachPosition = sphereObject.transform.position;
                state.AttachRotation = sphereObject.transform.rotation;
                state.IsAttachedToPlayer = false;
                state.IsAttachedToSurface = true;
                state.AttachTime = Time.time;
                state.LastError = "";

                LoggingSystem.Info($"Sphere attached to surface at {position}", "SphereAttachment");
                SphereEvents.TriggerAttached(state);
                SphereEvents.TriggerStateChanged(state);

                return true;
            }
            catch (Exception ex)
            {
                var error = $"Exception during surface attachment: {ex.Message}";
                LoggingSystem.Error(error, "SphereAttachment");
                state.Status = SphereAttachmentStatus.Failed;
                state.LastError = error;
                SphereEvents.TriggerAttachmentFailed(error);
                return false;
            }
        }

        /// <summary>
        /// Detach current sphere
        /// </summary>
        public bool Detach()
        {
            try
            {
                if (!state.IsAttached)
                {
                    LoggingSystem.Warning("No sphere to detach", "SphereAttachment");
                    return true;
                }

                LoggingSystem.Info($"Detaching sphere from {state.GetAttachmentType()}", "SphereAttachment");

                // Set detaching status
                state.Status = state.IsAttachedToPlayer ? 
                    SphereAttachmentStatus.DetachingFromPlayer : 
                    SphereAttachmentStatus.DetachingFromSurface;
                SphereEvents.TriggerStateChanged(state);

                // Destroy sphere object
                if (state.SphereObject != null)
                {
                    SphereAssetLoader.DestroySphere(state.SphereObject);
                }

                // Trigger events before reset
                SphereEvents.TriggerDetached(state);

                // Reset state
                state.Reset();
                SphereEvents.TriggerStateChanged(state);

                LoggingSystem.Info("Sphere detached successfully", "SphereAttachment");
                return true;
            }
            catch (Exception ex)
            {
                var error = $"Exception during sphere detachment: {ex.Message}";
                LoggingSystem.Error(error, "SphereAttachment");
                state.LastError = error;
                return false;
            }
        }

        /// <summary>
        /// Toggle sphere attachment to player
        /// </summary>
        public bool TogglePlayerAttachment()
        {
            if (state.IsAttachedToPlayer)
            {
                return Detach();
            }
            else
            {
                if (state.IsAttachedToSurface)
                {
                    Detach(); // Detach from surface first
                }
                return AttachToPlayer();
            }
        }

        /// <summary>
        /// Get attachment status description
        /// </summary>
        public string GetStatusDescription() => state.GetStatusDescription();

        /// <summary>
        /// Check if sphere object is valid
        /// </summary>
        public bool IsSphereValid() => SphereAssetLoader.IsSphereValid(state.SphereObject);

        /// <summary>
        /// Update sphere configuration
        /// </summary>
        public void UpdateConfiguration(SphereConfig newConfig)
        {
            if (newConfig == null) return;

            // Update internal config (for next attachment)
            // Note: This would require copying properties since config is readonly
            
            // Update current sphere if attached
            if (state.IsAttached && state.SphereObject != null)
            {
                SphereAssetLoader.UpdateSphereConfig(state.SphereObject, newConfig);
            }
        }
    }
} 