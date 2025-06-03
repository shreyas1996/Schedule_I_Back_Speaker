using UnityEngine;
using UnityEngine.Rendering;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Features.Headphones.Data;
using BackSpeakerMod.Configuration;
using Il2CppInterop.Runtime.InteropTypes;

namespace BackSpeakerMod.Core.Common.Helpers
{
    /// <summary>
    /// Helper for creating and configuring URP/Lit materials at runtime
    /// </summary>
    public static class URPMaterialHelper
    {
        /// <summary>
        /// Shader names to try for URP/Lit
        /// </summary>
        private static readonly string[] URPLitShaderNames = {
            "Universal Render Pipeline/Lit",
            "URP/Lit", 
            "Lit",
            "Standard" // Fallback
        };

        /// <summary>
        /// Create a URP/Lit material from configuration
        /// </summary>
        public static Material? CreateURPMaterial(URPMaterialConfig config)
        {
            if (config == null)
            {
                LoggingSystem.Error("Cannot create URP material - config is null", "MaterialHelper");
                return null;
            }

            try
            {
                // Find the best available shader
                Shader? urpShader = FindBestURPShader();
                if (urpShader == null)
                {
                    LoggingSystem.Error("No suitable URP shader found", "MaterialHelper");
                    return null;
                }

                if (FeatureFlags.Headphones.EnableMaterialDebugging)
                {
                    LoggingSystem.Debug($"Creating URP material '{config.Name}' with shader: {urpShader.name}", "MaterialHelper");
                }

                // Create the material
                var material = new Material(urpShader);
                material.name = config.Name;

                // Apply configuration
                ApplyURPMaterialConfig(material, config);

                // Validate if enabled
                if (FeatureFlags.Headphones.ValidateMaterialShaders)
                {
                    ValidateURPMaterial(material);
                }

                LoggingSystem.Debug($"✓ Created URP material: {config.Name}", "MaterialHelper");
                return material;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to create URP material '{config.Name}': {ex.Message}", "MaterialHelper");
                return null;
            }
        }

        /// <summary>
        /// Apply URP material configuration to an existing material
        /// </summary>
        public static void ApplyURPMaterialConfig(Material? material, URPMaterialConfig config)
        {
            if (material == null || config == null)
            {
                LoggingSystem.Warning("Cannot apply URP config - material or config is null", "MaterialHelper");
                return;
            }

            try
            {
                if (FeatureFlags.Headphones.EnableMaterialDebugging)
                {
                    LoggingSystem.Debug($"Applying URP config to material: {material.name}", "MaterialHelper");
                }

                // Base Color (Main texture color)
                var baseColor = config.BaseColor;
                if (!config.IsOpaque)
                {
                    baseColor.a = config.Alpha;
                }

                if (material?.HasProperty("_BaseColor") ?? false)
                {
                    material.SetColor("_BaseColor", baseColor);
                }
                else if (material?.HasProperty("_Color") ?? false)
                {
                    material.SetColor("_Color", baseColor);
                }

                // Metallic properties
                if (material?.HasProperty("_Metallic") ?? false)
                {
                    material.SetFloat("_Metallic", config.Metallic);
                }

                // Smoothness 
                if (material?.HasProperty("_Smoothness") ?? false)
                {
                    material.SetFloat("_Smoothness", config.Smoothness);
                }
                else if (material?.HasProperty("_Glossiness") ?? false)
                {
                    material.SetFloat("_Glossiness", config.Smoothness);
                }

                // Surface Type (Opaque/Transparent)
                ConfigureSurfaceType(material, config);

                // Render Face (Cull Mode)
                if (material?.HasProperty("_Cull") ?? false)
                {
                    material.SetFloat("_Cull", (float)config.CullMode);
                }

                // Emission
                ConfigureEmission(material, config);

                if (FeatureFlags.Headphones.EnableMaterialDebugging)
                {
                    LoggingSystem.Debug($"✓ Applied URP config: Color={config.BaseColor}, Metallic={config.Metallic}, Smoothness={config.Smoothness}", "MaterialHelper");
                }
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to apply URP config to material: {ex.Message}", "MaterialHelper");
            }
        }

        /// <summary>
        /// Configure surface type (opaque vs transparent)
        /// </summary>
        private static void ConfigureSurfaceType(Material? material, URPMaterialConfig config)
        {
            try
            {
                if (config.IsOpaque)
                {
                    // Opaque surface
                    if (material?.HasProperty("_Surface") ?? false)
                    {
                        material.SetFloat("_Surface", 0); // 0 = Opaque
                    }
                    
                    if (material?.HasProperty("_Mode") ?? false)
                    {
                        material.SetFloat("_Mode", 0); // Standard opaque
                    }

                    // Set render queue
                    if (material != null)
                    {
                        material.renderQueue = (int)RenderQueue.Geometry;
                        // Disable transparency keywords
                        material.DisableKeyword("_ALPHATEST_ON");
                        material.DisableKeyword("_ALPHABLEND_ON");
                        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    }

                }
                else
                {
                    // Transparent surface
                    if (material?.HasProperty("_Surface") ?? false)
                    {
                        material.SetFloat("_Surface", 1); // 1 = Transparent
                    }
                    
                    if (material?.HasProperty("_Mode") ?? false)
                    {
                        material.SetFloat("_Mode", 2); // Transparent
                    }

                    // Configure blending for transparency
                    if (material?.HasProperty("_SrcBlend") ?? false)
                    {
                        material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
                    }
                    if (material?.HasProperty("_DstBlend") ?? false)
                    {
                        material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
                    }
                    if (material?.HasProperty("_ZWrite") ?? false)
                    {
                        material.SetFloat("_ZWrite", 0); // Disable Z-write for transparency
                    }

                    // Set render queue
                    if (material != null)
                    {
                        material.renderQueue = (int)RenderQueue.Transparent;
                        // Enable transparency keyword
                        material.EnableKeyword("_ALPHABLEND_ON");
                    }

                }

                if (FeatureFlags.Headphones.EnableMaterialDebugging)
                {
                    LoggingSystem.Debug($"Configured surface type: {(config.IsOpaque ? "Opaque" : "Transparent")}", "MaterialHelper");
                }
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to configure surface type: {ex.Message}", "MaterialHelper");
            }
        }

        /// <summary>
        /// Configure emission properties
        /// </summary>
        private static void ConfigureEmission(Material? material, URPMaterialConfig config)
        {
            try
            {
                bool hasEmission = config.EmissionColor != Color.black && config.EmissionIntensity > 0;

                if (hasEmission)
                {
                    // Enable emission
                    if (material?.HasProperty("_EmissionColor") ?? false)
                    {
                        var emissionColor = config.EmissionColor * config.EmissionIntensity;
                        material.SetColor("_EmissionColor", emissionColor);
                        material.EnableKeyword("_EMISSION");
                        
                        // For URP, also set the global illumination flag
                        material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                        
                        if (FeatureFlags.Headphones.EnableMaterialDebugging)
                        {
                            LoggingSystem.Debug($"Enabled emission: {emissionColor} (intensity: {config.EmissionIntensity})", "MaterialHelper");
                        }
                    }
                }
                else
                {
                    // Disable emission
                    if (material?.HasProperty("_EmissionColor") ?? false)
                    {
                        material.SetColor("_EmissionColor", Color.black);
                        material.DisableKeyword("_EMISSION");
                        material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
                        
                        if (FeatureFlags.Headphones.EnableMaterialDebugging)
                        {
                            LoggingSystem.Debug("Disabled emission", "MaterialHelper");
                        }
                    }
                }
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to configure emission: {ex.Message}", "MaterialHelper");
            }
        }

        /// <summary>
        /// Find the best available URP shader
        /// </summary>
        private static Shader? FindBestURPShader()
        {
            foreach (var shaderName in URPLitShaderNames)
            {
                var shader = Shader.Find(shaderName);
                if (shader != null)
                {
                    if (FeatureFlags.Headphones.EnableMaterialDebugging)
                    {
                        LoggingSystem.Debug($"Found URP shader: {shaderName}", "MaterialHelper");
                    }
                    return shader;
                }
            }

            LoggingSystem.Warning("No URP shaders found, will return null", "MaterialHelper");
            return null;
        }

        /// <summary>
        /// Apply URP materials to all renderers in a GameObject
        /// </summary>
        public static void ApplyMaterialsToGameObject(GameObject gameObject, HeadphoneConfig config)
        {
            if (gameObject == null || config == null || !config.ApplyCustomMaterials)
            {
                if (FeatureFlags.Headphones.EnableMaterialDebugging)
                {
                    LoggingSystem.Debug("Skipping material application - disabled or null parameters", "MaterialHelper");
                }
                return;
            }

            if (!FeatureFlags.Headphones.ApplyCustomMaterials)
            {
                LoggingSystem.Debug("Custom material application disabled by feature flag", "MaterialHelper");
                return;
            }

            try
            {
                LoggingSystem.Debug($"Applying URP materials to {gameObject.name}", "MaterialHelper");

                var renderers = gameObject.GetComponentsInChildren<Renderer>();
                if (FeatureFlags.Headphones.EnableMaterialDebugging)
                {
                    LoggingSystem.Debug($"Found {renderers.Length} renderers to process", "MaterialHelper");
                }

                foreach (var renderer in renderers)
                {
                    // Check if renderer has multiple materials
                    var materials = renderer.materials;
                    int materialCount = materials.Length;
                    
                    if (FeatureFlags.Headphones.EnableMaterialDebugging)
                    {
                        LoggingSystem.Debug($"Renderer '{renderer.name}' has {materialCount} material slots", "MaterialHelper");
                    }

                    if (materialCount == 1)
                    {
                        // Single material - use first config or default
                        var materialConfig = config.MaterialConfigs.Count > 0 ? config.MaterialConfigs[0] : config.DefaultMaterial;
                        var urpMaterial = CreateURPMaterial(materialConfig);
                        
                        if (urpMaterial != null)
                        {
                            renderer.material = urpMaterial;
                            if (FeatureFlags.Headphones.EnableMaterialDebugging)
                            {
                                LoggingSystem.Debug($"✓ Applied single material '{materialConfig.Name}' to renderer: {renderer.name}", "MaterialHelper");
                            }
                        }
                    }
                    else
                    {
                        // Multiple materials - apply each config to corresponding slot
                        var newMaterials = new Material[materialCount];
                        
                        for (int i = 0; i < materialCount; i++)
                        {
                            URPMaterialConfig materialConfig;
                            if (i < config.MaterialConfigs.Count)
                            {
                                materialConfig = config.MaterialConfigs[i];
                            }
                            else
                            {
                                materialConfig = config.DefaultMaterial;
                                if (FeatureFlags.Headphones.EnableMaterialDebugging)
                                {
                                    LoggingSystem.Debug($"Using default material for slot {i} on renderer '{renderer.name}'", "MaterialHelper");
                                }
                            }

                            var urpMaterial = CreateURPMaterial(materialConfig);
                            if (urpMaterial != null)
                            {
                                newMaterials[i] = urpMaterial;
                                if (FeatureFlags.Headphones.EnableMaterialDebugging)
                                {
                                    LoggingSystem.Debug($"✓ Created material '{materialConfig.Name}' for slot {i} on renderer: {renderer.name}", "MaterialHelper");
                                }
                            }
                            else
                            {
                                // Keep original material if creation failed
                                newMaterials[i] = materials[i];
                                LoggingSystem.Warning($"Failed to create material for slot {i}, keeping original", "MaterialHelper");
                            }
                        }
                        
                        // Apply all materials at once
                        renderer.materials = newMaterials;
                        
                        if (FeatureFlags.Headphones.EnableMaterialDebugging)
                        {
                            LoggingSystem.Debug($"✓ Applied {materialCount} materials to renderer: {renderer.name}", "MaterialHelper");
                        }
                    }
                }

                LoggingSystem.Debug($"✓ Applied URP materials to {renderers.Length} renderers", "MaterialHelper");
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to apply materials to GameObject: {ex.Message}", "MaterialHelper");
            }
        }

        /// <summary>
        /// Validate that a material is properly configured for URP
        /// </summary>
        public static bool ValidateURPMaterial(Material? material)
        {
            if (material == null) return false;

            if (!FeatureFlags.Headphones.ValidateMaterialShaders)
            {
                return true; // Skip validation if disabled
            }

            try
            {
                // Check if it's using a URP-compatible shader
                var shaderName = material.shader.name;
                bool isURPShader = false;
                
                foreach (var validName in URPLitShaderNames)
                {
                    if (shaderName.Contains(validName))
                    {
                        isURPShader = true;
                        break;
                    }
                }

                if (!isURPShader)
                {
                    LoggingSystem.Warning($"Material '{material.name}' may not be URP-compatible (shader: {shaderName})", "MaterialHelper");
                }

                if (FeatureFlags.Headphones.EnableMaterialDebugging)
                {
                    LoggingSystem.Debug($"Material validation - '{material.name}': URP-compatible={isURPShader}", "MaterialHelper");
                }
                return isURPShader;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to validate material: {ex.Message}", "MaterialHelper");
                return false;
            }
        }

        /// <summary>
        /// Log material properties for debugging
        /// </summary>
        public static void LogMaterialProperties(Material? material)
        {
            if (material == null || !FeatureFlags.Headphones.EnableMaterialDebugging) return;

            try
            {
                LoggingSystem.Debug($"=== MATERIAL PROPERTIES: {material.name} ===", "MaterialHelper");
                LoggingSystem.Debug($"Shader: {material.shader.name}", "MaterialHelper");
                LoggingSystem.Debug($"Render Queue: {material.renderQueue}", "MaterialHelper");
                
                // Log common URP properties
                var commonProperties = new[] { "_BaseColor", "_Color", "_BaseMap", "_MainTex", "_Metallic", "_Smoothness", "_Cull", "_EmissionColor" };
                foreach (var prop in commonProperties)
                {
                    if (material?.HasProperty(prop) ?? false)
                    {
                        if (prop.Contains("Color"))
                        {
                            LoggingSystem.Debug($"{prop}: {material.GetColor(prop)}", "MaterialHelper");
                        }
                        else if (prop.Contains("Map") || prop.Contains("Tex"))
                        {
                            var texture = material.GetTexture(prop);
                            LoggingSystem.Debug($"{prop}: {(texture != null ? texture.name : "null")}", "MaterialHelper");
                        }
                        else
                        {
                            LoggingSystem.Debug($"{prop}: {material.GetFloat(prop)}", "MaterialHelper");
                        }
                    }
                    else
                    {
                        LoggingSystem.Debug($"{prop}: NOT AVAILABLE", "MaterialHelper");
                    }
                }

                LoggingSystem.Debug("=====================================", "MaterialHelper");
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to log material properties: {ex.Message}", "MaterialHelper");
            }
        }
    }
} 