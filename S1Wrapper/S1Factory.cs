using BackSpeakerMod.S1Wrapper.Interfaces;
using BackSpeakerMod.NewBackend.Utils;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Reflection;

// Conditional imports for Schedule I types
#if IL2CPP
using Il2CppScheduleOne.UI;
using Il2CppScheduleOne.UI.Phone.ProductManagerApp;
#else
using ScheduleOne.UI;
using ScheduleOne.UI.Phone.ProductManagerApp;
#endif

namespace BackSpeakerMod.S1Wrapper
{
    /// <summary>
    /// App template types available for cloning
    /// </summary>
    public enum AppTemplate
    {
        ProductManagerApp,
        // Add other app templates as needed
        // MusicApp,
        // SettingsApp,
        // etc.
    }

    /// <summary>
    /// App creation configuration
    /// </summary>
    public class AppCreationConfig
    {
        public AppTemplate Template { get; set; }
        public string AppName { get; set; } = "";
        public string IconLabel { get; set; } = "";
        public string? IconSpritePath { get; set; }
        public Action? OnIconClick { get; set; }
        public Action? SetupAppContent { get; set; }
    }

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

        #region App Creation and Management

        /// <summary>
        /// Clone app exactly like old code - simple and direct
        /// </summary>
        public static (IApp?, GameObject?) CloneApp(string appName, System.Action? onIconClick)
        {
            try
            {
                NewLoggingSystem.Debug($"Cloning app: {appName}", "S1Factory");

                // Get AppsCanvas and HomeScreen
                var appsCanvas = GameObject.Find("AppsCanvas");
                var homeScreen = GameObject.Find("HomeScreen");

                if (appsCanvas == null || homeScreen == null)
                {
                    NewLoggingSystem.Error("AppsCanvas not found", "S1Factory");
                    return (null,null);
                }

                // ColorBlock setup
                ColorBlock colorBlock = default(ColorBlock);
                colorBlock.normalColor = new Color(0.25f, 0.25f, 0.25f, 0.1f);
                colorBlock.highlightedColor = new Color(0.25f, 0.275f, 0.35f, 0.3f);
                colorBlock.pressedColor = new Color(0.5f, 0.5f, 0.5f, 0.4f);
                colorBlock.selectedColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                colorBlock.fadeDuration = 0.1f;
                colorBlock.colorMultiplier = 1f;
                
                // Find ProductManagerApp to clone (exactly like old code)
                var productManagerAppObj = appsCanvas.transform.FindChild("ProductManagerApp");
                if (productManagerAppObj == null)
                {
                    NewLoggingSystem.Error("ProductManagerApp not found", "S1Factory");
                    return (null,null);
                }
                
                
                // Clone ProductManagerApp (exactly like old code)
                var clonedCanvas = UnityEngine.Object.Instantiate<GameObject>(productManagerAppObj.gameObject, appsCanvas.transform);
                
                // Get the App<ProductManagerApp> component directly
                var appComponent = GetDirectAppComponent(clonedCanvas);
                if (appComponent != null)
                {
                    appComponent.AppName = appName;
                }

                clonedCanvas.name = appName + "App";
                clonedCanvas.transform.localPosition = Vector3.zero;
                clonedCanvas.transform.localScale = Vector3.one;
                clonedCanvas.transform.localRotation = Quaternion.identity;
                clonedCanvas.active = false;

                // CRITICAL: Set proper canvas sorting to prevent bleeding
                var canvasComponent = clonedCanvas.GetComponent<Canvas>();
                if (canvasComponent != null)
                {
                    canvasComponent.sortingOrder = 0; // Keep same level as other apps
                    canvasComponent.overrideSorting = false; // Don't override phone's sorting
                }

                NewLoggingSystem.Debug($"✓ Cloned ProductManagerApp as {clonedCanvas.name}", "S1Factory");
                
                
                // Modify the LAST existing icon (exactly like old code)
                ModifyLastIcon(appName, clonedCanvas, onIconClick);
                
                // Return simple wrapper
                if (appComponent != null)
                {
                    var wrapper = CreateSimpleAppWrapper(appComponent);
                    NewLoggingSystem.Debug($"✓ {appName} app created successfully", "S1Factory");
                    return (wrapper,clonedCanvas);
                }
                
                return (null,null);
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception cloning app: {ex}", "S1Factory");
                return (null,null);
            }
        }
        
        /// <summary>
        /// Modify the last icon exactly like old code
        /// </summary>
        private static void ModifyLastIcon(string appName, GameObject clonedCanvas, System.Action? onIconClick)
        {
            try
            {
                var appIcons = GameObject.Find("AppIcons");
                if (appIcons == null) return;
                
                int iconCount = appIcons.transform.childCount;
                if (iconCount == 0) return;
                
                // Get the LAST existing icon (exact old code pattern)
                var appIcon = appIcons.transform.GetChild(iconCount - 1).gameObject;
                
                // Update icon label
                var label = appIcon.transform.FindChild("Label").gameObject.GetComponent<Text>();
                if (label != null)
                {
                    NewLoggingSystem.Debug($"Setting label text to: {appName}", "S1Factory");
                    label.text = appName;
                }
                
                // Set BackSpeaker sprite
                var backSpeakerSprite = BackSpeakerMod.NewBackend.Utils.ResourceLoader.LoadEmbeddedSprite("BackSpeakerMod.EmbeddedResources.back_speaker_logo.png");
                if (backSpeakerSprite != null)
                {
                    var mask = appIcon.transform.FindChild("Mask").GetChild(0).GetComponent<Image>();
                    if (mask != null)
                    {
                        mask.sprite = backSpeakerSprite;
                    }
                }
                
                // Set button click handler
                var appButton = appIcon.GetComponent<Button>();
                if (appButton != null)
                {
                    // appButton.onClick.RemoveAllListeners();
                    appButton.onClick.AddListener((UnityEngine.Events.UnityAction)delegate() { onIconClick?.Invoke(); });
                }
                
                NewLoggingSystem.Debug("✓ Modified last icon for BackSpeaker", "S1Factory");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to modify last icon: {ex}", "S1Factory");
            }
        }
        
        
        /// <summary>
        /// Find app container (like old code)
        /// </summary>
        private static Transform? FindAppContainer(GameObject appCanvas)
        {
            return appCanvas.transform.FindChild("Container");
        }

        /// <summary>
        /// Get the App<ProductManagerApp> component directly using conditional compilation
        /// </summary>
        private static App<ProductManagerApp>? GetDirectAppComponent(GameObject canvas)
        {
            try
            {
                NewLoggingSystem.Debug($"Getting direct App component from canvas: {canvas.name}", "S1Factory");
                
                var appComponent = canvas.GetComponent<App<ProductManagerApp>>();
                if (appComponent != null)
                {
                    NewLoggingSystem.Debug("✓ Found App<ProductManagerApp> component", "S1Factory");
                    return appComponent;
                }
                
                NewLoggingSystem.Warning("No App<ProductManagerApp> component found", "S1Factory");
                return null;
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception getting direct App component: {ex}", "S1Factory");
                return null;
            }
        }
        
        /// <summary>
        /// Create a simple wrapper for the App component
        /// </summary>
        private static IApp? CreateSimpleAppWrapper(App<ProductManagerApp> appComponent)
        {
            try
            {
                if (appComponent == null) return null;
                
                NewLoggingSystem.Debug($"Creating simple wrapper for component: {appComponent.GetType().Name}", "S1Factory");
                
                // Create appropriate wrapper based on build configuration
#if IL2CPP
                return new Il2Cpp.Il2CppApp(appComponent);
#else
                return new Mono.MonoApp(appComponent);
#endif
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to create simple wrapper: {ex.Message}", "S1Factory");
                return null;
            }
        }

        /// <summary>
        /// Get the app canvas for UI creation
        /// </summary>
        public static Transform? GetAppCanvas(IApp app)
        {
            if (app == null) return null;

            try
            {
                // Try to find the app's specific canvas first
                var appName = app.AppName;
                if (!string.IsNullOrEmpty(appName))
                {
                    var appsCanvas = UIWrapper.S1PhoneAppBuilder.GetAppsCanvas();
                    if (appsCanvas != null)
                    {
                        var appCanvas = appsCanvas.Find(appName);
                        if (appCanvas != null)
                        {
                            var container = appCanvas.Find("Container");
                            if (container != null)
                            {
                                return container;
                            }
                            return appCanvas;
                        }
                    }
                }

                // Fallback to phone canvas
                var phoneCanvas = UIWrapper.S1PhoneAppBuilder.GetAppsCanvas();
                if (phoneCanvas != null)
                {
                    return phoneCanvas;
                }
                
                NewLoggingSystem.Warning("No suitable canvas found for app", "S1Factory");
                return null;
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception getting app canvas: {ex}", "S1Factory");
                return null;
            }
        }

        /// <summary>
        /// Get the phone homescreen
        /// </summary>
        public static Transform? GetHomescreen()
        {
            try
            {
                return UIWrapper.S1PhoneAppBuilder.GetHomeScreen();
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception getting homescreen: {ex}", "S1Factory");
                return null;
            }
        }

        /// <summary>
        /// Register app on homescreen
        /// </summary>
        public static bool RegisterAppOnHomescreen(IApp app, string appName, string iconLabel)
        {
            if (app == null) return false;

            try
            {
                // The S1PhoneAppBuilder already handles icon creation during app creation
                // This method is mainly for compatibility and logging
                NewLoggingSystem.Debug($"✓ App '{appName}' registered on homescreen", "S1Factory");
                return true;
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception registering app on homescreen: {ex}", "S1Factory");
                return false;
            }
        }

        /// <summary>
        /// Check if phone system is available
        /// </summary>
        public static bool IsPhoneAvailable()
        {
            return UIWrapper.S1PhoneAppBuilder.IsPhoneAvailable();
        }

        /// <summary>
        /// Get App<T> component from a GameObject - Generic helper
        /// </summary>
        public static IApp? GetAppComponent(GameObject gameObject)
        {
            try
            {
                var components = gameObject.GetComponents<Component>();
                
                foreach (var component in components)
                {
                    var componentType = component.GetType();
                    
                    // Look for App<T> generic types
                    if (componentType.IsGenericType && componentType.GetGenericTypeDefinition().Name.Contains("App"))
                    {
                        NewLoggingSystem.Debug($"Found App<T> component: {componentType.FullName}", "S1Factory");
                        
                        // Wrap it in our interface
#if IL2CPP
                        return new Il2Cpp.Il2CppApp(component);
#endif
#if !IL2CPP
                        return new BackSpeakerMod.S1Wrapper.Mono.MonoApp(component);
#endif
                    }
                }
                
                NewLoggingSystem.Warning("No App<T> component found", "S1Factory");
                return null;
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception getting App component: {ex}", "S1Factory");
                return null;
            }
        }

        /// <summary>
        /// Get App<T> component by name - searches all app canvases
        /// </summary>
        public static IApp? GetAppByName(string appName)
        {
            try
            {
                var appsCanvas = GameObject.Find("AppsCanvas");
                if (appsCanvas == null) return null;

                var appCanvas = appsCanvas.transform.Find(appName + "App");
                if (appCanvas == null) return null;

                return GetAppComponent(appCanvas.gameObject);
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception getting app by name: {ex}", "S1Factory");
                return null;
            }
        }

        /// <summary>
        /// Add component using S1Factory (ensures proper IL2CPP compatibility)
        /// NOTE: Built-in Unity components (Button, Text, Image, etc.) work automatically.
        /// Only custom MonoBehaviour components may need special handling.
        /// </summary>
        public static T? AddComponent<T>(GameObject gameObject) where T : Component
        {
            if (gameObject == null) return null;

            try
            {
                // Unity handles built-in components automatically in both Mono and IL2CPP
                return gameObject.AddComponent<T>();
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception adding component {typeof(T).Name}: {ex}", "S1Factory");
                return null;
            }
        }

        public static T? RegisterAndAddComponent<T>(GameObject gameObject) where T : Component
        {
            NewLoggingSystem.Debug($"Registering and adding component: {typeof(T).Name}", "S1Factory");
            RegisterComponent<T>();
            return AddComponent<T>(gameObject);
        }

        public static void RegisterComponent<T>() where T : Component
        {
            NewLoggingSystem.Debug($"Registering component: {typeof(T).Name}", "S1Factory");
            // register the component using the IL2CPPHelper if IL2CPP
            #if IL2CPP
            if (S1Environment.IsIl2Cpp)
            {
                IL2CPPHelper.RegisterIl2CppType<T>();
            }
            #endif
            return;
        }

        /// <summary>
        /// Register multiple custom component types for IL2CPP compatibility
        /// NOTE: Only use this for CUSTOM MonoBehaviour components that you've created.
        /// Built-in Unity components (Button, Text, Image, LayoutGroups, etc.) do NOT need registration.
        /// </summary>
        public static void RegisterComponents(params Type[] componentTypes)
        {
            if (componentTypes == null || componentTypes.Length == 0)
            {
                NewLoggingSystem.Warning("No component types provided for registration", "S1Factory");
                return;
            }

            try
            {
#if IL2CPP
                if (S1Environment.IsIl2Cpp)
                {
                    foreach (var type in componentTypes)
                    {
                        if (type.IsSubclassOf(typeof(UnityEngine.Component)))
                        {
                            // IL2CPP type registration handled automatically by AddComponent
                            NewLoggingSystem.Debug($"Ready to register component: {type.Name}", "S1Factory");
                        }
                    }
                }
#endif
                NewLoggingSystem.Debug($"Registered {componentTypes.Length} component types", "S1Factory");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to register components: {ex.Message}", "S1Factory");
            }
        }

        /// <summary>
        /// Find GameObject by name (with optional parent search)
        /// </summary>
        public static GameObject? FindGameObject(string name, Transform? parent = null)
        {
            try
            {
                if (parent != null)
                {
                    var childTransform = parent.Find(name);
                    return childTransform?.gameObject;
                }
                else
                {
                    return GameObject.Find(name);
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception finding GameObject '{name}': {ex}", "S1Factory");
                return null;
            }
        }

        /// <summary>
        /// Instantiate GameObject (safe wrapper)
        /// </summary>
        public static GameObject? Instantiate(GameObject original, Transform? parent = null)
        {
            try
            {
                if (original == null) return null;
                
                GameObject instance;
                if (parent != null)
                {
                    instance = UnityEngine.Object.Instantiate(original, parent);
                }
                else
                {
                    instance = UnityEngine.Object.Instantiate(original);
                }
                
                return instance;
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception instantiating GameObject: {ex}", "S1Factory");
                return null;
            }
        }

        /// <summary>
        /// Find all objects of type (safe wrapper)
        /// </summary>
        public static T[] FindObjectsOfType<T>() where T : UnityEngine.Object
        {
            try
            {
                return UnityEngine.Object.FindObjectsOfType<T>();
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception finding objects of type {typeof(T).Name}: {ex}", "S1Factory");
                return new T[0];
            }
        }

        /// <summary>
        /// Get components in children (safe wrapper)
        /// </summary>
        public static T[] GetComponentsInChildren<T>(GameObject gameObject) where T : Component
        {
            try
            {
                if (gameObject == null) return new T[0];
                return gameObject.GetComponentsInChildren<T>();
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception getting components in children: {ex}", "S1Factory");
                return new T[0];
            }
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
