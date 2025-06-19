using BackSpeakerMod.S1Wrapper.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace BackSpeakerMod.S1Wrapper
{
    /// <summary>
    /// Core Schedule One game object factory
    /// Provides unified access to game systems with IL2CPP/Mono compatibility
    /// </summary>
    public static class S1Factory
    {
        #region Initialization

        private static bool _initialized = false;

        /// <summary>
        /// Initialize the S1Factory system
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            try
            {
                IL2CPPHelper.Initialize();
                _initialized = true;
            }
            catch (System.Exception)
            {
                _initialized = false;
            }
        }

        public static bool IsInitialized => _initialized;

        #endregion

        #region Player Systems

        /// <summary>
        /// Get the local player instance
        /// </summary>
        public static IPlayer? GetLocalPlayer()
        {
#if IL2CPP
            if (S1Environment.IsIl2Cpp)
            {
                var player = Il2CppScheduleOne.PlayerScripts.Player.Local;
                return player != null ? new Il2Cpp.Il2CppPlayer(player) : null;
            }
            return null;
#else
            var player = ScheduleOne.PlayerScripts.Player.Local;
            return player != null ? new Mono.MonoPlayer(player) : null;
#endif
        }

        /// <summary>
        /// Get the player camera instance
        /// </summary>
        public static IPlayerCamera? GetPlayerCamera()
        {
#if IL2CPP
            if (S1Environment.IsIl2Cpp)
            {
                var cam = Il2CppScheduleOne.PlayerScripts.PlayerCamera.Instance;
                return cam != null ? new Il2Cpp.Il2CppPlayerCamera(cam) : null;
            }
            return null;
#else
            var cam = ScheduleOne.PlayerScripts.PlayerCamera.Instance;
            return cam != null ? new Mono.MonoPlayerCamera(cam) : null;
#endif
        }

        /// <summary>
        /// Get the player avatar (via player)
        /// </summary>
        public static IAvatar? GetPlayerAvatar()
        {
            var player = GetLocalPlayer();
            return player?.Avatar;
        }

        #endregion

        #region Audio Systems

        /// <summary>
        /// Find the audio manager in the scene
        /// </summary>
        public static IAudioManager? FindAudioManager()
        {
#if IL2CPP
            if (S1Environment.IsIl2Cpp)
            {
                var mgr = UnityEngine.Object.FindObjectOfType<Il2CppScheduleOne.Audio.AudioManager>();
                return mgr != null ? new Il2Cpp.Il2CppAudioManager(mgr) : null;
            }
            return null;
#else
            var mgr = UnityEngine.Object.FindObjectOfType<ScheduleOne.Audio.AudioManager>();
            return mgr != null ? new Mono.MonoAudioManager(mgr) : null;
#endif
        }

        /// <summary>
        /// Find the music player in the scene
        /// </summary>
        public static IMusicPlayer? FindMusicPlayer()
        {
#if IL2CPP
            if (S1Environment.IsIl2Cpp)
            {
                var player = UnityEngine.Object.FindObjectOfType<Il2CppScheduleOne.Audio.MusicPlayer>();
                return player != null ? new Il2Cpp.Il2CppMusicPlayer(player) : null;
            }
            return null;
#else
            var player = UnityEngine.Object.FindObjectOfType<ScheduleOne.Audio.MusicPlayer>();
            return player != null ? new Mono.MonoMusicPlayer(player) : null;
#endif
        }

        #endregion

        #region UI Systems

        /// <summary>
        /// Get the phone instance (PlayerSingleton)
        /// </summary>
        public static IPhone? GetPhone()
        {
#if IL2CPP
            if (S1Environment.IsIl2Cpp)
            {
                var phone = Il2CppScheduleOne.DevUtilities.PlayerSingleton<Il2CppScheduleOne.UI.Phone.Phone>.instance;
                return phone != null ? new Il2Cpp.Il2CppPhone(phone) : null;
            }
            return null;
#else
            var phone = ScheduleOne.DevUtilities.PlayerSingleton<ScheduleOne.UI.Phone.Phone>.instance;
            return phone != null ? new Mono.MonoPhone(phone) : null;
#endif
        }

        #endregion

        #region Object Scripts

        /// <summary>
        /// Find all jukebox objects in the scene
        /// </summary>
        public static IJukebox[] FindJukeboxes()
        {
#if IL2CPP
            if (S1Environment.IsIl2Cpp)
            {
                var jukeboxes = UnityEngine.Object.FindObjectsOfType<Il2CppScheduleOne.ObjectScripts.Jukebox>();
                if (jukeboxes != null && jukeboxes.Length > 0)
                {
                    var wrappers = new IJukebox[jukeboxes.Length];
                    for (int i = 0; i < jukeboxes.Length; i++)
                    {
                        wrappers[i] = new Il2Cpp.Il2CppJukebox(jukeboxes[i]);
                    }
                    return wrappers;
                }
            }
            return new IJukebox[0];
#else
            var jukeboxes = UnityEngine.Object.FindObjectsOfType<ScheduleOne.ObjectScripts.Jukebox>();
            if (jukeboxes != null && jukeboxes.Length > 0)
            {
                var wrappers = new IJukebox[jukeboxes.Length];
                for (int i = 0; i < jukeboxes.Length; i++)
                {
                    wrappers[i] = new Mono.MonoJukebox(jukeboxes[i]);
                }
                return wrappers;
            }
            return new IJukebox[0];
#endif
        }

        #endregion

        #region Game Systems

        /// <summary>
        /// Get the console instance
        /// </summary>
        public static IConsole? GetConsole()
        {
#if IL2CPP
            if (S1Environment.IsIl2Cpp)
            {
                var console = UnityEngine.Object.FindObjectOfType<Il2CppScheduleOne.Console>();
                return console != null ? new Il2Cpp.Il2CppConsole(console) : null;
            }
            return null;
#else
            var console = UnityEngine.Object.FindObjectOfType<ScheduleOne.Console>();
            return console != null ? new Mono.MonoConsole(console) : null;
#endif
        }

        /// <summary>
        /// Get the registry instance
        /// </summary>
        public static IRegistry? GetRegistry()
        {
#if IL2CPP
            if (S1Environment.IsIl2Cpp)
            {
                return new Il2Cpp.Il2CppRegistry();
            }
            return null;
#else
            return new Mono.MonoRegistry();
#endif
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Get a PlayerSingleton instance
        /// </summary>
        public static T? GetPlayerSingleton<T>() where T : class => S1DevUtilities.GetPlayerSingleton<T>();

        /// <summary>
        /// Set layer recursively on a GameObject
        /// </summary>
        public static void SetLayerRecursively(GameObject obj, int layer) => S1DevUtilities.SetLayerRecursively(obj, layer);

        /// <summary>
        /// Check if the player is currently typing
        /// </summary>
        public static bool IsTyping
        {
            get => S1GameInput.IsTyping;
            set => S1GameInput.IsTyping = value;
        }

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
        /// Register a type in IL2CPP (if needed)
        /// </summary>
        public static void RegisterType<T>() where T : UnityEngine.Object => IL2CPPHelper.RegisterIl2CppType<T>();

        /// <summary>
        /// Gets all active Schedule One systems in a single call
        /// Useful for initialization and system status checks
        /// </summary>
        public static S1SystemStatus GetSystemStatus()
        {
            return new S1SystemStatus
            {
                HasPlayer = GetLocalPlayer() != null,
                HasPlayerCamera = GetPlayerCamera() != null,
                HasAudioManager = FindAudioManager() != null,
                HasMusicPlayer = FindMusicPlayer() != null,
                HasPhone = GetPhone() != null,
                JukeboxCount = FindJukeboxes().Length,
                HasConsole = GetConsole() != null,
                Environment = S1Environment.IsIl2Cpp ? "IL2CPP" : "Mono"
            };
        }

        #endregion
    }

    /// <summary>
    /// Status information about Schedule One systems
    /// </summary>
    public class S1SystemStatus
    {
        public bool HasPlayer { get; set; }
        public bool HasPlayerCamera { get; set; }
        public bool HasAudioManager { get; set; }
        public bool HasMusicPlayer { get; set; }
        public bool HasPhone { get; set; }
        public int JukeboxCount { get; set; }
        public bool HasConsole { get; set; }
        public string Environment { get; set; } = "Unknown";

        public override string ToString()
        {
            return $"S1 Systems [{Environment}]: Player={HasPlayer}, Camera={HasPlayerCamera}, " +
                   $"Audio={HasAudioManager}, Music={HasMusicPlayer}, Phone={HasPhone}, " +
                   $"Jukeboxes={JukeboxCount}, Console={HasConsole}";
        }
    }
}
