using System;
using System.IO;
using System.Reflection;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.Utils
{
    /// <summary>
    /// External assembly loader for AudioImportLib
    /// No longer embeds DLL - loads from external file instead
    /// </summary>
    public static class EmbeddedAssemblyLoader
    {
        private static Assembly? _audioImportLibAssembly;
        private static bool _loadAttempted = false;

        /// <summary>
        /// Load the external AudioImportLib assembly
        /// </summary>
        public static bool LoadAudioImportLib()
        {
            if (_loadAttempted)
                return _audioImportLibAssembly != null;

            _loadAttempted = true;

            try
            {
                LoggingSystem.Debug("Loading external AudioImportLib.dll...", "EmbeddedAssemblyLoader");

                // Check if external file exists (UserLibs first, then Libs directory)
                var audioImportLibPath = DependencyChecker.GetAudioImportLibPath();
                
                if (!File.Exists(audioImportLibPath))
                {
                    LoggingSystem.Warning($"AudioImportLib.dll not found at: {audioImportLibPath}", "EmbeddedAssemblyLoader");
                    LoggingSystem.Info($"Preferred location: {DependencyChecker.AudioImportLibUserLibsPath}", "EmbeddedAssemblyLoader");
                    LoggingSystem.Info($"Alternative location: {DependencyChecker.AudioImportLibLibsPath}", "EmbeddedAssemblyLoader");
                    LoggingSystem.Info("Audio loading functionality will be limited without AudioImportLib.dll", "EmbeddedAssemblyLoader");
                    return false;
                }

                LoggingSystem.Debug($"Loading AudioImportLib.dll from: {audioImportLibPath}", "EmbeddedAssemblyLoader");

                // Load the external DLL
                _audioImportLibAssembly = Assembly.LoadFrom(audioImportLibPath);

                LoggingSystem.Info($"Successfully loaded external AudioImportLib: {_audioImportLibAssembly.FullName}", "EmbeddedAssemblyLoader");
                return true;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to load external AudioImportLib: {ex.Message}", "EmbeddedAssemblyLoader");
                LoggingSystem.Info($"To enable full audio features, place AudioImportLib.dll at: {DependencyChecker.AudioImportLibPath}", "EmbeddedAssemblyLoader");
                return false;
            }
        }

        /// <summary>
        /// Get the AudioImportLib API type for calling LoadAudioClip
        /// </summary>
        public static Type? GetAudioImportLibApiType()
        {
            if (!LoadAudioImportLib() || _audioImportLibAssembly == null)
                return null;

            try
            {
                // Look for the API class in the assembly
                var types = _audioImportLibAssembly.GetTypes();
                foreach (var type in types)
                {
                    if (type.Name.Contains("API") || type.Name.Contains("AudioImport"))
                    {
                        LoggingSystem.Debug($"Found AudioImportLib API type: {type.FullName}", "EmbeddedAssemblyLoader");
                        return type;
                    }
                }

                LoggingSystem.Warning("Could not find API type in AudioImportLib assembly", "EmbeddedAssemblyLoader");
                return null;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error getting AudioImportLib API type: {ex.Message}", "EmbeddedAssemblyLoader");
                return null;
            }
        }

        /// <summary>
        /// Get the loaded AudioImportLib assembly
        /// </summary>
        public static Assembly? GetAudioImportLibAssembly()
        {
            LoadAudioImportLib();
            return _audioImportLibAssembly;
        }

        /// <summary>
        /// Check if AudioImportLib is available and loaded
        /// </summary>
        public static bool IsAudioImportLibAvailable()
        {
            // First check if external file exists
            if (!DependencyChecker.CheckAudioImportLib())
                return false;

            // Then try to load it
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
        
        /// <summary>
        /// Get setup instructions for missing AudioImportLib
        /// </summary>
        public static string GetSetupInstructions()
        {
            if (IsAudioImportLibAvailable())
                return "AudioImportLib.dll is available! ✓";
            
            return $"AudioImportLib.dll is missing.\n" +
                   $"Preferred location: {DependencyChecker.AudioImportLibUserLibsPath}\n" +
                   $"Alternative location: {DependencyChecker.AudioImportLibLibsPath}\n" +
                   $"Contact the mod author for this dependency.\n" +
                   $"Audio loading will be limited without this library.";
        }
    }
} 