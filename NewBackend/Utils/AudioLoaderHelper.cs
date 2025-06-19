using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using MelonLoader;


namespace BackSpeakerMod.NewBackend.Utils
{
    public static class AudioLoaderHelper
    {
        public static IEnumerator LoadAudioClipFromFile(string filePath, System.Action<AudioClip> onComplete)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                NewLoggingSystem.Warning("Empty file path provided", "AudioLoaderHelper");
                onComplete?.Invoke(null);
                yield break;
            }

            NewLoggingSystem.Debug($"Loading audio clip from: {filePath}", "AudioLoaderHelper");

            var loadTask = AudioHelper.LoadAudioFileAsync(filePath);
            
            while (!loadTask.IsCompleted)
            {
                yield return new WaitForSeconds(0.1f);
            }

            AudioClip clip = null;
            try
            {
                clip = loadTask.Result;
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to load audio clip: {ex}", "AudioLoaderHelper");
            }

            if (clip != null)
            {
                NewLoggingSystem.Info($"✓ Audio clip loaded: {clip.name} ({clip.length:F1}s)", "AudioLoaderHelper");
            }
            else
            {
                NewLoggingSystem.Warning($"Failed to load audio clip from: {filePath}", "AudioLoaderHelper");
            }

            onComplete?.Invoke(clip);
        }

        public static bool IsExtensionSupported(string extension)
        {
            return AudioHelper.IsExtensionSupported(extension);
        }

        public static string[] GetSupportedExtensions()
        {
            return AudioHelper.GetSupportedExtensions();
        }

        public static bool IsAudioImportLibAvailable()
        {
            return AudioHelper.IsAudioImportLibAvailable();
        }
    }
} 