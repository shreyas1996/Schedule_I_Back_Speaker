using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Features.Headphones.Data;
using BackSpeakerMod.Core.Common.Helpers;
using System;

namespace BackSpeakerMod.Core.Features.Headphones.Attachment
{
    /// <summary>
    /// Coordinates headphone attachment to player
    /// </summary>
    public class HeadphoneAttachment
    {
        private readonly HeadphoneConfig config;
        private readonly HeadphoneState state;

        /// <summary>
        /// Initialize headphone attachment system
        /// </summary>
        public HeadphoneAttachment(HeadphoneConfig headphoneConfig = null)
        {
            config = headphoneConfig ?? new HeadphoneConfig();
            state = new HeadphoneState();
            LoggingSystem.Info("HeadphoneAttachment system initialized", "Headphones");
        }

        /// <summary>
        /// Attach headphones to player's head
        /// </summary>
        public bool AttachToPlayer(GameObject headphonePrefab, Il2CppScheduleOne.PlayerScripts.Player player = null)
        {
            if (!FeatureFlags.Headphones.Enabled)
            {
                LoggingSystem.Warning("Headphones feature is disabled", "Headphones");
                return false;
            }

            if (headphonePrefab == null)
            {
                LoggingSystem.Error("Cannot attach headphones - prefab is null", "Headphones");
                HeadphoneEvents.FireAttachmentFailed("Headphone prefab is null");
                return false;
            }

            // Use provided player or find local player
            player ??= Il2CppScheduleOne.PlayerScripts.Player.Local;
            if (player == null)
            {
                LoggingSystem.Warning("Cannot find player to attach headphones", "Headphones");
                HeadphoneEvents.FireAttachmentFailed("Player not found");
                return false;
            }

            try
            {
                // Remove existing headphones first
                DetachFromPlayer();

                // Find attachment point using detector
                var attachmentPoint = PlayerHeadDetector.FindAttachmentPoint(player);
                if (!PlayerHeadDetector.IsValidAttachmentPoint(attachmentPoint))
                {
                    LoggingSystem.Warning("No valid attachment point found on player", "Headphones");
                    HeadphoneEvents.FireAttachmentFailed("No valid attachment point found");
                    return false;
                }

                // Create and position headphones using positioner
                var headphoneInstance = HeadphonePositioner.CreateAndPositionHeadphones(headphonePrefab, attachmentPoint, config);
                if (headphoneInstance == null)
                {
                    LoggingSystem.Error("Failed to create headphone instance", "Headphones");
                    HeadphoneEvents.FireAttachmentFailed("Failed to instantiate headphones");
                    return false;
                }

                // Update state
                UpdateAttachmentState(headphoneInstance, attachmentPoint);

                LoggingSystem.Info($"Successfully attached headphones to {player.name}", "Headphones");
                
                if (FeatureFlags.Headphones.ShowDebugInfo)
                {
                    HeadphonePositioner.LogAttachmentDetails(headphoneInstance, attachmentPoint);
                }

                // Fire events
                HeadphoneEvents.FireAttached(headphoneInstance);
                HeadphoneEvents.FireStateChanged(state);

                return true;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Exception during headphone attachment: {ex.Message}", "Headphones");
                HeadphoneEvents.FireAttachmentFailed($"Exception: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Detach headphones from player
        /// </summary>
        public void DetachFromPlayer()
        {
            if (!state.IsAttached || state.AttachedObject == null)
            {
                LoggingSystem.Debug("No headphones to detach", "Headphones");
                return;
            }

            try
            {
                LoggingSystem.Info("Detaching headphones from player", "Headphones");
                
                UnityEngine.Object.Destroy(state.AttachedObject);
                state.Reset();

                HeadphoneEvents.FireDetached();
                HeadphoneEvents.FireStateChanged(state);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Exception during headphone detachment: {ex.Message}", "Headphones");
                state.Reset(); // Reset state anyway
            }
        }

        /// <summary>
        /// Toggle headphones on/off
        /// </summary>
        public bool ToggleAttachment(GameObject headphonePrefab, Il2CppScheduleOne.PlayerScripts.Player player = null)
        {
            if (state.IsAttached)
            {
                DetachFromPlayer();
                return false;
            }
            else
            {
                return AttachToPlayer(headphonePrefab, player);
            }
        }

        /// <summary>
        /// Update attachment state with new instance
        /// </summary>
        private void UpdateAttachmentState(GameObject headphoneInstance, Transform attachmentPoint)
        {
            state.IsAttached = true;
            state.AttachedObject = headphoneInstance;
            state.AttachedTo = attachmentPoint;
            state.AttachmentTime = Time.time;
            state.OriginalPosition = headphoneInstance.transform.position;
            state.OriginalRotation = headphoneInstance.transform.rotation;
            state.OriginalScale = headphoneInstance.transform.localScale;
        }

        /// <summary>
        /// Get current attachment state
        /// </summary>
        public HeadphoneState GetState() => state;

        /// <summary>
        /// Check if headphones are currently attached
        /// </summary>
        public bool IsAttached => state.IsAttached;

        /// <summary>
        /// Get status string
        /// </summary>
        public string GetStatus() => state.GetStatusString();
    }
} 