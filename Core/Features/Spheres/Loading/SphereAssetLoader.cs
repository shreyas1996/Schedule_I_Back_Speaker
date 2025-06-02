using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Features.Spheres.Data;
using BackSpeakerMod.Core.Features.Spheres.Components;
using System;

namespace BackSpeakerMod.Core.Features.Spheres.Loading
{
    /// <summary>
    /// Loads/creates sphere assets for attachment functionality
    /// </summary>
    public static class SphereAssetLoader
    {
        /// <summary>
        /// Create a glowing sphere GameObject based on configuration
        /// </summary>
        public static GameObject CreateSphere(SphereConfig config = null)
        {
            try
            {
                config ??= new SphereConfig();
                
                LoggingSystem.Info("Creating sphere for attachment", "SphereLoader");
                
                // Create primitive sphere
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = "AttachmentSphere";
                
                // Set scale based on radius
                var scale = Vector3.one * (config.Radius * 2f); // Radius to diameter
                sphere.transform.localScale = Vector3.Scale(scale, config.ScaleMultiplier);
                
                // Configure material
                ConfigureMaterial(sphere, config);
                
                // Add rotation component if enabled
                if (config.EnableRotation)
                {
                    ConfigureRotation(sphere, config);
                }
                
                LoggingSystem.Info($"Sphere created with radius {config.Radius}, color {config.Color}", "SphereLoader");
                return sphere;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to create sphere: {ex.Message}", "SphereLoader");
                return null;
            }
        }

        /// <summary>
        /// Configure sphere material with glow effect
        /// </summary>
        private static void ConfigureMaterial(GameObject sphere, SphereConfig config)
        {
            var renderer = sphere.GetComponent<Renderer>();
            if (renderer == null)
            {
                LoggingSystem.Warning("Sphere has no renderer component", "SphereLoader");
                return;
            }

            try
            {
                Material material;
                
                // Use custom material if provided
                if (config.CustomMaterial != null)
                {
                    material = UnityEngine.Object.Instantiate(config.CustomMaterial);
                    LoggingSystem.Debug("Using custom material", "SphereLoader");
                }
                else
                {
                    // Create standard glowing material
                    material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
                    
                    // Base color
                    // material.color = config.Color;
                    material.SetColor("_TintColor", config.Color);
                    
                    // Emission for glow effect
                    if (config.EnableGlow)
                    {
                        var light = sphere.AddComponent<Light>();
                        light.type = LightType.Point;
                        light.color = config.Color;
                        light.range = config.Radius * 8f;
                        light.intensity = config.GlowIntensity * 2f;
                        LoggingSystem.Debug($"Configured Light with intensity {config.GlowIntensity}", "SphereLoader");

                        // var emissionColor = config.Color * config.GlowIntensity;
                        // material.SetColor("_EmissionColor", emissionColor);
                        // material.EnableKeyword("_EMISSION");
                        // LoggingSystem.Debug($"Configured glow with intensity {config.GlowIntensity}", "SphereLoader");
                    }
                    
                    // Make it more reflective for better visibility
                    // material.SetFloat("_Metallic", 0.2f);
                    // material.SetFloat("_Glossiness", 0.8f);
                }
                
                renderer.material = material;
                LoggingSystem.Debug("Sphere material configured", "SphereLoader");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to configure sphere material: {ex.Message}", "SphereLoader");
            }
        }

        /// <summary>
        /// Configure sphere rotation animation
        /// </summary>
        private static void ConfigureRotation(GameObject sphere, SphereConfig config)
        {
            try
            {
                var rotator = sphere.AddComponent<SphereRotator>();
                rotator.RotationSpeed = config.RotationSpeed;
                LoggingSystem.Debug($"Added rotation component with speed {config.RotationSpeed}", "SphereLoader");
            }
            catch (Exception ex)
            {
                LoggingSystem.Warning($"Failed to add rotation component: {ex.Message}", "SphereLoader");
            }
        }

        /// <summary>
        /// Destroy sphere GameObject safely
        /// </summary>
        public static void DestroySphere(GameObject sphere)
        {
            if (sphere == null) return;
            
            try
            {
                LoggingSystem.Info($"Destroying sphere: {sphere.name}", "SphereLoader");
                UnityEngine.Object.Destroy(sphere);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to destroy sphere: {ex.Message}", "SphereLoader");
            }
        }

        /// <summary>
        /// Check if sphere is valid and not destroyed
        /// </summary>
        public static bool IsSphereValid(GameObject sphere)
        {
            return sphere != null && sphere;
        }

        /// <summary>
        /// Update sphere configuration at runtime
        /// </summary>
        public static void UpdateSphereConfig(GameObject sphere, SphereConfig newConfig)
        {
            if (!IsSphereValid(sphere) || newConfig == null) return;
            
            try
            {
                LoggingSystem.Debug("Updating sphere configuration", "SphereLoader");
                
                // Update scale
                var scale = Vector3.one * (newConfig.Radius * 2f);
                sphere.transform.localScale = Vector3.Scale(scale, newConfig.ScaleMultiplier);
                
                // Update material
                ConfigureMaterial(sphere, newConfig);
                
                // Update rotation
                var rotator = sphere.GetComponent<SphereRotator>();
                if (newConfig.EnableRotation)
                {
                    if (rotator == null)
                    {
                        ConfigureRotation(sphere, newConfig);
                    }
                    else
                    {
                        rotator.RotationSpeed = newConfig.RotationSpeed;
                    }
                }
                else if (rotator != null)
                {
                    UnityEngine.Object.Destroy(rotator);
                }
                
                LoggingSystem.Debug("Sphere configuration updated", "SphereLoader");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to update sphere configuration: {ex.Message}", "SphereLoader");
            }
        }
    }
} 