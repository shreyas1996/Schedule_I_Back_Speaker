using BackSpeakerMod.S1Wrapper.Interfaces;
using System;
using UnityEngine;

namespace BackSpeakerMod.S1Wrapper
{
    /// <summary>
    /// Main API entry point for Schedule One game integration
    /// Provides unified access to all game systems with IL2CPP/Mono compatibility
    /// </summary>
    public static class S1API
    {
        #region Initialization

        private static bool _initialized = false;
        private static S1SystemStatus? _lastStatus = null;

        /// <summary>
        /// Initialize the S1API system
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            try
            {
                // Initialize environment detection
                var environment = S1Environment.IsIl2Cpp ? "IL2CPP" : "Mono";
                
                // Get initial system status
                _lastStatus = S1Factory.GetSystemStatus();
                
                _initialized = true;
            }
            catch (Exception)
            {
                _initialized = false;
            }
        }

        /// <summary>
        /// Check if S1API is properly initialized
        /// </summary>
        public static bool IsInitialized => _initialized;

        /// <summary>
        /// Get current system status
        /// </summary>
        public static S1SystemStatus GetSystemStatus()
        {
            if (!_initialized) Initialize();
            _lastStatus = S1Factory.GetSystemStatus();
            return _lastStatus;
        }

        #endregion

        #region Player Systems

        /// <summary>
        /// Get the local player instance
        /// </summary>
        public static IPlayer? Player => S1Factory.GetLocalPlayer();

        /// <summary>
        /// Get the player camera instance
        /// </summary>
        public static IPlayerCamera? PlayerCamera => S1Factory.GetPlayerCamera();

        /// <summary>
        /// Get the player's avatar
        /// </summary>
        public static IAvatar? PlayerAvatar => S1Factory.GetPlayerAvatar();

        #endregion

        #region Audio Systems

        /// <summary>
        /// Get the game's audio manager
        /// </summary>
        public static IAudioManager? AudioManager => S1Factory.FindAudioManager();

        /// <summary>
        /// Get the game's music player
        /// </summary>
        public static IMusicPlayer? MusicPlayer => S1Factory.FindMusicPlayer();

        #endregion

        #region UI Systems

        /// <summary>
        /// Get the phone instance
        /// </summary>
        public static IPhone? Phone => S1Factory.GetPhone();

        /// <summary>
        /// Get a PlayerSingleton instance of the specified type
        /// </summary>
        public static T? GetPlayerSingleton<T>() where T : class => S1DevUtilities.GetPlayerSingleton<T>();

        /// <summary>
        /// Check if a PlayerSingleton instance exists
        /// </summary>
        public static bool HasPlayerSingleton<T>() where T : class => S1DevUtilities.HasPlayerSingleton<T>();

        #endregion

        #region Game Objects

        /// <summary>
        /// Find all jukebox objects in the scene
        /// </summary>
        public static IJukebox[] FindJukeboxes() => S1Factory.FindJukeboxes();

        /// <summary>
        /// Get the game console
        /// </summary>
        public static IConsole? Console => S1Factory.GetConsole();

        /// <summary>
        /// Get the game registry
        /// </summary>
        public static IRegistry? Registry => S1Factory.GetRegistry();

        #endregion

        #region Asset Management

        /// <summary>
        /// Load an asset bundle from embedded resources
        /// </summary>
        public static bool LoadAssetBundle(string name)
        {
            var assetBundle = S1AssetBundleLoader.LoadFromEmbeddedResource(name);
            return assetBundle != null && assetBundle.IsValid;
        }

        /// <summary>
        /// Get a loaded asset bundle
        /// </summary>
        public static IAssetBundle? GetAssetBundle(string name) => S1AssetBundleLoader.GetAssetBundle(name);

        /// <summary>
        /// Unload an asset bundle
        /// </summary>
        public static void UnloadAssetBundle(string name) => S1AssetBundleLoader.UnloadAssetBundle(name);

        /// <summary>
        /// Load asset bundle from memory
        /// </summary>
        public static bool LoadAssetBundleFromMemory(string name, byte[] data) => S1AssetBundleLoader.LoadFromMemory(name, data);

        #endregion

        #region Game Input

        /// <summary>
        /// Check if the player is currently typing
        /// </summary>
        public static bool IsTyping
        {
            get => S1GameInput.IsTyping;
            set => S1GameInput.IsTyping = value;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Set layer recursively on a GameObject
        /// </summary>
        public static void SetLayerRecursively(GameObject obj, int layer) => S1DevUtilities.SetLayerRecursively(obj, layer);

        /// <summary>
        /// Check if running in IL2CPP environment
        /// </summary>
        public static bool IsIL2CPP => S1Environment.IsIl2Cpp;

        /// <summary>
        /// Get environment string (IL2CPP or Mono)
        /// </summary>
        public static string Environment => S1Environment.IsIl2Cpp ? "IL2CPP" : "Mono";

        #endregion

        #region Diagnostics

        /// <summary>
        /// Get detailed information about loaded asset bundles
        /// </summary>
        public static System.Collections.Generic.Dictionary<string, string> GetAssetBundleInfo()
            => S1AssetBundleLoader.GetLoadedBundleInfo();

        /// <summary>
        /// Perform a comprehensive system check
        /// </summary>
        public static SystemHealthReport GetSystemHealth()
        {
            if (!_initialized) Initialize();

            var status = GetSystemStatus();
            var report = new SystemHealthReport
            {
                Environment = status.Environment,
                IsInitialized = _initialized,
                SystemStatus = status,
                
                // Component availability
                HasPlayer = status.HasPlayer,
                HasAudioSystems = status.HasAudioManager && status.HasMusicPlayer,
                HasUISystems = status.HasPhone,
                HasGameSystems = status.HasConsole,
                
                // Asset management
                LoadedAssetBundles = GetAssetBundleInfo(),
                
                // Overall health
                OverallHealth = CalculateOverallHealth(status)
            };

            return report;
        }

        private static HealthStatus CalculateOverallHealth(S1SystemStatus status)
        {
            int healthScore = 0;
            int maxScore = 6;

            if (status.HasPlayer) healthScore++;
            if (status.HasPlayerCamera) healthScore++;
            if (status.HasAudioManager) healthScore++;
            if (status.HasPhone) healthScore++;
            if (status.HasConsole) healthScore++;
            if (_initialized) healthScore++;

            float healthPercentage = (float)healthScore / maxScore;

            if (healthPercentage >= 0.8f) return HealthStatus.Excellent;
            if (healthPercentage >= 0.6f) return HealthStatus.Good;
            if (healthPercentage >= 0.4f) return HealthStatus.Fair;
            if (healthPercentage >= 0.2f) return HealthStatus.Poor;
            return HealthStatus.Critical;
        }

        #endregion
    }

    /// <summary>
    /// System health report for diagnostics
    /// </summary>
    public class SystemHealthReport
    {
        public string Environment { get; set; } = "Unknown";
        public bool IsInitialized { get; set; }
        public S1SystemStatus SystemStatus { get; set; } = new S1SystemStatus();
        
        public bool HasPlayer { get; set; }
        public bool HasAudioSystems { get; set; }
        public bool HasUISystems { get; set; }
        public bool HasGameSystems { get; set; }
        
        public System.Collections.Generic.Dictionary<string, string> LoadedAssetBundles { get; set; } 
            = new System.Collections.Generic.Dictionary<string, string>();
        
        public HealthStatus OverallHealth { get; set; }

        public override string ToString()
        {
            return $"S1API Health Report [{Environment}]\n" +
                   $"Overall: {OverallHealth}\n" +
                   $"Initialized: {IsInitialized}\n" +
                   $"Player: {HasPlayer}, Audio: {HasAudioSystems}, UI: {HasUISystems}, Game: {HasGameSystems}\n" +
                   $"Asset Bundles: {LoadedAssetBundles.Count} loaded\n" +
                   $"Full Status: {SystemStatus}";
        }
    }

    /// <summary>
    /// Health status enumeration
    /// </summary>
    public enum HealthStatus
    {
        Critical,
        Poor,
        Fair,
        Good,
        Excellent
    }
} 