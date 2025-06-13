using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Modules;
using BackSpeakerMod.UI.Components;
using System.Collections;
using MelonLoader;
using BackSpeakerMod.UI.Helpers;

namespace BackSpeakerMod.UI.Components
{
    /// <summary>
    /// Action buttons component following design specifications
    /// Layout: [Tab Action] [📁 Manage Directories] [🎧 Detach Headphones] side by side
    /// </summary>
    public class ActionButtonsComponent : MonoBehaviour
    {
        private BackSpeakerManager? manager;
        private MusicSourceType currentTab = MusicSourceType.Jukebox;
        
        private Button? leftButton;
        private Button? middleButton;
        private Button? rightButton;
        private Text? leftButtonText;
        
        // Container references for dynamic button management
        private GameObject? middleContainer;
        
        // Music directory manager
        private MusicDirectoryManagerComponent? directoryManager;
        
        public ActionButtonsComponent() : base() { }
        
        public void Setup(BackSpeakerManager manager)
        {
            this.manager = manager;
            CreateActionButtons();
            CreateYouTubeSearchPopup();
            // Note: CreateMusicDirectoryManager() is now called lazily when needed
            UpdateButtons(); // Initialize button states
        }
        
        private void CreateYouTubeSearchPopup()
        {
            // This method is placeholder and should be integrated elsewhere
            // The actual popup creation is handled in ShowYouTubeSearchPopup
        }
        
        private void CreateActionButtons()
        {
            // Left button container (40% width)
            var leftContainer = new GameObject("LeftButtonContainer");
            leftContainer.transform.SetParent(this.transform, false);
            
            var leftRect = leftContainer.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0.02f, 0.1f);
            leftRect.anchorMax = new Vector2(0.40f, 0.9f);
            leftRect.offsetMin = Vector2.zero;
            leftRect.offsetMax = Vector2.zero;
            
            // Middle button container (28% width) - will be shown/hidden based on tab
            middleContainer = new GameObject("MiddleButtonContainer");
            middleContainer.transform.SetParent(this.transform, false);
            
            var middleRect = middleContainer.AddComponent<RectTransform>();
            middleRect.anchorMin = new Vector2(0.42f, 0.1f);
            middleRect.anchorMax = new Vector2(0.68f, 0.9f);
            middleRect.offsetMin = Vector2.zero;
            middleRect.offsetMax = Vector2.zero;
            
            // Right button container (28% width)
            var rightContainer = new GameObject("RightButtonContainer");
            rightContainer.transform.SetParent(this.transform, false);
            
            var rightRect = rightContainer.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.70f, 0.1f);
            rightRect.anchorMax = new Vector2(0.98f, 0.9f);
            rightRect.offsetMin = Vector2.zero;
            rightRect.offsetMax = Vector2.zero;
            
            // Create left button (tab-specific action)
            leftButton = CreateButton(leftContainer, "🔄 Reload Jukebox", new Color(0.2f, 0.7f, 0.2f, 0.8f), (UnityEngine.Events.UnityAction)OnLeftButtonClick);
            leftButtonText = leftButton.GetComponentInChildren<Text>();
            
            // Create middle button (manage directories) - will be shown/hidden based on tab
            middleButton = CreateButton(middleContainer, "📁 Manage Local Directories", new Color(0.6f, 0.4f, 0.2f, 0.8f), (UnityEngine.Events.UnityAction)OnDirectoryManagerClick);
            
            // Create right button (headphone control)
            rightButton = CreateButton(rightContainer, "🎧 Detach Headphones", new Color(0.4f, 0.2f, 0.8f, 0.8f), (UnityEngine.Events.UnityAction)OnHeadphoneToggle);
        }
        
        private Button CreateButton(GameObject container, string text, Color color, UnityEngine.Events.UnityAction action)
        {
            var buttonObj = new GameObject("Button");
            buttonObj.transform.SetParent(container.transform, false);
            
            var buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = Vector2.zero;
            buttonRect.anchorMax = Vector2.one;
            buttonRect.offsetMin = new Vector2(5f, 5f);
            buttonRect.offsetMax = new Vector2(-5f, -5f);
            
            var button = buttonObj.AddComponent<Button>();
            var buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = color;
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5f, 0f);
            textRect.offsetMax = new Vector2(-5f, 0f);
            
            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            FontHelper.SetSafeFont(textComponent);
            textComponent.fontSize = 12;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.fontStyle = FontStyle.Bold;
            
            button.targetGraphic = buttonImage;
            button.onClick.AddListener(action);
            
            return button;
        }
        
        private void OnLeftButtonClick()
        {
            LoggingSystem.Info($"Left button clicked for tab: {currentTab}", "UI");
            
            switch (currentTab)
            {
                case MusicSourceType.Jukebox:
                    ReloadJukebox();
                    break;
                case MusicSourceType.LocalFolder:
                    RefreshLocalMusic();
                    break;
                case MusicSourceType.YouTube:
                    OpenYouTubeSearch();
                    break;
                default:
                    LoggingSystem.Warning($"Unknown tab type: {currentTab}", "UI");
                    break;
            }
        }
        
        private void ReloadJukebox()
        {
            LoggingSystem.Info("Reloading jukebox tracks...", "UI");
            try
            {
                // Check if headphones are attached first
                bool headphonesAttached = manager?.AreHeadphonesAttached() ?? false;
                bool audioReady = manager?.IsAudioReady() ?? false;
                
                LoggingSystem.Info($"System status - Headphones: {headphonesAttached}, Audio Ready: {audioReady}", "UI");
                
                if (!headphonesAttached)
                {
                    LoggingSystem.Warning("Headphones not attached - cannot load tracks", "UI");
                    UpdateLeftButtonFeedback("🎧 Attach headphones first", new Color(0.8f, 0.4f, 0.2f, 0.8f));
                    return;
                }
                
                if (!audioReady)
                {
                    LoggingSystem.Warning("Audio system not ready", "UI");
                    UpdateLeftButtonFeedback("⚠️ Audio not ready", new Color(0.8f, 0.4f, 0.2f, 0.8f));
                    return;
                }
                
                // First set the music source to jukebox
                manager?.SetMusicSource(MusicSourceType.Jukebox);
                
                // Then reload tracks through the manager
                manager?.ReloadTracks();
                
                LoggingSystem.Info("Jukebox reload initiated", "UI");
                UpdateLeftButtonFeedback("🔄 Reloading...", new Color(0.8f, 0.8f, 0.2f, 0.8f));
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to reload jukebox: {ex.Message}", "UI");
                UpdateLeftButtonFeedback("❌ Failed to reload", new Color(0.8f, 0.2f, 0.2f, 0.8f));
            }
        }
        
        private void RefreshLocalMusic()
        {
            LoggingSystem.Info("Refreshing local music...", "UI");
            try
            {
                // Check system status
                bool headphonesAttached = manager?.AreHeadphonesAttached() ?? false;
                
                if (!headphonesAttached)
                {
                    LoggingSystem.Warning("Headphones not attached - cannot load local music", "UI");
                    UpdateLeftButtonFeedback("🎧 Attach headphones first", new Color(0.8f, 0.4f, 0.2f, 0.8f));
                    return;
                }
                
                // Force load from LocalFolder source (this will create folder if needed and switch source)
                manager?.ForceLoadFromSource(MusicSourceType.LocalFolder);
                
                LoggingSystem.Info("Local music refresh initiated", "UI");
                UpdateLeftButtonFeedback("🔄 Refreshing...", new Color(0.8f, 0.8f, 0.2f, 0.8f));
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to refresh local music: {ex.Message}", "UI");
                UpdateLeftButtonFeedback("❌ Failed to refresh", new Color(0.8f, 0.2f, 0.2f, 0.8f));
            }
        }
        
        private void OpenYouTubeSearch()
        {
            LoggingSystem.Info("Opening YouTube actions", "UI");
            try
            {
                // Check if headphones are attached first
                bool headphonesAttached = manager?.AreHeadphonesAttached() ?? false;
                
                if (!headphonesAttached)
                {
                    LoggingSystem.Warning("Headphones not attached - cannot use YouTube", "UI");
                    UpdateLeftButtonFeedback("🎧 Attach headphones first", new Color(0.8f, 0.4f, 0.2f, 0.8f));
                    return;
                }

                // Switch to YouTube source first
                manager?.SetMusicSource(MusicSourceType.YouTube);
                
                // Force load cached tracks from YouTube source to refresh playlist
                manager?.ForceLoadFromSource(MusicSourceType.YouTube);
                
                LoggingSystem.Info("YouTube source activated and cached tracks refreshed", "UI");
                UpdateLeftButtonFeedback("📺 YouTube Ready", new Color(0.8f, 0.2f, 0.2f, 0.8f));
                
                // Show the search popup for adding new songs
                ShowYouTubeSearchPopup();
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to open YouTube actions: {ex.Message}", "UI");
                UpdateLeftButtonFeedback("❌ YouTube Error", new Color(0.8f, 0.2f, 0.2f, 0.8f));
            }
        }

        private void ShowYouTubeSearchPopup()
        {
            LoggingSystem.Info("Showing YouTube search popup", "UI");
            try
            {
                // Find our app's container using the same approach as PlaylistToggleComponent
                Transform? appContainer = null;
                Transform current = this.transform;
                
                // Walk up the hierarchy to find "Container" (our app's container)
                while (current != null && appContainer == null)
                {
                    if (current.name == "Container")
                    {
                        appContainer = current;
                        LoggingSystem.Debug("Found app Container by walking up hierarchy", "UI");
                        break;
                    }
                    current = current.parent;
                }
                
                // If no container found, try to find BackSpeakerApp canvas
                if (appContainer == null)
                {
                    current = this.transform;
                    while (current != null)
                    {
                        if (current.name == "BackSpeakerApp")
                        {
                            // Look for Container child
                            var containerChild = current.Find("Container");
                            if (containerChild != null)
                            {
                                appContainer = containerChild;
                                LoggingSystem.Debug("Found Container as child of BackSpeakerApp", "UI");
                                break;
                            }
                        }
                        current = current.parent;
                    }
                }
                
                if (appContainer == null)
                {
                    LoggingSystem.Error("No app Container found for YouTube popup! This will cause UI issues.", "UI");
                    return;
                }
                
                LoggingSystem.Info($"Found app Container: {appContainer.name}", "UI");

                var popupContainer = new GameObject("YouTubeSearchPopupComponent");
                popupContainer.transform.SetParent(appContainer, false);
                
                // Make sure the popup appears on top by setting it as last sibling
                popupContainer.transform.SetAsLastSibling();

                var popupComponent = popupContainer.AddComponent<YouTubePopupComponent>();
                if (manager != null)
                {
                    popupComponent.Setup(manager);
                    popupComponent.OpenYouTubeSearchPopup();
                    
                    LoggingSystem.Info("YouTube search popup shown successfully", "UI");
                }
                else
                {
                    LoggingSystem.Error("Manager is null - cannot setup YouTube popup", "UI");
                    UpdateLeftButtonFeedback("❌ Manager Error", new Color(0.8f, 0.2f, 0.2f, 0.8f));
                }
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to show YouTube popup: {ex.Message}", "UI");
                UpdateLeftButtonFeedback("❌ Popup Error", new Color(0.8f, 0.2f, 0.2f, 0.8f));
            }
        }
        
        private void OnHeadphoneToggle()
        {
            LoggingSystem.Info("Headphone toggle requested", "UI");
            try
            {
                bool success = manager?.ToggleHeadphones() ?? false;
                string status = manager?.GetHeadphoneStatus() ?? "Unknown";
                
                LoggingSystem.Info($"Headphone toggle result: {success}, Status: {status}", "UI");
                
                // Update right button text immediately to show new state
                UpdateButtons();
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to toggle headphones: {ex.Message}", "UI");
            }
        }
        
        private void OnDirectoryManagerClick()
        {
            LoggingSystem.Info("Music directory manager button clicked", "UI");
            try
            {
                if (directoryManager != null)
                {
                    directoryManager.ShowPopup();
                }
                else
                {
                    LoggingSystem.Warning("Directory manager not initialized, attempting to create it now", "UI");
                    CreateMusicDirectoryManager();
                    
                    // Try again after creation
                    if (directoryManager != null)
                    {
                        directoryManager.ShowPopup();
                    }
                    else
                    {
                        LoggingSystem.Error("Failed to create directory manager", "UI");
                    }
                }
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to show directory manager: {ex.Message}", "UI");
            }
        }
        
        private void CreateMusicDirectoryManager()
        {
            try
            {
                // Find our app's container using the same approach as other popups
                Transform? appContainer = null;
                Transform current = this.transform;
                
                // Walk up the hierarchy to find "Container" (our app's container)
                while (current != null && appContainer == null)
                {
                    if (current.name == "Container")
                    {
                        appContainer = current;
                        LoggingSystem.Debug("Found app Container for directory manager", "UI");
                        break;
                    }
                    current = current.parent;
                }
                
                // If no container found, try to find BackSpeakerApp canvas
                if (appContainer == null)
                {
                    current = this.transform;
                    while (current != null)
                    {
                        if (current.name == "BackSpeakerApp")
                        {
                            // Look for Container child
                            var containerChild = current.Find("Container");
                            if (containerChild != null)
                            {
                                appContainer = containerChild;
                                LoggingSystem.Debug("Found Container as child of BackSpeakerApp", "UI");
                                break;
                            }
                        }
                        current = current.parent;
                    }
                }
                
                if (appContainer == null)
                {
                    LoggingSystem.Error("No app Container found for directory manager! Using current transform.", "UI");
                    appContainer = this.transform;
                }
                
                LoggingSystem.Info($"Creating directory manager in container: {appContainer.name}", "UI");

                var managerContainer = new GameObject("MusicDirectoryManagerComponent");
                managerContainer.transform.SetParent(appContainer, false);
                
                // Make sure the manager appears on top by setting it as last sibling
                managerContainer.transform.SetAsLastSibling();

                directoryManager = managerContainer.AddComponent<MusicDirectoryManagerComponent>();
                directoryManager.Initialize(() => {
                    // Callback when directories change - refresh local music if needed
                    if (currentTab == MusicSourceType.LocalFolder && manager != null)
                    {
                        LoggingSystem.Info("Directories changed, refreshing local music", "UI");
                        manager.ForceLoadFromSource(MusicSourceType.LocalFolder);
                    }
                });
                
                LoggingSystem.Info("Music directory manager created successfully", "UI");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to create music directory manager: {ex.Message}", "UI");
            }
        }
        
        private void UpdateLeftButtonFeedback(string text, Color color)
        {
            if (leftButtonText != null && leftButton != null)
            {
                leftButtonText.text = text;
                leftButton.GetComponent<Image>().color = color;
                
                // Reset to normal after 2 seconds using MonoBehaviour.StartCoroutine
                MelonCoroutines.Start(ResetLeftButtonAfterDelay(2f));
            }
        }
        
        private System.Collections.IEnumerator ResetLeftButtonAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            UpdateLeftButton(); // Reset to normal state
        }
        
        public void UpdateForTab(MusicSourceType newTab)
        {
            currentTab = newTab;
            UpdateLeftButton();
        }
        
        private void UpdateLeftButton()
        {
            if (leftButtonText == null) return;
            
            var (text, color) = currentTab switch
            {
                MusicSourceType.Jukebox => ("🔄 Reload Jukebox", new Color(0.2f, 0.7f, 0.2f, 0.8f)),
                MusicSourceType.LocalFolder => ("🔄 Refresh Local Music", new Color(0.2f, 0.4f, 0.8f, 0.8f)),
                MusicSourceType.YouTube => ("🔄 Refresh & Search YouTube", new Color(0.8f, 0.2f, 0.2f, 0.8f)),
                _ => ("🔄 Default Action", new Color(0.5f, 0.5f, 0.5f, 0.8f))
            };
            
            leftButtonText.text = text;
            leftButton!.GetComponent<Image>().color = color;
        }
        
        public void UpdateButtons()
        {
            // Update left button for current tab
            UpdateLeftButton();
            
            // Update middle button visibility based on current tab
            UpdateMiddleButton();
            
            // Update right button for headphone status
            if (rightButton != null && manager != null)
            {
                var rightButtonText = rightButton.GetComponentInChildren<Text>();
                var rightButtonImage = rightButton.GetComponent<Image>();
                
                try
                {
                    bool headphonesAttached = manager.AreHeadphonesAttached();
                    string status = manager.GetHeadphoneStatus();
                    
                    if (headphonesAttached)
                    {
                        rightButtonText.text = "🎧 Detach Headphones";
                        rightButtonImage.color = new Color(0.8f, 0.2f, 0.2f, 0.8f); // Red for detach
                    }
                    else
                    {
                        rightButtonText.text = "🎧 Attach Headphones";
                        rightButtonImage.color = new Color(0.2f, 0.7f, 0.2f, 0.8f); // Green for attach
                    }
                    
                    // LoggingSystem.Debug($"Headphone button updated - Attached: {headphonesAttached}, Status: {status}", "UI");
                }
                catch (System.Exception ex)
                {
                    LoggingSystem.Error($"Failed to update headphone button: {ex.Message}", "UI");
                    rightButtonText.text = "🎧 Headphone Error";
                    rightButtonImage.color = new Color(0.5f, 0.5f, 0.5f, 0.8f);
                }
            }
        }
        
        private void UpdateMiddleButton()
        {
            if (middleContainer != null)
            {
                // Show middle button only for LocalFolder tab
                bool shouldShow = currentTab == MusicSourceType.LocalFolder;
                middleContainer.SetActive(shouldShow);
                
                // LoggingSystem.Debug($"Middle button (directory manager) visibility: {shouldShow} for tab: {currentTab}", "UI");
            }
        }
    }
} 