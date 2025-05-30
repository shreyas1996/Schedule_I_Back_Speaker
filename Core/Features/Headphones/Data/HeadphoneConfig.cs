using UnityEngine;
using System.Collections.Generic;

namespace BackSpeakerMod.Core.Features.Headphones.Data
{
    /// <summary>
    /// URP/Lit material configuration
    /// </summary>
    [global::System.Serializable]
    public class URPMaterialConfig
    {
        /// <summary>
        /// Material name identifier
        /// </summary>
        public string Name { get; set; } = "Default";

        /// <summary>
        /// Base map color (Main Color)
        /// </summary>
        public Color BaseColor { get; set; } = Color.white;

        /// <summary>
        /// Metallic value (0.0 to 1.0)
        /// </summary>
        public float Metallic { get; set; } = 0.0f;

        /// <summary>
        /// Smoothness value (0.0 to 1.0)
        /// </summary>
        public float Smoothness { get; set; } = 0.5f;

        /// <summary>
        /// Whether this material is opaque or transparent
        /// </summary>
        public bool IsOpaque { get; set; } = true;

        /// <summary>
        /// Render face setting (Both, Front, Back)
        /// </summary>
        public UnityEngine.Rendering.CullMode CullMode { get; set; } = UnityEngine.Rendering.CullMode.Back;

        /// <summary>
        /// Alpha value for transparency (only used if IsOpaque = false)
        /// </summary>
        public float Alpha { get; set; } = 1.0f;

        /// <summary>
        /// Optional normal map texture name (will be loaded from bundle if provided)
        /// </summary>
        public string NormalMapName { get; set; } = "";

        /// <summary>
        /// Optional emission color for glowing effects
        /// </summary>
        public Color EmissionColor { get; set; } = Color.black;

        /// <summary>
        /// Emission intensity multiplier
        /// </summary>
        public float EmissionIntensity { get; set; } = 1.0f;

        /// <summary>
        /// Create a default black material config
        /// </summary>
        public static URPMaterialConfig CreateBlack()
        {
            return new URPMaterialConfig
            {
                Name = "Color - Black 1",
                BaseColor = new Color(0.094f, 0.09f, 0.09f, 1.0f), // #181717
                Metallic = 1.0f,
                Smoothness = 0.5f,
                IsOpaque = true,
                CullMode = UnityEngine.Rendering.CullMode.Off // Both faces
            };
        }

        public static URPMaterialConfig CreateSilver()
        {
            return new URPMaterialConfig
            {
                Name = "Color - Silver 1",
                BaseColor = new Color(0.498f, 0.498f, 0.498f, 1.0f), // #7F7F7F
                Metallic = 1.0f,
                Smoothness = 0.5f,
                IsOpaque = true,
                CullMode = UnityEngine.Rendering.CullMode.Back // Front faced
            };  
        }

        public static URPMaterialConfig CreateInnerBlack()
        {
            return new URPMaterialConfig
            {
                Name = "Color - Inner Black 1",
                BaseColor = new Color(0.220f, 0.220f, 0.220f, 1.0f), // #383838
                Metallic = 1.0f,
                Smoothness = 0.5f,
                IsOpaque = true,
                CullMode = UnityEngine.Rendering.CullMode.Back // Front faced
            };
        }

        public static URPMaterialConfig CreateRed()
        {
            return new URPMaterialConfig
            {
                Name = "Color - Red 1",
                BaseColor = new Color(0.576f, 0.051f, 0.098f, 1.0f), // #930D19
                Metallic = 0.6f,
                Smoothness = 0.5f,
                IsOpaque = true,
                CullMode = UnityEngine.Rendering.CullMode.Back // Front faced
            };
        }

        /// <summary>
        /// Create template configs for the other 3 materials
        /// </summary>
        public static URPMaterialConfig CreateTemplate(string name, Color baseColor)
        {
            return new URPMaterialConfig
            {
                Name = name,
                BaseColor = baseColor,
                Metallic = 1.0f,
                Smoothness = 0.5f,
                IsOpaque = true,
                CullMode = UnityEngine.Rendering.CullMode.Off
            };
        }
    }

    /// <summary>
    /// Configuration settings for headphone functionality
    /// </summary>
    public class HeadphoneConfig
    {
        /// <summary>
        /// Asset bundle name containing headphone assets
        /// </summary>
        public string AssetBundleName { get; set; } = "scheduleoneheadphones";

        /// <summary>
        /// Embedded resource name for headphone assets
        /// </summary>
        public string EmbeddedResourceName { get; set; } = "scheduleoneheadphones";

        /// <summary>
        /// Asset name within the bundle
        /// </summary>
        public string AssetName { get; set; } = "ScheduleOneHeadphones";

        /// <summary>
        /// Position offset from player head
        /// </summary>
        public Vector3 PositionOffset { get; set; } = Vector3.zero;

        /// <summary>
        /// Rotation offset from player head
        /// </summary>
        public Vector3 RotationOffset { get; set; } = new Vector3(-90f, 0f, 0f); // Rotate -90 degrees on X axis

        /// <summary>
        /// Scale multiplier for headphone model
        /// </summary>
        public Vector3 ScaleMultiplier { get; set; } = Vector3.one * 0.2f; // Scale down to 0.2

        /// <summary>
        /// Whether to use local position relative to parent
        /// </summary>
        public bool UseLocalPosition { get; set; } = true;

        /// <summary>
        /// Auto-attach headphones when player spawns
        /// </summary>
        public bool AutoAttachOnSpawn { get; set; } = true;

        /// <summary>
        /// Enable debug visualization
        /// </summary>
        public bool ShowDebugInfo { get; set; } = false;

        /// <summary>
        /// Whether to apply custom materials at runtime
        /// </summary>
        public bool ApplyCustomMaterials { get; set; } = true;

        /// <summary>
        /// List of material configurations to apply to different parts of the headphones
        /// </summary>
        public List<URPMaterialConfig> MaterialConfigs { get; set; } = new List<URPMaterialConfig>();

        /// <summary>
        /// Default material configuration (used as fallback)
        /// </summary>
        public URPMaterialConfig DefaultMaterial { get; set; } = URPMaterialConfig.CreateBlack();

        /// <summary>
        /// Initialize with default material configurations
        /// </summary>
        public HeadphoneConfig()
        {
            SetupDefaultMaterials();
        }

        /// <summary>
        /// Setup default material configurations
        /// </summary>
        private void SetupDefaultMaterials()
        {
            MaterialConfigs.Clear();
            
            // Add the default black material you specified
            MaterialConfigs.Add(URPMaterialConfig.CreateBlack());
            MaterialConfigs.Add(URPMaterialConfig.CreateSilver());
            MaterialConfigs.Add(URPMaterialConfig.CreateInnerBlack());
            MaterialConfigs.Add(URPMaterialConfig.CreateRed());
        }

        /// <summary>
        /// Get material config by name
        /// </summary>
        public URPMaterialConfig GetMaterialConfig(string name)
        {
            foreach (var config in MaterialConfigs)
            {
                if (config.Name.Equals(name, global::System.StringComparison.OrdinalIgnoreCase))
                {
                    return config;
                }
            }
            return DefaultMaterial;
        }

        /// <summary>
        /// Add or update a material configuration
        /// </summary>
        public void SetMaterialConfig(URPMaterialConfig materialConfig)
        {
            // Remove any existing config with the same name
            MaterialConfigs.RemoveAll(m => m.Name.Equals(materialConfig.Name, global::System.StringComparison.OrdinalIgnoreCase));
            
            // Add the new config
            MaterialConfigs.Add(materialConfig);
        }
    }
} 