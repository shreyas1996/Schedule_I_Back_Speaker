using System;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;
using UnityEngine;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.Utils
{
    /// <summary>
    /// Helper class for loading audio files using external AudioImportLib
    /// Provides reliable audio loading that works in MelonLoader environment with real audio decoding
    /// </summary>
    public static class AudioHelper
    {
        /// <summary>
        /// Load an audio file and convert it to Unity AudioClip using external AudioImportLib
        /// This actually loads and decodes the audio content for real playback
        /// </summary>
        public static async Task<AudioClip?> LoadAudioFileAsync(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    LoggingSystem.Warning($"Audio file not found: {filePath}", "AudioHelper");
                    return null;
                }

                string fileName = Path.GetFileNameWithoutExtension(filePath);
                string extension = Path.GetExtension(filePath).ToLower();

                LoggingSystem.Debug($"Loading audio file with external AudioImportLib: {fileName} ({extension})", "AudioHelper");

                // Use external AudioImportLib for real audio loading
                return await Task.Run(() => LoadAudioWithExternalAudioImportLib(filePath, fileName));
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to load audio file {filePath}: {ex.Message}", "AudioHelper");
                return null;
            }
        }

        private static AudioClip? LoadAudioWithExternalAudioImportLib(string filePath, string fileName)
        {
            try
            {
                // Check if AudioImportLib is available
                if (!EmbeddedAssemblyLoader.IsAudioImportLibAvailable())
                {
                    LoggingSystem.Warning("AudioImportLib not available, creating placeholder clip", "AudioHelper");
                    return CreatePlaceholderClip(fileName + " (AudioImportLib unavailable)");
                }

                // Get the LoadAudioClip method via reflection
                var apiType = EmbeddedAssemblyLoader.GetAudioImportLibApiType();
                var loadMethod = apiType?.GetMethod("LoadAudioClip", BindingFlags.Public | BindingFlags.Static);
                if (loadMethod == null)
                {
                    LoggingSystem.Warning("Could not get LoadAudioClip method from AudioImportLib", "AudioHelper");
                    return CreatePlaceholderClip(fileName + " (Method not found)");
                }

                LoggingSystem.Debug($"Calling AudioImportLib.API.LoadAudioClip via reflection for: {fileName}", "AudioHelper");
                
                // Call AudioImportLib.API.LoadAudioClip(filePath, true) via reflection
                object?[] parameters = { filePath, true };
                var result = loadMethod.Invoke(null, parameters);
                
                var clip = result as AudioClip;
                
                if (clip != null)
                {
                    // Set a proper name for the clip
                    clip.name = fileName;
                    
                    LoggingSystem.Info($"Successfully loaded real audio via external AudioImportLib: {fileName} ({clip.length:F1}s, {clip.frequency}Hz, {clip.channels}ch)", "AudioHelper");
                    return clip;
                }
                else
                {
                    LoggingSystem.Warning($"External AudioImportLib returned null for: {fileName}", "AudioHelper");
                    return CreatePlaceholderClip(fileName + " (LoadFailed)");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"External AudioImportLib failed to load {fileName}: {ex.Message}", "AudioHelper");
                return CreatePlaceholderClip(fileName + " (Error)");
            }
        }

        private static AudioClip CreatePlaceholderClip(string fileName)
        {
            try
            {
                // Create a 1-second placeholder clip as fallback
                var clip = AudioClip.Create(fileName, 44100, 2, 44100, false);
                LoggingSystem.Debug($"Created placeholder clip: {fileName}", "AudioHelper");
                return clip;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Could not create placeholder clip for {fileName}: {ex.Message}", "AudioHelper");
                throw;
            }
        }

        /// <summary>
        /// Get supported audio file extensions (based on AudioImportLib capabilities)
        /// </summary>
        public static string[] GetSupportedExtensions()
        {
            // AudioImportLib supports these formats via BASS library
            return new string[] { ".mp3", ".wav", ".ogg", ".flac", ".aiff", ".aif", ".wma", ".m4a" };
        }

        /// <summary>
        /// Check if a file extension is supported by AudioImportLib
        /// </summary>
        public static bool IsExtensionSupported(string extension)
        {
            string ext = extension.ToLower();
            foreach (string supported in GetSupportedExtensions())
            {
                if (ext == supported)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Get basic audio file info from file system
        /// </summary>
        public static (long fileSize, string extension)? GetAudioInfo(string filePath)
        {
            try
            {
                var fileInfo = new FileInfo(filePath);
                return (fileInfo.Length, fileInfo.Extension.ToLower());
            }
            catch (Exception ex)
            {
                LoggingSystem.Warning($"Could not get file info for {filePath}: {ex.Message}", "AudioHelper");
                return null;
            }
        }

        /// <summary>
        /// Check if AudioImportLib is available for audio loading
        /// </summary>
        public static bool IsAudioImportLibAvailable()
        {
            return EmbeddedAssemblyLoader.IsAudioImportLibAvailable();
        }

        /// <summary>
        /// Load an audio clip directly from a streaming URL using AudioImportLib
        /// This enables real-time streaming without downloading files first
        /// </summary>
        public static async Task<AudioClip?> LoadAudioFromUrlAsync(string url, string clipName = "StreamedAudio")
        {
            try
            {
                LoggingSystem.Info($"Loading audio from URL with AudioImportLib: {clipName}", "AudioHelper");
                LoggingSystem.Debug($"Streaming URL: {url}", "AudioHelper");

                // Use external AudioImportLib for URL streaming
                return await Task.Run(() => LoadAudioFromUrlWithExternalAudioImportLib(url, clipName));
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to load audio from URL {url}: {ex.Message}", "AudioHelper");
                return null;
            }
        }

        private static AudioClip? LoadAudioFromUrlWithExternalAudioImportLib(string url, string clipName)
        {
            try
            {
                // Check if AudioImportLib is available
                if (!EmbeddedAssemblyLoader.IsAudioImportLibAvailable())
                {
                    LoggingSystem.Warning("AudioImportLib not available for URL streaming, creating placeholder clip", "AudioHelper");
                    return CreatePlaceholderClip(clipName + " (AudioImportLib unavailable)");
                }

                // Get the LoadAudioClip method via reflection
                var apiType = EmbeddedAssemblyLoader.GetAudioImportLibApiType();
                var loadMethod = apiType?.GetMethod("LoadAudioClip", BindingFlags.Public | BindingFlags.Static);
                if (loadMethod == null)
                {
                    LoggingSystem.Warning("Could not get LoadAudioClip method from AudioImportLib for URL streaming", "AudioHelper");
                    return CreatePlaceholderClip(clipName + " (Method not found)");
                }

                LoggingSystem.Debug($"Calling AudioImportLib.API.LoadAudioClip via reflection for URL: {clipName}", "AudioHelper");
                
                // Call AudioImportLib.API.LoadAudioClip(url, true) via reflection
                // AudioImportLib should support HTTP URLs directly via BASS library
                object?[] parameters = { url, true };
                var result = loadMethod.Invoke(null, parameters);
                
                var clip = result as AudioClip;
                
                if (clip != null)
                {
                    // Set a proper name for the clip
                    clip.name = clipName;
                    
                    LoggingSystem.Info($"Successfully loaded streaming audio via AudioImportLib: {clipName} ({clip.length:F1}s, {clip.frequency}Hz, {clip.channels}ch)", "AudioHelper");
                    return clip;
                }
                else
                {
                    LoggingSystem.Warning($"AudioImportLib returned null for URL stream: {clipName}", "AudioHelper");
                    return CreatePlaceholderClip(clipName + " (LoadFailed)");
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"AudioImportLib failed to load URL stream {clipName}: {ex.Message}", "AudioHelper");
                LoggingSystem.Debug($"URL that failed: {url}", "AudioHelper");
                return CreatePlaceholderClip(clipName + " (Error)");
            }
        }
    }
} 