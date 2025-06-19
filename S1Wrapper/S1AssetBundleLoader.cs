using BackSpeakerMod.S1Wrapper.Interfaces;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System;
using BackSpeakerMod.NewBackend.Utils;

namespace BackSpeakerMod.S1Wrapper
{
    /// <summary>
    /// Asset bundle loader that handles both IL2CPP and Mono runtimes
    /// Uses Il2CppAssetBundleManager + Il2CppAssetBundle for IL2CPP
    /// Uses AssetBundleModule + AssetBundle for Mono
    /// </summary>
    public static class S1AssetBundleLoader
    {
        private static Dictionary<string, IAssetBundle> _assetBundles = new Dictionary<string, IAssetBundle>();
        private static Dictionary<string, string> _tempFiles = new Dictionary<string, string>();

        public static IAssetBundle? GetAssetBundle(string name)
        {
            if (_assetBundles.ContainsKey(name))
            {
                return _assetBundles[name];
            }
            return null;
        }

        /// <summary>
        /// Load an asset bundle from an embedded resource
        /// </summary>
        public static IAssetBundle? LoadFromEmbeddedResource(string resourceName)
        {
            NewLoggingSystem.Info($"Loading asset bundle from embedded resource: {resourceName}", "S1AssetBundleLoader");

            try
            {
                // Get the embedded resource
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var fullResourceName = $"BackSpeakerMod.EmbeddedResources.{resourceName}";
                
                NewLoggingSystem.Debug($"Looking for embedded resource: {fullResourceName}", "S1AssetBundleLoader");
                
                using (var stream = assembly.GetManifestResourceStream(fullResourceName))
                {
                    if (stream == null)
                    {
                        NewLoggingSystem.Error($"Embedded resource not found: {fullResourceName}", "S1AssetBundleLoader");
                        
                        // List all available resources for debugging
                        var resourceNames = assembly.GetManifestResourceNames();
                        NewLoggingSystem.Debug($"Available embedded resources: [{string.Join(", ", resourceNames)}]", "S1AssetBundleLoader");
                        return null;
                    }

                    NewLoggingSystem.Debug($"Found embedded resource, size: {stream.Length} bytes", "S1AssetBundleLoader");

                    // Read the stream into a byte array
                    var data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                    
                    NewLoggingSystem.Debug($"Read {data.Length} bytes from embedded resource", "S1AssetBundleLoader");

                    // Load from memory using the appropriate method for the runtime
                    var bundle = IL2CPPHelper.LoadFromMemory(data);
                    if (bundle != null)
                    {
                        NewLoggingSystem.Debug($"Loaded asset bundle: {resourceName}", "S1AssetBundleLoader");
                        _assetBundles.Add(resourceName, new Il2Cpp.Il2CppAssetBundleWrapper(bundle));
                        return _assetBundles[resourceName];
                    }
                    NewLoggingSystem.Error($"Failed to load asset bundle: {resourceName}", "S1AssetBundleLoader");
                    return null;
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to load embedded resource {resourceName}: {ex.Message}", "S1AssetBundleLoader");
                NewLoggingSystem.Debug($"Exception details: {ex}", "S1AssetBundleLoader");
                return null;
            }
        }

        /// <summary>
        /// Load an asset bundle from a file path
        /// </summary>
        // public static IAssetBundle? LoadFromFile(string path)
        // {
        //     if (string.IsNullOrEmpty(path) || !File.Exists(path))
        //     {
        //         NewLoggingSystem.Error($"Asset bundle file not found: {path}", "S1AssetBundleLoader");
        //         return null;
        //     }

        //     NewLoggingSystem.Debug($"Loading asset bundle from file: {path}", "S1AssetBundleLoader");

        //     #if IL2CPP
        //         if (S1Environment.IsIl2Cpp)
        //         {
        //             NewLoggingSystem.Debug("Using IL2CPP file loading path", "S1AssetBundleLoader");
        //             return IL2CPPHelper.LoadFromFile(path);
        //         }
        //         else
        //         {
        //             NewLoggingSystem.Debug("IL2CPP build but not IL2CPP runtime, using Mono file path", "S1AssetBundleLoader");
        //             return LoadFromFileMono(path);
        //         }
        //     #else
        //         NewLoggingSystem.Debug("Using Mono file loading path", "S1AssetBundleLoader");
        //         return LoadFromFileMono(path);
        //     #endif
        // }

        public static void UnloadAssetBundle(string name)
        {
            if (_assetBundles.ContainsKey(name))
            {
                try
                {
                    _assetBundles[name].Unload(true);
                }
                catch (Exception)
                {
                    NewLoggingSystem.Error($"Failed to unload asset bundle: {name}", "S1AssetBundleLoader");
                    // Handle unload failures silently
                }
                _assetBundles.Remove(name);
            }

            // Clean up temp file if it exists
            if (_tempFiles.ContainsKey(name))
            {
                try
                {
                    File.Delete(_tempFiles[name]);
                }
                catch (Exception)
                {
                    NewLoggingSystem.Error($"Failed to delete temp file: {_tempFiles[name]}", "S1AssetBundleLoader");
                    // File cleanup failures are non-critical
                }
                _tempFiles.Remove(name);
            }
        }

        public static void UnloadAllAssetBundles()
        {
            // Unload all asset bundles
            foreach (var kvp in _assetBundles)
            {
                try
                {
                    kvp.Value.Unload(true);
                }
                catch (Exception)
                {
                    NewLoggingSystem.Error($"Failed to unload asset bundle: {kvp.Key}", "S1AssetBundleLoader");
                    // Handle unload failures silently
                }
            }
            _assetBundles.Clear();

            // Clean up all temp files
            foreach (var kvp in _tempFiles)
            {
                try
                {
                    File.Delete(kvp.Value);
                }
                catch (Exception)
                {
                    NewLoggingSystem.Error($"Failed to delete temp file: {kvp.Value}", "S1AssetBundleLoader");
                    // File cleanup failures are non-critical
                }
            }
            _tempFiles.Clear();
        }

        /// <summary>
        /// Load AssetBundle from memory (for advanced scenarios)
        /// </summary>
        public static bool LoadFromMemory(string name, byte[] data)
        {
            if (_assetBundles.ContainsKey(name) || data == null || data.Length == 0)
            {
                return false;
            }

            try
            {
#if IL2CPP
                if (S1Environment.IsIl2Cpp)
                {
                    try
                    {
                        // Try Il2CppAssetBundleManager first
#pragma warning disable CS0618
                        var bundle = Il2CppAssetBundleManager.LoadFromMemory(data);
#pragma warning restore CS0618
                        if (bundle != null)
                        {
                            _assetBundles.Add(name, new Il2Cpp.Il2CppAssetBundleWrapper(bundle));
                            return true;
                        }
                    }
                    catch (Exception)
                    {
                        // Fallback to regular AssetBundle
                    }

                    // Fallback approach
                    try
                    {
                        var bundle = Il2CppAssetBundleManager.LoadFromMemory(data);
                        if (bundle != null)
                        {
                            _assetBundles.Add(name, new Il2Cpp.Il2CppAssetBundleWrapper(bundle));
                            return true;
                        }
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }
                return false;
#else
#pragma warning disable CS0618
                var bundle = Il2CppAssetBundleManager.LoadFromMemory(data);
#pragma warning restore CS0618
                if (bundle != null)
                {
                    _assetBundles.Add(name, new Mono.MonoAssetBundleWrapper(bundle));
                    return true;
                }
                return false;
#endif
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Get information about all loaded asset bundles
        /// </summary>
        public static Dictionary<string, string> GetLoadedBundleInfo()
        {
            var info = new Dictionary<string, string>();
            foreach (var kvp in _assetBundles)
            {
                try
                {
                    var bundle = kvp.Value;
                    info[kvp.Key] = $"Loaded, Assets: {bundle.GetAllAssetNames().Length}";
                }
                catch (Exception)
                {
                    info[kvp.Key] = "Loaded, Info unavailable";
                }
            }
            return info;
        }
    }
}
