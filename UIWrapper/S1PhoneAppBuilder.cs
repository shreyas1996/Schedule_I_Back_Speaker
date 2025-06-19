using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using BackSpeakerMod.S1Wrapper;

namespace BackSpeakerMod.UIWrapper
{
    /// <summary>
    /// Specialized builder for creating Schedule One phone apps
    /// Handles the Schedule One phone system integration
    /// </summary>
    public static class S1PhoneAppBuilder
    {
        private static readonly Dictionary<string, GameObject> _createdApps = new Dictionary<string, GameObject>();

        #region Phone App Creation

        /// <summary>
        /// Create a new phone app with comprehensive configuration
        /// </summary>
        public static AppCreationResult CreatePhoneApp(AppCreationConfig config)
        {
            try
            {
                var result = new AppCreationResult { Success = false };

                // Use S1Factory to get phone components
                var appsCanvas = GetAppsCanvas();
                var homeScreen = GetHomeScreen();
                var appIcons = GetAppIcons();

                if (appsCanvas == null || homeScreen == null || appIcons == null)
                {
                    result.ErrorMessage = "Required phone components not found";
                    return result;
                }

                // Check if app already exists
                if (GameObject.Find(config.AppName) != null)
                {
                    result.ErrorMessage = $"App '{config.AppName}' already exists";
                    return result;
                }

                // Find template app to clone from
                var templateApp = appsCanvas.transform.FindChild(config.TemplateAppName);
                if (templateApp == null)
                {
                    result.ErrorMessage = $"Template app '{config.TemplateAppName}' not found";
                    return result;
                }

                // Clone the template app
                var newAppCanvas = UnityEngine.Object.Instantiate(templateApp.gameObject, appsCanvas.transform);
                newAppCanvas.name = config.AppName;
                newAppCanvas.transform.localPosition = Vector3.zero;
                newAppCanvas.transform.localScale = Vector3.one;
                newAppCanvas.transform.localRotation = Quaternion.identity;
                newAppCanvas.SetActive(false);

                // Configure canvas properties
                var canvasComponent = newAppCanvas.GetComponent<Canvas>();
                if (canvasComponent != null)
                {
                    canvasComponent.sortingOrder = 0;
                    canvasComponent.overrideSorting = false;
                }

                // Create app icon
                var iconResult = CreateAppIcon(config, appIcons.gameObject);
                if (!iconResult.Success)
                {
                    UnityEngine.Object.DestroyImmediate(newAppCanvas);
                    return iconResult;
                }

                // Configure the app component
                ConfigureAppComponent(newAppCanvas, config);

                // Update the app UI
                UpdateAppUI(newAppCanvas, config);

                // Store created app for management
                _createdApps[config.AppName] = newAppCanvas;

                result.Success = true;
                result.CreatedApp = newAppCanvas;
                result.CreatedIcon = iconResult.CreatedIcon;

                return result;
            }
            catch (Exception ex)
            {
                return new AppCreationResult 
                { 
                    Success = false, 
                    ErrorMessage = $"Exception creating app: {ex.Message}" 
                };
            }
        }

        /// <summary>
        /// Quick BackSpeaker-style app creation
        /// </summary>
        public static GameObject? CreateBackSpeakerApp(string appName, string displayName, System.Action<Transform>? setupUI = null)
        {
            // Load the BackSpeaker logo
            var logo = BackSpeakerMod.NewBackend.Utils.ResourceLoader.LoadEmbeddedSprite("BackSpeakerMod.EmbeddedResources.back_speaker_logo.png");
            
            // Fallback to colored sprite if logo fails to load
            if (logo == null)
            {
                logo = S1UIFactory.CreateColoredSprite(new Color(0.2f, 0.6f, 1f, 1f), 64, 64);
            }

            var config = new AppCreationConfig
            {
                AppName = appName,
                DisplayName = displayName,
                IconSprite = logo,
                TitleColor = Color.white,
                BackgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f),
                ClearExistingContent = true,
                ReuseLastIcon = true,
                OnAppIconClick = () => OnBackSpeakerAppIconClick(appName)
            };

            var result = CreatePhoneApp(config);
            if (result.Success && result.CreatedApp != null && setupUI != null)
            {
                var container = result.CreatedApp.transform.FindChild("Container");
                if (container != null)
                {
                    setupUI(container);
                }
            }

            return result.Success ? result.CreatedApp : null;
        }

        private static void OnBackSpeakerAppIconClick(string appName)
        {
            // Close all other apps
            CloseAllAppsToHome();
            
            // Find and activate our app
            var appsCanvas = GetAppsCanvas();
            if (appsCanvas != null)
            {
                var ourApp = appsCanvas.FindChild(appName);
                if (ourApp != null)
                {
                    ourApp.gameObject.SetActive(true);
                }
            }
        }

        #endregion

        #region Phone Integration Utilities

        /// <summary>
        /// Get the phone apps canvas
        /// </summary>
        public static Transform? GetAppsCanvas() => GameObject.Find("AppsCanvas")?.transform;

        /// <summary>
        /// Get the phone home screen
        /// </summary>
        public static Transform? GetHomeScreen() => GameObject.Find("HomeScreen")?.transform;

        /// <summary>
        /// Get the app icons container
        /// </summary>
        public static Transform? GetAppIcons() => GameObject.Find("AppIcons")?.transform;

        /// <summary>
        /// Check if phone is available
        /// </summary>
        public static bool IsPhoneAvailable() => GetAppsCanvas() != null && GetHomeScreen() != null;

        /// <summary>
        /// Close all apps and return to home screen
        /// </summary>
        public static void CloseAllAppsToHome()
        {
            var appsCanvas = GetAppsCanvas();
            if (appsCanvas == null) return;

            // Deactivate all app canvases
            for (int i = 0; i < appsCanvas.childCount; i++)
            {
                var child = appsCanvas.GetChild(i);
                if (child.gameObject.activeInHierarchy)
                {
                    child.gameObject.SetActive(false);
                }
            }

            // Make sure home screen is active
            var homeScreen = GetHomeScreen();
            if (homeScreen != null)
            {
                homeScreen.gameObject.SetActive(true);
            }
        }

        #endregion

        #region Private Implementation

        private static AppCreationResult CreateAppIcon(AppCreationConfig config, GameObject appIcons)
        {
            var result = new AppCreationResult { Success = false };

            try
            {
                // Find an existing icon to use as template
                GameObject? iconTemplate = null;
                int iconCount = appIcons.transform.childCount;

                if (iconCount > 0)
                {
                    iconTemplate = appIcons.transform.GetChild(iconCount - 1).gameObject;
                }

                if (iconTemplate == null)
                {
                    result.ErrorMessage = "No icon template found";
                    return result;
                }

                // Clone or reuse the last icon
                GameObject appIcon;
                if (config.ReuseLastIcon && iconCount > 0)
                {
                    appIcon = iconTemplate;
                }
                else
                {
                    appIcon = UnityEngine.Object.Instantiate(iconTemplate, appIcons.transform);
                }

                // Update icon label
                var labelTransform = appIcon.transform.FindChild("Label");
                if (labelTransform != null)
                {
                    var labelText = labelTransform.GetComponent<Text>();
                    if (labelText != null)
                    {
                        labelText.text = config.DisplayName;
                    }
                }

                // Update icon sprite
                if (config.IconSprite != null)
                {
                    var maskTransform = appIcon.transform.FindChild("Mask");
                    if (maskTransform != null && maskTransform.childCount > 0)
                    {
                        var iconImage = maskTransform.GetChild(0).GetComponent<Image>();
                        if (iconImage != null)
                        {
                            iconImage.sprite = config.IconSprite;
                        }
                    }
                }

                // Configure button click event
                var button = appIcon.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener((UnityAction)(() => config.OnAppIconClick?.Invoke()));
                }

                result.Success = true;
                result.CreatedIcon = appIcon;
                return result;
            }
            catch (Exception ex)
            {
                result.ErrorMessage = $"Error creating icon: {ex.Message}";
                return result;
            }
        }

        private static void ConfigureAppComponent(GameObject appCanvas, AppCreationConfig config)
        {
            try
            {
                // Try to get the App component and configure it
                var appComponent = appCanvas.GetComponent<UnityEngine.Component>();
                if (appComponent != null)
                {
                    // Use reflection to set app properties
                    var appType = appComponent.GetType();
                    
                    var appNameProperty = appType.GetProperty("AppName");
                    if (appNameProperty != null && appNameProperty.CanWrite)
                    {
                        appNameProperty.SetValue(appComponent, config.DisplayName);
                    }

                    var iconLabelProperty = appType.GetProperty("IconLabel");
                    if (iconLabelProperty != null && iconLabelProperty.CanWrite)
                    {
                        iconLabelProperty.SetValue(appComponent, config.DisplayName);
                    }

                    if (config.IconSprite != null)
                    {
                        var appIconProperty = appType.GetProperty("AppIcon");
                        if (appIconProperty != null && appIconProperty.CanWrite)
                        {
                            appIconProperty.SetValue(appComponent, config.IconSprite);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently handle reflection failures
            }
        }

        private static void UpdateAppUI(GameObject appCanvas, AppCreationConfig config)
        {
            try
            {
                var container = appCanvas.transform.FindChild("Container");
                if (container == null) return;

                // Update topbar title
                var topbarTitle = container.FindChild("Topbar")?.FindChild("Title");
                if (topbarTitle != null)
                {
                    var titleText = topbarTitle.GetComponent<Text>();
                    if (titleText != null)
                    {
                        titleText.text = config.DisplayName;
                        titleText.color = config.TitleColor;
                    }
                }

                // Update background color
                var background = container.FindChild("Background");
                if (background != null)
                {
                    var backgroundImage = background.GetComponent<Image>();
                    if (backgroundImage != null)
                    {
                        backgroundImage.color = config.BackgroundColor;
                    }
                }

                // Clear existing content if specified
                if (config.ClearExistingContent)
                {
                    ClearAppContent(container);
                }
            }
            catch (Exception)
            {
                // Silently handle UI update failures
            }
        }

        private static void ClearAppContent(Transform container)
        {
            try
            {
                // Remove common content elements but keep structure
                var scrollView = container.FindChild("Scroll View");
                if (scrollView != null)
                {
                    scrollView.DetachChildren();
                    UnityEngine.Object.Destroy(scrollView.gameObject);
                }

                var details = container.FindChild("Details");
                if (details != null)
                {
                    UnityEngine.Object.Destroy(details.gameObject);
                }
            }
            catch (Exception)
            {
                // Silently handle cleanup failures
            }
        }

        #endregion

        #region Object Management

        /// <summary>
        /// Get a created app by name
        /// </summary>
        public static GameObject? GetCreatedApp(string name)
        {
            if (_createdApps.TryGetValue(name, out var app))
            {
                return app;
            }
            return null;
        }

        /// <summary>
        /// Destroy a created app
        /// </summary>
        public static bool DestroyCreatedApp(string name)
        {
            if (_createdApps.TryGetValue(name, out var app))
            {
                UnityEngine.Object.Destroy(app);
                _createdApps.Remove(name);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Get all created app names
        /// </summary>
        public static string[] GetCreatedAppNames() => new List<string>(_createdApps.Keys).ToArray();

        #endregion
    }

    #region Configuration Classes

    /// <summary>
    /// Configuration for creating new phone apps
    /// </summary>
    public class AppCreationConfig
    {
        public string AppName { get; set; } = "NewApp";
        public string DisplayName { get; set; } = "New App";
        public string TemplateAppName { get; set; } = "ProductManagerApp";
        public Sprite? IconSprite { get; set; }
        public Color TitleColor { get; set; } = Color.white;
        public Color BackgroundColor { get; set; } = new Color(0.1f, 0.1f, 0.1f, 1f);
        public bool ClearExistingContent { get; set; } = true;
        public bool ReuseLastIcon { get; set; } = true;
        public System.Action? OnAppIconClick { get; set; }
    }

    /// <summary>
    /// Result of app creation operations
    /// </summary>
    public class AppCreationResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public GameObject? CreatedApp { get; set; }
        public GameObject? CreatedIcon { get; set; }
    }

    #endregion
} 