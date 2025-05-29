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
        private GameObject currentHeadphoneInstance;
        private Il2CppScheduleOne.PlayerScripts.Player attachedPlayer;
        private bool isAttached;
        private float attachmentTime;
        private HeadphoneState currentState;

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
                return false;
            }

            // Use provided player or find local player
            player ??= Il2CppScheduleOne.PlayerScripts.Player.Local;
            if (player == null)
            {
                LoggingSystem.Warning("Cannot attach headphones - no player found", "Headphones");
                return false;
            }

            // Check if already attached
            if (currentHeadphoneInstance != null)
            {
                LoggingSystem.Info("Headphones already attached, detaching first", "Headphones");
                DetachFromPlayer();
            }

            try
            {
                LoggingSystem.Info($"Attaching headphones to player: {player.name}", "Headphones");

                // Create headphone instance - persist across scenes since it's attached to player
                currentHeadphoneInstance = UnityEngine.Object.Instantiate(headphonePrefab);
                currentHeadphoneInstance.name = "HeadphoneInstance_Attached";
                
                // Mark as persistent since it's attached to player who persists across scenes
                UnityEngine.Object.DontDestroyOnLoad(currentHeadphoneInstance);
                LoggingSystem.Debug($"Created persistent headphone instance: {currentHeadphoneInstance.name}", "Headphones");

                // Position and attach to player
                var headTransform = PlayerHeadDetector.FindAttachmentPoint(player);
                if (headTransform != null)
                {
                    // Use static methods from HeadphonePositioner
                    var positionedInstance = HeadphonePositioner.CreateAndPositionHeadphones(headphonePrefab, headTransform, config);
                    if (positionedInstance != null)
                    {
                        // Replace our simple instance with the properly positioned one
                        UnityEngine.Object.Destroy(currentHeadphoneInstance);
                        currentHeadphoneInstance = positionedInstance;
                        currentHeadphoneInstance.name = "HeadphoneInstance_Attached";
                        
                        // Mark as persistent since it's attached to player who persists across scenes
                        UnityEngine.Object.DontDestroyOnLoad(currentHeadphoneInstance);
                        LoggingSystem.Debug($"Positioned headphone instance: {currentHeadphoneInstance.name}", "Headphones");
                    }
                    LoggingSystem.Info($"✓ Headphones positioned on player head", "Headphones");
                }
                else
                {
                    LoggingSystem.Warning("Could not find player head - attaching to player root", "Headphones");
                    currentHeadphoneInstance.transform.SetParent(player.transform);
                    currentHeadphoneInstance.transform.localPosition = Vector3.zero;
                }

                // Update state
                attachedPlayer = player;
                isAttached = true;
                attachmentTime = Time.time;

                // Create and update state
                if (currentState == null)
                    currentState = new HeadphoneState();
                
                // Update state manually since UpdateAttachment doesn't exist
                currentState.IsAttached = true;
                currentState.AttachedObject = currentHeadphoneInstance;
                currentState.AttachedTo = headTransform ?? player.transform;
                currentState.AttachmentTime = attachmentTime;

                LoggingSystem.Info($"✓ Headphones successfully attached to {player.name}", "Headphones");
                return true;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to attach headphones: {ex.Message}", "Headphones");
                
                // Cleanup on failure
                if (currentHeadphoneInstance != null)
                {
                    UnityEngine.Object.Destroy(currentHeadphoneInstance);
                    currentHeadphoneInstance = null;
                }
                
                isAttached = false;
                attachedPlayer = null;
                return false;
            }
        }

        /// <summary>
        /// Detach headphones from player
        /// </summary>
        public void DetachFromPlayer()
        {
            if (!isAttached || currentHeadphoneInstance == null)
            {
                LoggingSystem.Debug("No headphones to detach", "Headphones");
                return;
            }

            try
            {
                LoggingSystem.Info($"Detaching headphones from {attachedPlayer?.name ?? "player"}", "Headphones");

                // Destroy the headphone instance
                if (currentHeadphoneInstance != null)
                {
                    UnityEngine.Object.Destroy(currentHeadphoneInstance);
                    currentHeadphoneInstance = null;
                }

                // Clear state
                isAttached = false;
                attachedPlayer = null;
                attachmentTime = 0f;
                
                if (currentState != null)
                {
                    currentState.Reset();
                }

                LoggingSystem.Info("✓ Headphones detached successfully", "Headphones");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error detaching headphones: {ex.Message}", "Headphones");
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
        /// Get current attachment state
        /// </summary>
        public HeadphoneState GetState() => currentState ?? new HeadphoneState();

        /// <summary>
        /// Check if headphones are currently attached
        /// </summary>
        public bool IsAttached => isAttached && currentHeadphoneInstance != null;

        /// <summary>
        /// Get simple status
        /// </summary>
        public string GetStatus()
        {
            if (!FeatureFlags.Headphones.Enabled)
                return "Disabled";
                
            if (!IsAttached)
                return "Not attached";
                
            return $"Attached to {attachedPlayer?.name ?? "Unknown"}";
        }
    }
} 