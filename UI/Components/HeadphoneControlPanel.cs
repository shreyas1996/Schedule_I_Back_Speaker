using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.UI.Components
{
    /// <summary>
    /// Simple headphone control panel with minimal UI
    /// </summary>
    public class HeadphoneControlPanel : MonoBehaviour
    {
        private BackSpeakerManager? manager;
        private Button? toggleButton;
        private Text? statusText;
        
        // IL2CPP compatibility
        public HeadphoneControlPanel() : base() { }

        public void Setup(BackSpeakerManager manager, RectTransform container)
        {
            this.manager = manager;
            CreateSimpleUI(container);
        }

        private void CreateSimpleUI(RectTransform container)
        {
            // Create main panel - position it below volume control, but give more space for side-by-side layout
            var panel = new GameObject("HeadphonePanel").AddComponent<RectTransform>();
            panel.SetParent(container, false);
            panel.anchorMin = new Vector2(0f, 0f);
            panel.anchorMax = new Vector2(1f, 0f);
            panel.anchoredPosition = new Vector2(0f, 20f); // Same vertical position
            panel.sizeDelta = new Vector2(-20f, 80f); // Increased height for better layout
            
            // Background
            var bg = panel.gameObject.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Headphone toggle button - LEFT SIDE (takes left 60% of the panel)
            toggleButton = CreateButton(panel, "Toggle Headphones", new Vector2(0.05f, 0.5f), new Vector2(0.65f, 1f), (UnityEngine.Events.UnityAction)OnToggleClicked);
            
            // Status text - BOTTOM half of panel, spans full width
            statusText = CreateText(panel, "Headphones ready", new Vector2(0f, 0f), new Vector2(1f, 0.5f));
            
            // NOTE: Playlist button will be created in the RIGHT SIDE (35% to 95%) by PlaylistPanel.CreateToggleButton()
            // This provides space for the playlist button to be positioned at new Vector2(0.7f, 0.5f), new Vector2(0.95f, 1f)
        }

        private Button CreateButton(RectTransform parent, string text, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick)
        {
            var buttonObj = new GameObject($"{text}Button").AddComponent<RectTransform>();
            buttonObj.SetParent(parent, false);
            buttonObj.anchorMin = anchorMin;
            buttonObj.anchorMax = anchorMax;
            buttonObj.offsetMin = new Vector2(5f, 2f); // Reduced margins
            buttonObj.offsetMax = new Vector2(-5f, -2f);
            
            var button = buttonObj.gameObject.AddComponent<Button>();
            var image = buttonObj.gameObject.AddComponent<Image>();
            image.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Green color for headphone toggle
            
            var textObj = new GameObject("Text").AddComponent<RectTransform>();
            textObj.SetParent(buttonObj, false);
            textObj.anchorMin = Vector2.zero;
            textObj.anchorMax = Vector2.one;
            textObj.offsetMin = Vector2.zero;
            textObj.offsetMax = Vector2.zero;
            
            var textComponent = textObj.gameObject.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 12;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;
            
            button.onClick.AddListener((UnityEngine.Events.UnityAction)onClick);
            return button;
        }

        private Text CreateText(RectTransform parent, string text, Vector2 anchorMin, Vector2 anchorMax)
        {
            var textObj = new GameObject("StatusText").AddComponent<RectTransform>();
            textObj.SetParent(parent, false);
            textObj.anchorMin = anchorMin;
            textObj.anchorMax = anchorMax;
            textObj.offsetMin = new Vector2(5f, 2f); // Reduced margins
            textObj.offsetMax = new Vector2(-5f, -2f);
            
            var textComponent = textObj.gameObject.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 10;
            textComponent.color = Color.yellow;
            textComponent.alignment = TextAnchor.MiddleCenter;
            
            return textComponent;
        }

        private void OnToggleClicked()
        {
            if (manager != null)
            {
                bool wasAttached = manager.AreHeadphonesAttached();
                bool success = manager.ToggleHeadphones();
                
                if (success)
                {
                    // Update button color based on state
                    if (toggleButton != null)
                    {
                        var image = toggleButton.GetComponent<Image>();
                        bool nowAttached = manager.AreHeadphonesAttached();
                        image.color = nowAttached ? new Color(0.2f, 0.8f, 0.2f, 1f) : new Color(0.6f, 0.2f, 0.2f, 1f);
                        
                        // Update button text
                        var text = toggleButton.GetComponentInChildren<Text>();
                        text.text = nowAttached ? "Detach Headphones" : "Attach Headphones";
                    }
                }
                
                UpdateStatus();
            }
        }

        public void UpdateStatus()
        {
            if (manager == null || statusText == null) return;
            
            bool attached = manager.AreHeadphonesAttached();
            string status = attached ? "HEADPHONES ON" : "HEADPHONES OFF";
            
            statusText.text = status;
            statusText.color = attached ? Color.green : Color.gray;
            
            // Update button appearance based on current state
            if (toggleButton != null)
            {
                var image = toggleButton.GetComponent<Image>();
                image.color = attached ? new Color(0.2f, 0.8f, 0.2f, 1f) : new Color(0.6f, 0.2f, 0.2f, 1f);
                
                var text = toggleButton.GetComponentInChildren<Text>();
                text.text = attached ? "Detach Headphones" : "Attach Headphones";
            }
        }

        private void Update()
        {
            UpdateStatus();
        }
    }
} 