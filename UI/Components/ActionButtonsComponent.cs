using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Modules;

namespace BackSpeakerMod.UI.Components
{
    /// <summary>
    /// Action buttons component following design specifications
    /// Layout: [Tab Action] [🎧 Detach Headphones] side by side
    /// </summary>
    public class ActionButtonsComponent : MonoBehaviour
    {
        private BackSpeakerManager? manager;
        private TrackLoader? trackLoader;
        private MusicSourceType currentTab = MusicSourceType.Jukebox;
        
        private Button? leftButton;
        private Button? rightButton;
        private Text? leftButtonText;
        
        public ActionButtonsComponent() : base() { }
        
        public void Setup(BackSpeakerManager manager, TrackLoader trackLoader)
        {
            this.manager = manager;
            this.trackLoader = trackLoader;
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
            trackLoader?.InitializeExternalProviders(this.gameObject);
        }
        
        private void RefreshLocalMusic()
        {
            LoggingSystem.Info("Refreshing local music...", "UI");
            trackLoader?.InitializeExternalProviders(this.gameObject);
        }
        
        private void OpenYouTubeSearch()
        {
            LoggingSystem.Info("YouTube search requested (placeholder)", "UI");
            // TODO: Implement YouTube search popup
        }
        
        private void OnHeadphoneToggle()
        {
            LoggingSystem.Info("Headphone toggle requested", "UI");
            // TODO: Integrate with existing headphone system
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
            // Update button states if needed
        }
    }
} 