using System;
using System.IO;
using System.Reflection;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.Utils
{
    /// <summary>
    /// Helper class for loading embedded assemblies at runtime
    /// Allows embedding dependency DLLs as resources instead of requiring separate installation
    /// </summary>
    public static class EmbeddedAssemblyLoader
    {
        private static Assembly? _audioImportLibAssembly;
        private static bool _loadAttempted = false;

        /// <summary>
        /// Load the embedded AudioImportLib assembly
        /// </summary>
        public static bool LoadAudioImportLib()
        {
            if (_loadAttempted)
                return _audioImportLibAssembly != null;

            _loadAttempted = true;

            try
            {
                LoggingSystem.Debug("Loading embedded AudioImportLib.dll...", "EmbeddedAssemblyLoader");

                // Get the current assembly (our mod)
                var currentAssembly = Assembly.GetExecutingAssembly();
                
                // Resource name follows the pattern: Namespace.ResourcePath
                string resourceName = "BackSpeakerMod.EmbeddedResources.Libs.AudioImportLib.dll";

                // Check if resource exists
                var resourceNames = currentAssembly.GetManifestResourceNames();
                bool resourceExists = false;
                foreach (var name in resourceNames)
                {
                    if (name.EndsWith("AudioImportLib.dll"))
                    {
                        resourceName = name;
                        resourceExists = true;
                        break;
                    }
                }

                if (!resourceExists)
                {
                    LoggingSystem.Error("AudioImportLib.dll not found in embedded resources", "EmbeddedAssemblyLoader");
                    return false;
                }

                LoggingSystem.Debug($"Found embedded resource: {resourceName}", "EmbeddedAssemblyLoader");

                // Load the embedded DLL
                using (var stream = currentAssembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        LoggingSystem.Error("Could not open embedded AudioImportLib.dll stream", "EmbeddedAssemblyLoader");
                        return false;
                    }

                    // Read the DLL bytes
                    byte[] assemblyBytes = new byte[stream.Length];
                    stream.Read(assemblyBytes, 0, assemblyBytes.Length);

                    LoggingSystem.Debug($"Read {assemblyBytes.Length} bytes from embedded AudioImportLib.dll", "EmbeddedAssemblyLoader");

                    // Load the assembly from bytes
                    _audioImportLibAssembly = Assembly.Load(assemblyBytes);

                    LoggingSystem.Info($"Successfully loaded embedded AudioImportLib: {_audioImportLibAssembly.FullName}", "EmbeddedAssemblyLoader");
                    return true;
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to load embedded AudioImportLib: {ex.Message}", "EmbeddedAssemblyLoader");
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
                LoggingSystem.Error($"Could not get AudioImportLib.API type: {ex.Message}", "EmbeddedAssemblyLoader");
                return null;
            }
        }

        /// <summary>
        /// Get the LoadAudioClip method from the embedded AudioImportLib
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
                LoggingSystem.Error($"Could not get LoadAudioClip method: {ex.Message}", "EmbeddedAssemblyLoader");
                return null;
            }
        }

        /// <summary>
        /// Check if AudioImportLib is available (either embedded or already loaded)
        /// </summary>
        public static bool IsAudioImportLibAvailable()
        {
            // First try to load our embedded version
            if (LoadAudioImportLib())
                return true;

            // Fallback: check if AudioImportLib is already loaded in the app domain
            try
            {
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetName().Name == "AudioImportLib")
                    {
                        LoggingSystem.Info("Found existing AudioImportLib assembly in app domain", "EmbeddedAssemblyLoader");
                        _audioImportLibAssembly = assembly;
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Warning($"Error checking for existing AudioImportLib: {ex.Message}", "EmbeddedAssemblyLoader");
            }

            return false;
        }
    }
} 