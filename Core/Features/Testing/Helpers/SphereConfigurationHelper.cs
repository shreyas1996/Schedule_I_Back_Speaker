using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Features.Testing.Data;
using BackSpeakerMod.Core.Features.Testing.Components;
using System;

namespace BackSpeakerMod.Core.Features.Testing.Helpers
{
    /// <summary>
    /// Helper class for configuring sphere materials and components
    /// </summary>
    public static class SphereConfigurationHelper
    {
        /// <summary>
        /// Configure material with emission and glow
        /// </summary>
        public static void ConfigureMaterial(GameObject sphere, GlowingSphereConfig config)
        {
            var renderer = sphere.GetComponent<Renderer>();
            if (renderer == null)
            {
                LoggingSystem.Warning("Sphere has no renderer component", "Testing");
                return;
            }

            try
            {
                // Create emissive material
                var material = new Material(Shader.Find("Standard"));
                
                // Base color
                material.color = config.PrimaryColor;
                
                // Emission settings
                if (config.EmissionColor != Color.clear)
                {
                    material.SetColor("_EmissionColor", config.EmissionColor * config.EmissionIntensity);
                    material.EnableKeyword("_EMISSION");
                }
                
                // Surface properties for better visibility
                material.SetFloat("_Metallic", 0f);
                material.SetFloat("_Glossiness", 1f);
                
                renderer.material = material;
                
                LoggingSystem.Debug($"Configured sphere material with emission: {config.EmissionColor}", "Testing");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to configure sphere material: {ex.Message}", "Testing");
            }
        }

        /// <summary>
        /// Configure rotation animation if enabled
        /// </summary>
        public static void ConfigureRotation(GameObject sphere, GlowingSphereConfig config)
        {
            if (!config.EnableRotation)
            {
                return;
            }

            try
            {
                var rotator = sphere.AddComponent<TestSphereRotator>();
                rotator.rotationSpeed = config.RotationSpeed;
                LoggingSystem.Debug($"Added rotation component with speed {config.RotationSpeed}", "Testing");
            }
            catch (Exception ex)
            {
                LoggingSystem.Warning($"Failed to add rotation component: {ex.Message}", "Testing");
            }
        }

        /// <summary>
        /// Log detailed sphere information for debugging
        /// </summary>
        public static void LogSphereDetails(GameObject sphere)
        {
            LoggingSystem.Debug("=== GLOWING SPHERE DETAILS ===", "Testing");
            LoggingSystem.Debug($"Name: {sphere.name}", "Testing");
            LoggingSystem.Debug($"Position: {sphere.transform.position}", "Testing");
            LoggingSystem.Debug($"Rotation: {sphere.transform.rotation.eulerAngles}", "Testing");
            LoggingSystem.Debug($"Scale: {sphere.transform.localScale}", "Testing");
            LoggingSystem.Debug($"Layer: {sphere.layer} ({LayerMask.LayerToName(sphere.layer)})", "Testing");
            LoggingSystem.Debug($"Active: {sphere.activeInHierarchy}", "Testing");

            var renderer = sphere.GetComponent<Renderer>();
            if (renderer != null)
            {
                LoggingSystem.Debug($"Renderer enabled: {renderer.enabled}", "Testing");
                LoggingSystem.Debug($"Renderer visible: {renderer.isVisible}", "Testing");
                LoggingSystem.Debug($"Material: {renderer.material?.name ?? "null"}", "Testing");
            }
            LoggingSystem.Debug("=== END SPHERE DETAILS ===", "Testing");
        }
    }
} 