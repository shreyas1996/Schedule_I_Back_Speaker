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
#if IL2CPP
                IL2CPPHelper.Initialize();
#endif
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
            var phone = ScheduleOne.DevUtilities.PlayerSingleton<ScheduleOne.UI.Phone.Phone>.Instance;
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

        private static void SetupPortraitContainer(Transform container, string appName)
        {
            var containerRect = container.GetComponent<RectTransform>();
            if (containerRect == null)
            {
                NewLoggingSystem.Warning($"Container has no RectTransform in {appName}", "S1Factory");
                return;
            }
            
            NewLoggingSystem.Info($"Original container: Size={containerRect.sizeDelta}, Rotation={containerRect.localRotation.eulerAngles}, Scale={containerRect.localScale}", "S1Factory");
            
            // DEBUGGING: Let's trace the entire hierarchy and their sizes
            NewLoggingSystem.Info("=== HIERARCHY DEBUG ===", "S1Factory");
            var current = containerRect;
            var level = 0;
            while (current != null && level < 5)
            {
                var hierarchySize = new Vector2(current.rect.width, current.rect.height);
                NewLoggingSystem.Info($"Level {level}: {current.name} - sizeDelta={current.sizeDelta}, actualSize={hierarchySize}, anchors=({current.anchorMin}, {current.anchorMax})", "S1Factory");
                current = current.parent?.GetComponent<RectTransform>();
                level++;
            }
            NewLoggingSystem.Info("=== END HIERARCHY DEBUG ===", "S1Factory");
            
            // STEP 1: Get the actual calculated size from Unity's layout system
            // The container has sizeDelta=(0,0) but Unity calculates the actual size as (1201, 655)
            var actualSize = new Vector2(containerRect.rect.width, containerRect.rect.height);
            var originalSizeDelta = containerRect.sizeDelta;
            var originalAnchors = $"min={containerRect.anchorMin}, max={containerRect.anchorMax}";
            
            NewLoggingSystem.Info($"Container original sizeDelta: {originalSizeDelta}, actualSize: {actualSize}, anchors: {originalAnchors}", "S1Factory");
            
            // STEP 2: Set explicit portrait dimensions and disable stretch behavior
            // We need to break away from the landscape parent and set our own portrait size
            if (actualSize.x > 0 && actualSize.y > 0)
            {
                // Swap dimensions for portrait: landscape width (1201) becomes portrait height
                var portraitWidth = actualSize.y;   // 655 becomes width
                var portraitHeight = actualSize.x;  // 1201 becomes height
                
                // Set explicit size instead of stretching
                containerRect.sizeDelta = new Vector2(portraitWidth, portraitHeight);
                
                // Use center anchors so it doesn't stretch to fill parent
                containerRect.anchorMin = new Vector2(0.5f, 0.5f);  // Center anchor
                containerRect.anchorMax = new Vector2(0.5f, 0.5f);  // Center anchor
                containerRect.anchoredPosition = Vector2.zero;      // Centered position
                containerRect.pivot = new Vector2(0.5f, 0.5f);      // Center pivot
                
                // STEP 3: Rotate the container 90 degrees counter-clockwise for portrait orientation
                // This makes the portrait-sized container display in portrait orientation
                containerRect.localRotation = Quaternion.Euler(0, 0, 90);
                
                NewLoggingSystem.Info($"Container set to explicit portrait size: {containerRect.sizeDelta} and rotated 90°", "S1Factory");
            }
            else
            {
                NewLoggingSystem.Warning("Could not get actual container size, using fallback portrait dimensions", "S1Factory");
                // Fallback portrait dimensions
                containerRect.sizeDelta = new Vector2(655f, 1201f);
                containerRect.anchorMin = new Vector2(0.5f, 0.5f);
                containerRect.anchorMax = new Vector2(0.5f, 0.5f);
                containerRect.anchoredPosition = Vector2.zero;
                containerRect.pivot = new Vector2(0.5f, 0.5f);
                containerRect.localRotation = Quaternion.Euler(0, 0, 90);
            }

            NewLoggingSystem.Info($"Configured container: Size={containerRect.sizeDelta}, Rotation={containerRect.localRotation.eulerAngles}, Scale={containerRect.localScale}", "S1Factory");
            NewLoggingSystem.Info($"✓ Container configured for portrait mode during creation", "S1Factory");
        }

        /// <summary>
        /// Configure the cloned app container for portrait mode
        /// This sets up the container transformation during app creation, not runtime
        /// </summary>
        private static void ConfigureContainerOrientation(GameObject clonedCanvas, string appName, bool isPortrait)
        {
            try
            {
                NewLoggingSystem.Info($"🔄 Configuring container for portrait mode during app creation: {appName}", "S1Factory");
                
                // Find the app container within the cloned canvas
                var container = FindAppContainer(clonedCanvas);
                if (container == null)
                {
                    NewLoggingSystem.Warning($"No container found in {appName} app canvas", "S1Factory");
                    return;
                }

                if(isPortrait) {
                    SetupPortraitContainer(container, appName);
                }
                
                // STEP 4: Fix background anchoring for portrait container
                // The background needs to properly fill the portrait container
                var background = container.Find("Background");
                if (background != null)
                {
                    var backgroundRect = background.GetComponent<RectTransform>();
                    if (backgroundRect != null)
                    {
                        NewLoggingSystem.Info($"Fixing background anchors for portrait container", "S1Factory");
                        
                        // Ensure background fills the entire portrait container
                        backgroundRect.anchorMin = Vector2.zero;
                        backgroundRect.anchorMax = Vector2.one;
                        backgroundRect.offsetMin = Vector2.zero;
                        backgroundRect.offsetMax = Vector2.zero;
                        backgroundRect.anchoredPosition = Vector2.zero;
                        
                        // Ensure background is behind everything else
                        background.SetAsFirstSibling();
                        
                        // Set a solid background color to ensure visibility
                        var backgroundImage = background.GetComponent<Image>();
                        if (backgroundImage != null)
                        {
                            backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark background
                            NewLoggingSystem.Info("✓ Background color set to dark", "S1Factory");
                        }
                    }
                }
                else
                {
                    NewLoggingSystem.Warning("Background element not found in container", "S1Factory");
                }
                
                // STEP 5: Setup BackSpeaker specific container content
                SetupBackSpeakerContainer(container, appName);
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to configure container for portrait mode: {ex}", "S1Factory");
            }
        }

        /// <summary>
        /// Clone app exactly like old code - simple and direct
        /// </summary>
        public static (IApp?, GameObject?) CloneApp(string appName, System.Action? onIconClick, Sprite appIcon, bool isPortrait = false)
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
                var productManagerAppObj = appsCanvas.transform.Find("ProductManagerApp");
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
                    // No need to hack orientation - we'll set it properly when opening the app
                }

                clonedCanvas.name = appName + "App";
                clonedCanvas.transform.localPosition = Vector3.zero;
                clonedCanvas.transform.localScale = Vector3.one;
                clonedCanvas.transform.localRotation = Quaternion.identity;
                clonedCanvas.SetActive(false);

                // CRITICAL: Set proper canvas sorting to prevent bleeding
                var canvasComponent = clonedCanvas.GetComponent<Canvas>();
                if (canvasComponent != null)
                {
                    canvasComponent.sortingOrder = 0; // Keep same level as other apps
                    canvasComponent.overrideSorting = false; // Don't override phone's sorting
                }

                NewLoggingSystem.Debug($"✓ Cloned ProductManagerApp as {clonedCanvas.name}", "S1Factory");
                
                
                // Modify the LAST existing icon (exactly like old code)
                ModifyLastIcon(appName, clonedCanvas, appIcon, onIconClick);
                
                // Configure container for portrait mode and setup BackSpeaker content
                ConfigureContainerOrientation(clonedCanvas, appName, isPortrait);
                
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
        private static void ModifyLastIcon(string appName, GameObject clonedCanvas, Sprite iconSprite, System.Action? onIconClick)
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
                var label = appIcon.transform.Find("Label").gameObject.GetComponent<Text>();
                if (label != null)
                {
                    NewLoggingSystem.Debug($"Setting label text to: {appName}", "S1Factory");
                    label.text = appName;
                }
                
                // Set BackSpeaker sprite
                if (iconSprite != null)
                {
                    var mask = appIcon.transform.Find("Mask").GetChild(0).GetComponent<Image>();
                    if (mask != null)
                    {
                        mask.sprite = iconSprite;
                    }
                }
                
                // Set button click handler
                var appButton = appIcon.GetComponent<Button>();
                if (appButton != null)
                {
                    // appButton.onClick.RemoveAllListeners();
                    var iconClickAction = ConvertToUnityAction(() => onIconClick?.Invoke());
                if (iconClickAction != null) appButton.onClick.AddListener(iconClickAction);
                }
                
                NewLoggingSystem.Debug("✓ Modified last icon for BackSpeaker", "S1Factory");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to modify last icon: {ex}", "S1Factory");
            }
        }
        
        /// <summary>
        /// Setup BackSpeaker specific container content
        /// Handles titlebar, removes ProductManager elements, configures background
        /// </summary>
        private static void SetupBackSpeakerContainer(Transform container, string appName)
        {
            try
            {
                NewLoggingSystem.Info("🔧 Setting up BackSpeaker container content", "S1Factory");
                
                // Update topbar title to "BackSpeaker"
                var topbar = container.Find("Topbar");
                if (topbar != null)
                {
                    var title = topbar.Find("Title");
                    if (title != null)
                    {
                        var titleText = title.GetComponent<Text>();
                        if (titleText != null)
                        {
                            titleText.text = appName;
                            NewLoggingSystem.Info("✓ Topbar title updated to 'BackSpeaker'", "S1Factory");
                        }
                        else
                        {
                            NewLoggingSystem.Warning("Title Text component not found", "S1Factory");
                        }
                    }
                    else
                    {
                        NewLoggingSystem.Warning("Title GameObject not found in Topbar", "S1Factory");
                    }
                }
                else
                {
                    NewLoggingSystem.Warning("Topbar GameObject not found", "S1Factory");
                }

                // Remove ProductManager specific elements
                var scrollView = container.Find("Scroll View");
                if (scrollView != null)
                {
                    scrollView.DetachChildren();
                    UnityEngine.Object.Destroy(scrollView.gameObject);
                    NewLoggingSystem.Info("✓ Removed Scroll View", "S1Factory");
                }

                var details = container.Find("Details");
                if (details != null)
                {
                    UnityEngine.Object.Destroy(details.gameObject);
                    NewLoggingSystem.Info("✓ Removed Details", "S1Factory");
                }

                // Background is already configured in ConfigureContainerForPortrait
                NewLoggingSystem.Info("✓ BackSpeaker container setup complete", "S1Factory");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error setting up BackSpeaker container: {ex}", "S1Factory");
            }
        }
        
        /// <summary>
        /// Add screen content to an app container
        /// This is called by BackSpeakerPhoneApp to provide the actual UI content
        /// </summary>
        public static void AddScreenToContainer(Transform container, GameObject screenContent)
        {
            try
            {
                NewLoggingSystem.Info("📱 Adding screen content to configured container", "S1Factory");
                
                if (container == null)
                {
                    NewLoggingSystem.Error("Container is null, cannot add screen content", "S1Factory");
                    return;
                }
                
                if (screenContent == null)
                {
                    NewLoggingSystem.Error("Screen content is null, cannot add to container", "S1Factory");
                    return;
                }
                
                // Set the screen content as a child of the container
                screenContent.transform.SetParent(container, false);
                
                // Ensure the screen content fills the container
                var screenRect = screenContent.GetComponent<RectTransform>();
                if (screenRect != null)
                {
                    screenRect.anchorMin = Vector2.zero;
                    screenRect.anchorMax = Vector2.one;
                    screenRect.offsetMin = Vector2.zero;
                    screenRect.offsetMax = Vector2.zero;
                    screenRect.anchoredPosition = Vector2.zero;
                }
                
                NewLoggingSystem.Info("✓ Screen content added to container successfully", "S1Factory");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error adding screen content to container: {ex}", "S1Factory");
            }
        }

        /// <summary>
        /// Find app container (like old code)
        /// </summary>
        private static Transform? FindAppContainer(GameObject appCanvas)
        {
            return appCanvas.transform.Find("Container");
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
                        // Cast to the expected IL2CPP type
                        var il2cppApp = component as Il2CppScheduleOne.UI.App<Il2CppScheduleOne.UI.Phone.ProductManagerApp.ProductManagerApp>;
                        if (il2cppApp != null)
                        {
                            return new Il2Cpp.Il2CppApp(il2cppApp);
                        }
                        else
                        {
                            NewLoggingSystem.Warning($"Component is not IL2CPP App type: {componentType.FullName}", "S1Factory");
                            return null;
                        }
#endif
#if !IL2CPP
                        // Cast to the expected Mono type
                        var monoApp = component as ScheduleOne.UI.App<ScheduleOne.UI.Phone.ProductManagerApp.ProductManagerApp>;
                        if (monoApp != null)
                        {
                            return new BackSpeakerMod.S1Wrapper.Mono.MonoApp(monoApp);
                        }
                        else
                        {
                            NewLoggingSystem.Warning($"Component is not Mono App type: {componentType.FullName}", "S1Factory");
                            return null;
                        }
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
        public static void RegisterType<T>() where T : UnityEngine.Object 
        {
            #if IL2CPP
                IL2CPPHelper.RegisterIl2CppType<T>();
            #endif
        }

        /// <summary>
        /// Convert System.Action to UnityAction safely for both IL2CPP and Mono
        /// </summary>
        public static UnityEngine.Events.UnityAction? ConvertToUnityAction(System.Action? action)
        {
            if (action == null) return null;

            try
            {
#if IL2CPP
                // For IL2CPP, direct cast works
                return (UnityEngine.Events.UnityAction)action;
#else
                // For Mono, need to create new UnityAction instance
                return new UnityEngine.Events.UnityAction(action);
#endif
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to convert Action to UnityAction: {ex}", "S1Factory");
                return null!;
            }
        }

        public static UnityEngine.Events.UnityAction<T> ConvertToUnityAction<T>(System.Action<T> action)
        {
            if (action == null) return null!;

            try
            {
                #if IL2CPP
                return (UnityEngine.Events.UnityAction<T>)action;
                #else
                return new UnityEngine.Events.UnityAction<T>(action);
                #endif
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to convert Action<T> to UnityAction<T>: {ex}", "S1Factory");
                return null!;
            }
        }

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
