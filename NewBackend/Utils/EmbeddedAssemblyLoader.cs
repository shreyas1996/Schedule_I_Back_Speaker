using System;
using System.IO;
using System.Reflection;

namespace BackSpeakerMod.NewBackend.Utils
{
    // We load audioimprot lib on runtime from UserLibs\AudioImportLib.dll
    public static class AudioImportLibAssemblyLoader
    {
        private static Assembly? _audioImportLibAssembly;

        private static string _audioImportLibPath = Path.Combine("UserLibs", "AudioImportLib.dll");
        private static bool _loadAttempted = false;

        public static bool LoadAudioImportLib()
        {
            if (_loadAttempted)
                return _audioImportLibAssembly != null;

            _loadAttempted = true;

            try
            {
                NewLoggingSystem.Debug("Loading  AudioImportLib.dll...", "AudioImportLibAssemblyLoader");

                // Get the current assembly (our mod)
                var currentAssembly = Assembly.GetExecutingAssembly();

                if (!File.Exists(_audioImportLibPath))
                {
                    NewLoggingSystem.Error($"AudioImportLib.dll not found at {_audioImportLibPath}", "AudioImportLibAssemblyLoader");
                    return false;
                }

                _audioImportLibAssembly = Assembly.LoadFrom(_audioImportLibPath);
                NewLoggingSystem.Info($"Successfully loaded AudioImportLib: {_audioImportLibAssembly.FullName}", "AudioImportLibAssemblyLoader");
                return true;

            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to load AudioImportLib: {ex.Message}", "AudioImportLibAssemblyLoader");
                return false;
            }
        }

        /// <summary>
        /// Get the AudioImportLib API type for calling LoadAudioClip
        /// </summary>
        public static Type? GetAudioImportLibApiType()
        {
            if (_audioImportLibAssembly == null)
            {
                if (!LoadAudioImportLib())
                    return null;
            }

            try
            {
                return _audioImportLibAssembly?.GetType("AudioImportLib.API");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Could not get AudioImportLib.API type: {ex.Message}", "AudioImportLibAssemblyLoader");
                return null;
            }
        }

        /// <summary>
        /// Get the LoadAudioClip method from the  AudioImportLib
        /// </summary>
        public static MethodInfo? GetLoadAudioClipMethod()
        {
            var apiType = GetAudioImportLibApiType();
            if (apiType == null)
                return null;

            try
            {
                return apiType.GetMethod("LoadAudioClip", BindingFlags.Public | BindingFlags.Static);
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Could not get LoadAudioClip method: {ex.Message}", "AudioImportLibAssemblyLoader");
                return null;
            }
        }

        /// <summary>
        /// Check if AudioImportLib is available (either  or already loaded)
        /// </summary>
        public static bool IsAudioImportLibAvailable()
        {
            // First try to load our  version
            if (LoadAudioImportLib())
                return true;

            // Fallback: check if AudioImportLib is already loaded in the app domain
            try
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name == "AudioImportLib")
                    {
                        NewLoggingSystem.Info("Found existing AudioImportLib assembly in app domain", "AudioImportLibAssemblyLoader");
                        _audioImportLibAssembly = assembly;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Warning($"Error checking for existing AudioImportLib: {ex.Message}", "AudioImportLibAssemblyLoader");
            }

            return false;
        }
    }
} 