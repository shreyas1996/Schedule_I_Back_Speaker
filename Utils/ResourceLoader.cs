using System.Reflection;
using System.IO;
using UnityEngine;
using MelonLoader;
using System;
// using UnityEngine.ImageConversion;

namespace BackSpeakerMod.Utils
{
    public static class ResourceLoader
    {
        public static Sprite LoadEmbeddedSprite(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            if (assembly == null)
            {
                throw new Exception("Failed to get executing assembly");
            }
            using (var stream = assembly.GetManifestResourceStream(resourceName)!)
            {
                if (stream == null)
                {
                    // LoggerUtil.Error($"Resource {resourceName} not found!");
                    throw new Exception($"Resource {resourceName} not found!");
                }
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(buffer);
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
        }
    }
} 