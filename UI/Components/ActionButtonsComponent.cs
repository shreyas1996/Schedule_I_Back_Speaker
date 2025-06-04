using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Modules;
using System.Collections;
using MelonLoader;

namespace BackSpeakerMod.UI.Components
{
    /// <summary>
    /// Action buttons component following design specifications
    /// Layout: [Tab Action] [🎧 Detach Headphones] side by side
    /// </summary>
    public class ActionButtonsComponent : MonoBehaviour
    {
        private BackSpeakerManager? manager;
        private MusicSourceType currentTab = MusicSourceType.Jukebox;
        
        private Button? leftButton;
        private Button? rightButton;
        private Text? leftButtonText;
        
        public ActionButtonsComponent() : base() { }
        
        public void Setup(BackSpeakerManager manager)
        {
            this.manager = manager;
            CreateActionButtons();
        }
        
        private void CreateActionButtons()
        {
            // Left button container (48% width)
            var leftContainer = new GameObject("LeftButtonContainer");
            leftContainer.transform.SetParent(this.transform, false);
            
            var leftRect = leftContainer.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0.02f, 0.1f);
            leftRect.anchorMax = new Vector2(0.48f, 0.9f);
            leftRect.offsetMin = Vector2.zero;
            leftRect.offsetMax = Vector2.zero;
            
            // Right button container (48% width)
            var rightContainer = new GameObject("RightButtonContainer");
            rightContainer.transform.SetParent(this.transform, false);
            
            var rightRect = rightContainer.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(0.52f, 0.1f);
            rightRect.anchorMax = new Vector2(0.98f, 0.9f);
            rightRect.offsetMin = Vector2.zero;
            rightRect.offsetMax = Vector2.zero;
            
            // Create left button (tab-specific action)
            leftButton = CreateButton(leftContainer, "🔄 Reload Jukebox", new Color(0.2f, 0.7f, 0.2f, 0.8f), (UnityEngine.Events.UnityAction)delegate() { OnLeftButtonClick(); });
            leftButtonText = leftButton.GetComponentInChildren<Text>();
            
            // Create right button (headphone control)
            rightButton = CreateButton(rightContainer, "🎧 Detach Headphones", new Color(0.4f, 0.2f, 0.8f, 0.8f), (UnityEngine.Events.UnityAction)delegate() { OnHeadphoneToggle(); });
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
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
            LoggingSystem.Info("Switching to YouTube source", "UI");
            try
            {
                // Switch to YouTube source
                manager?.SetMusicSource(MusicSourceType.YouTube);
                
                LoggingSystem.Info("YouTube source activated", "UI");
                UpdateLeftButtonFeedback("📺 YouTube Ready", new Color(0.8f, 0.2f, 0.2f, 0.8f));
                // TODO: Implement YouTube search popup when ready
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to switch to YouTube: {ex.Message}", "UI");
                UpdateLeftButtonFeedback("❌ YouTube Error", new Color(0.8f, 0.2f, 0.2f, 0.8f));
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
                MusicSourceType.YouTube => ("🔍 YouTube Search & Cache", new Color(0.8f, 0.2f, 0.2f, 0.8f)),
                _ => ("🔄 Default Action", new Color(0.5f, 0.5f, 0.5f, 0.8f))
            };
            
            leftButtonText.text = text;
            leftButton!.GetComponent<Image>().color = color;
        }
        
        public void UpdateButtons()
        {
            // Update left button for current tab
            UpdateLeftButton();
            
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
    }
} 