using System.Collections.Generic;
using UnityEngine;

namespace BackSpeakerMod.NewBackend.Configs
{
    [global::System.Serializable]
    public class URPMaterialConfig
    {
        public Shader Shader { get; set; } = Shader.Find("Universal Render Pipeline/Lit");
        public Color BaseColor { get; set; } = Color.white;
        public float Metallic { get; set; } = 0.0f;
        public float Smoothness { get; set; } = 0.5f;
        public bool IsOpaque { get; set; } = true;
        public UnityEngine.Rendering.CullMode CullMode { get; set; } = UnityEngine.Rendering.CullMode.Back;
    }
    public static class HeadphoneConfig
    {
        public static string Name { get; set; } = "ScheduleOneHeadphones";
        public static Dictionary<string, URPMaterialConfig> Materials { get; set; } = 
            new Dictionary<string, URPMaterialConfig>() {
                {
                    "Silver Metal 1",
                    new URPMaterialConfig
                    {
                        BaseColor = new Color(0.498f, 0.498f, 0.498f, 1.0f), // #7F7F7F
                        Metallic = 1.0f,
                        Smoothness = 0.5f,
                        IsOpaque = true,
                        CullMode = UnityEngine.Rendering.CullMode.Back
                    }
                },
                {
                    "Color - Red 1",
                    new URPMaterialConfig
                    {
                        BaseColor = new Color(0.576f, 0.051f, 0.098f, 1.0f), // #930D19
                        Metallic = 0.6f,
                        Smoothness = 0.5f,
                        IsOpaque = true,
                        CullMode = UnityEngine.Rendering.CullMode.Back
                    }
                },
                {
                    "Color - Black 1",
                    new URPMaterialConfig
                    {
                        BaseColor = new Color(0.094f, 0.09f, 0.09f, 1.0f), // #181717
                        Metallic = 1.0f,    
                        Smoothness = 0.5f,
                        IsOpaque = true,
                        CullMode = UnityEngine.Rendering.CullMode.Off
                    }
                },
                {   
                    "Color - Inner Black 1",
                    new URPMaterialConfig
                    {
                        BaseColor = new Color(0.220f, 0.220f, 0.220f, 1.0f), // #383838
                        Metallic = 1.0f,
                        Smoothness = 0.5f,  
                        IsOpaque = true,
                        CullMode = UnityEngine.Rendering.CullMode.Off
                    }
                }
            };
    }
}