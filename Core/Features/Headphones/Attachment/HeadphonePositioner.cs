using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Features.Headphones.Data;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Common.Helpers;

namespace BackSpeakerMod.Core.Features.Headphones.Attachment
{
    /// <summary>
    /// Handles positioning and configuration of headphone instances
    /// </summary>
    public static class HeadphonePositioner
    {
        /// <summary>
        /// Create and configure headphone instance at attachment point
        /// </summary>
        public static GameObject CreateAndPositionHeadphones(GameObject prefab, Transform attachmentPoint, HeadphoneConfig config)
        {
            if (prefab == null || attachmentPoint == null)
            {
                LoggingSystem.Error("Cannot create headphones - prefab or attachment point is null", "Headphones");
                return null;
            }

            try
            {
                // Instantiate as child of attachment point
                var instance = UnityEngine.Object.Instantiate(prefab, attachmentPoint);
                instance.name = "AttachedHeadphones";

                // Apply configuration
                ApplyTransformConfiguration(instance, config);
                ApplyVisibilityConfiguration(instance);
                
                // Apply URP materials at runtime
                ApplyMaterialConfiguration(instance, config);

                LoggingSystem.Debug($"Created and positioned headphones: {instance.name}", "Headphones");
                return instance;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Exception during positioning: {ex.Message}", "HeadphonePositioner");
                return null;
            }
        }

        /// <summary>
        /// Apply transform configuration (position, rotation, scale)
        /// </summary>
        private static void ApplyTransformConfiguration(GameObject instance, HeadphoneConfig config)
        {
            var transform = instance.transform;

            if (config.UseLocalPosition)
            {
                transform.localPosition = config.PositionOffset;
                transform.localRotation = Quaternion.Euler(config.RotationOffset);
            }
            else
            {
                transform.position += config.PositionOffset;
                transform.rotation *= Quaternion.Euler(config.RotationOffset);
            }

            transform.localScale = Vector3.Scale(transform.localScale, config.ScaleMultiplier);

            LoggingSystem.Debug($"Applied transform config - Pos: {transform.localPosition}, Rot: {transform.localRotation.eulerAngles}, Scale: {transform.localScale}", "Headphones");
        }

        /// <summary>
        /// Apply visibility and rendering configuration
        /// </summary>
        private static void ApplyVisibilityConfiguration(GameObject instance)
        {
            try
            {
                // Ensure object is active
                instance.SetActive(true);

                // Configure renderers
                var renderers = instance.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    renderer.enabled = true;
                    
                    if (FeatureFlags.Headphones.EnableVisibilityDebugging)
                    {
                        LoggingSystem.Debug($"Renderer: {renderer.name}, enabled: {renderer.enabled}, visible: {renderer.isVisible}", "Headphones");
                    }
                }

                LoggingSystem.Debug($"Configured {renderers.Length} renderers for visibility", "Headphones");
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to apply visibility configuration: {ex.Message}", "Headphones");
            }
        }

        /// <summary>
        /// Apply URP material configuration at runtime
        /// </summary>
        private static void ApplyMaterialConfiguration(GameObject instance, HeadphoneConfig config)
        {
            if (!config.ApplyCustomMaterials)
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
                if (FeatureFlags.Headphones.ShowDebugInfo)
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
        private static void LogMaterialDebugInfo(GameObject instance)
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
        /// Log detailed attachment information for debugging
        /// </summary>
        public static void LogAttachmentDetails(GameObject instance, Transform attachmentPoint)
        {
            if (!FeatureFlags.Headphones.ShowDebugInfo) return;

            try
            {
                LoggingSystem.Debug("=== HEADPHONE ATTACHMENT DETAILS ===", "Headphones");
                LoggingSystem.Debug($"Instance: {instance.name}", "Headphones");
                LoggingSystem.Debug($"Attachment Point: {attachmentPoint.name}", "Headphones");
                LoggingSystem.Debug($"Position: {instance.transform.position}", "Headphones");
                LoggingSystem.Debug($"Local Position: {instance.transform.localPosition}", "Headphones");
                LoggingSystem.Debug($"Rotation: {instance.transform.rotation.eulerAngles}", "Headphones");
                LoggingSystem.Debug($"Local Rotation: {instance.transform.localRotation.eulerAngles}", "Headphones");
                LoggingSystem.Debug($"Scale: {instance.transform.localScale}", "Headphones");
                
                var renderers = instance.GetComponentsInChildren<Renderer>();
                LoggingSystem.Debug($"Renderers: {renderers.Length}", "Headphones");
                
                foreach (var renderer in renderers)
                {
                    LoggingSystem.Debug($"  - {renderer.name}: enabled={renderer.enabled}, visible={renderer.isVisible}", "Headphones");
                    if (renderer.material != null)
                    {
                        LoggingSystem.Debug($"    Material: {renderer.material.name} (Shader: {renderer.material.shader.name})", "Headphones");
                    }
                }
                
                LoggingSystem.Debug("=======================================", "Headphones");
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to log attachment details: {ex.Message}", "Headphones");
            }
        }

        /// <summary>
        /// Apply materials to an already-instantiated headphone object
        /// (useful for updating materials without recreating the object)
        /// </summary>
        public static void UpdateMaterials(GameObject instance, HeadphoneConfig config)
        {
            if (instance == null || config == null)
            {
                LoggingSystem.Warning("Cannot update materials - instance or config is null", "Headphones");
                return;
            }

            try
            {
                LoggingSystem.Info($"Updating materials for existing headphone instance: {instance.name}", "Headphones");
                ApplyMaterialConfiguration(instance, config);
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to update materials: {ex.Message}", "Headphones");
            }
        }
    }
} 