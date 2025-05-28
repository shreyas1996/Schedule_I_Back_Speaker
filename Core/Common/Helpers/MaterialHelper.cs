using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;

namespace BackSpeakerMod.Core.Common.Helpers
{
    /// <summary>
    /// Material and shader setup utilities
    /// </summary>
    public static class MaterialHelper
    {
        /// <summary>
        /// Create standard material with color
        /// </summary>
        public static Material CreateStandardMaterial(Color color)
        {
            var material = new Material(Shader.Find("Standard"));
            material.color = color;
            LoggingSystem.Debug($"Created standard material with color {color}", "Helper");
            return material;
        }

        /// <summary>
        /// Create transparent material
        /// </summary>
        public static Material CreateTransparentMaterial(Color color, float alpha = 0.5f)
        {
            var material = CreateStandardMaterial(color);
            SetMaterialTransparent(material);
            
            var finalColor = color;
            finalColor.a = alpha;
            material.color = finalColor;
            
            LoggingSystem.Debug($"Created transparent material with alpha {alpha}", "Helper");
            return material;
        }

        /// <summary>
        /// Set material to transparent mode
        /// </summary>
        public static void SetMaterialTransparent(Material material)
        {
            if (material == null) return;
            
            material.SetFloat("_Mode", 3); // Transparent mode
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            
            LoggingSystem.Debug("Set material to transparent mode", "Helper");
        }

        /// <summary>
        /// Create emissive material
        /// </summary>
        public static Material CreateEmissiveMaterial(Color emissionColor, float intensity = 1f)
        {
            var material = new Material(Shader.Find("Standard"));
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emissionColor * intensity);
            
            LoggingSystem.Debug($"Created emissive material with intensity {intensity}", "Helper");
            return material;
        }

        /// <summary>
        /// Apply material to renderer
        /// </summary>
        public static void ApplyMaterial(Renderer renderer, Material material)
        {
            if (renderer == null || material == null) return;
            
            renderer.material = material;
            LoggingSystem.Debug($"Applied material to {renderer.name}", "Helper");
        }
    }
} 