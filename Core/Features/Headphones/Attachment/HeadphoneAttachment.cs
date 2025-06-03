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
        private GameObject? currentHeadphoneInstance;
        private Il2CppScheduleOne.PlayerScripts.Player? attachedPlayer;
        private bool isAttached;
        private float attachmentTime;
        private HeadphoneState? currentState;

        /// <summary>
        /// Initialize headphone attachment system
        /// </summary>
        public HeadphoneAttachment(HeadphoneConfig? headphoneConfig = null)
        {
            config = headphoneConfig ?? new HeadphoneConfig();
            state = new HeadphoneState();
            LoggingSystem.Info("HeadphoneAttachment system initialized", "Headphones");
        }

        /// <summary>
        /// Attach headphones to player's head
        /// </summary>
        public bool AttachToPlayer(GameObject headphonePrefab, Il2CppScheduleOne.PlayerScripts.Player? player = null)
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
                var headTransform = PlayerHeadDetector.FindHeadphoneAttachmentPoint(player);
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
                    
                    // Apply URP materials when attached to player root (fallback case)
                    ApplyMaterialConfiguration(currentHeadphoneInstance);
                }

                // Always apply materials to ensure they're correct regardless of attachment method
                if (currentHeadphoneInstance != null)
                {
                    ApplyMaterialConfiguration(currentHeadphoneInstance);
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
        /// Apply URP material configuration to headphone instance
        /// </summary>
        private void ApplyMaterialConfiguration(GameObject instance)
        {
            if (!config.ApplyCustomMaterials || !FeatureFlags.Headphones.ApplyCustomMaterials)
            {
                LoggingSystem.Debug("Custom material application disabled, skipping", "Headphones");
                return;
            }

            try
            {
                LoggingSystem.Info($"Applying runtime URP materials to headphones: {instance.name}", "Headphones");

                // Use the URPMaterialHelper to apply all configured materials
                URPMaterialHelper.ApplyMaterialsToGameObject(instance, config);

                // Log material debug information if enabled
                if (FeatureFlags.Headphones.ShowDebugInfo || FeatureFlags.Headphones.EnableMaterialDebugging)
                {
                    LogMaterialDebugInfo(instance);
                }

                LoggingSystem.Info("✓ Runtime URP material application completed", "Headphones");
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to apply material configuration: {ex.Message}", "Headphones");
            }
        }

        /// <summary>
        /// Log material debug information
        /// </summary>
        private void LogMaterialDebugInfo(GameObject instance)
        {
            try
            {
                LoggingSystem.Debug("=== HEADPHONE MATERIAL DEBUG INFO ===", "Headphones");
                
                var renderers = instance.GetComponentsInChildren<Renderer>();
                for (int i = 0; i < renderers.Length; i++)
                {
                    var renderer = renderers[i];
                    var material = renderer.material;
                    
                    LoggingSystem.Debug($"Renderer {i}: {renderer.name}", "Headphones");
                    if (material != null)
                    {
                        URPMaterialHelper.LogMaterialProperties(material);
                        URPMaterialHelper.ValidateURPMaterial(material);
                    }
                    else
                    {
                        LoggingSystem.Warning($"Renderer {i} has null material", "Headphones");
                    }
                }
                
                LoggingSystem.Debug("====================================", "Headphones");
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to log material debug info: {ex.Message}", "Headphones");
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
        public bool ToggleAttachment(GameObject? headphonePrefab, Il2CppScheduleOne.PlayerScripts.Player? player = null)
        {
            if (state.IsAttached)
            {
                DetachFromPlayer();
                return false;
            }
            else
            {
                // if (headphonePrefab == null)
                // {
                //     LoggingSystem.Error("Cannot attach headphones - prefab is null", "Headphones");
                //     return false;
                // }
                return AttachToPlayer(headphonePrefab!, player);
            }
        }

        /// <summary>
        /// Update materials on existing headphone instance
        /// </summary>
        public void UpdateMaterials()
        {
            if (currentHeadphoneInstance != null)
            {
                LoggingSystem.Info("Updating materials on existing headphone instance", "Headphones");
                ApplyMaterialConfiguration(currentHeadphoneInstance);
            }
            else
            {
                LoggingSystem.Warning("Cannot update materials - no headphone instance attached", "Headphones");
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