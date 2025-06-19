using UnityEngine;
using BackSpeakerMod.NewBackend.Configs;
using System.Linq;

namespace BackSpeakerMod.NewBackend.Utils
{
    public class MaterialConfig
    {
        public string Name { get; set; }
        public Material Material { get; set; }
    }

    public static class FixShaderAndMaterial
    {
        public static void ApplyShaderAndMaterials(GameObject? gameObject)
        {
            if (gameObject == null)
            {
                NewLoggingSystem.Error("Game object is null", "FixShaderAndMaterial");
                return;
            }
            NewLoggingSystem.Debug($"Applying shader and material to {gameObject.name}", "FixShaderAndMaterial");
            
            if(gameObject.name == HeadphoneConfig.Name)
            {
                var renderers = gameObject.GetComponentsInChildren<Renderer>();
                foreach (var renderer in renderers)
                {
                    NewLoggingSystem.Debug($"Applying shader and material to {renderer.name}", "FixShaderAndMaterial");
                    var materials = renderer.materials;
                    NewLoggingSystem.Debug($"Applying shader and material to {materials.Length} materials", "FixShaderAndMaterial");
                    foreach (var material in materials)
                    {
                        NewLoggingSystem.Debug($"Applying shader and material to {material.name}", "FixShaderAndMaterial");
                        URPMaterialConfig materialConfig;
                        try {
                            var strippedMaterialName = material.name.Replace(" (Instance)", "").Trim();
                            materialConfig = HeadphoneConfig.Materials[strippedMaterialName];
                        }
                        catch (KeyNotFoundException e)
                        {
                            NewLoggingSystem.Debug($"No material config found for {material.name}", "FixShaderAndMaterial");
                            NewLoggingSystem.Debug(e.Message, "FixShaderAndMaterial");
                            continue;
                        }

                        NewLoggingSystem.Debug($"Applying shader and material to {material.name}", "FixShaderAndMaterial");
                        if(materialConfig.Shader != null)
                        {
                            NewLoggingSystem.Debug($"Applying shader to {material.name}", "FixShaderAndMaterial");
                            material.shader = materialConfig.Shader;
                        }
                        if(materialConfig.BaseColor != null && material.HasProperty("_BaseColor"))
                        {
                            NewLoggingSystem.Debug($"Applying shader and material to {material.name}", "FixShaderAndMaterial");
                            material.SetColor("_BaseColor", materialConfig.BaseColor);
                        }
                        if(materialConfig.Metallic != null && material.HasProperty("_Metallic"))
                        {
                            NewLoggingSystem.Debug($"Applying shader and material to {material.name}", "FixShaderAndMaterial");
                            material.SetFloat("_Metallic", materialConfig.Metallic);
                        }
                        if(materialConfig.Smoothness != null && material.HasProperty("_Smoothness"))
                        {
                            NewLoggingSystem.Debug($"Applying shader and material to {material.name}", "FixShaderAndMaterial");
                            material.SetFloat("_Smoothness", materialConfig.Smoothness);
                        }
                        if(materialConfig.IsOpaque != null && material.HasProperty("_IsOpaque"))
                        {
                            NewLoggingSystem.Debug($"Applying shader and material to {material.name}", "FixShaderAndMaterial");
                            material.SetInt("_IsOpaque", materialConfig.IsOpaque ? 1 : 0);
                        }
                        if(materialConfig.CullMode != null && material.HasProperty("_CullMode"))
                        {
                            NewLoggingSystem.Debug($"Applying shader and material to {material.name}", "FixShaderAndMaterial");
                            material.SetInt("_CullMode", (int)materialConfig.CullMode);
                        }
                        
                    }
                }
            }
        }
    }
}