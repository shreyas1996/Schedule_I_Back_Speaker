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
        private Button? placementButton;
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
            // Create main panel
            var panel = new GameObject("HeadphonePanel").AddComponent<RectTransform>();
            panel.SetParent(container, false);
            panel.anchorMin = new Vector2(0f, 0f);
            panel.anchorMax = new Vector2(1f, 0f);
            panel.anchoredPosition = new Vector2(0f, 50f);
            panel.sizeDelta = new Vector2(-20f, 100f);
            
            // Background
            var bg = panel.gameObject.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Toggle button
            toggleButton = CreateButton(panel, "Toggle Headphones", new Vector2(0f, 0.5f), new Vector2(0.5f, 1f), (UnityEngine.Events.UnityAction)OnToggleClicked);
            
            // Placement button
            placementButton = CreateButton(panel, "Placement Mode", new Vector2(0.5f, 0.5f), new Vector2(1f, 1f), (UnityEngine.Events.UnityAction)OnPlacementClicked);
            
            // Status text
            statusText = CreateText(panel, "Headphones ready", new Vector2(0f, 0f), new Vector2(1f, 0.5f));
        }

        private Button CreateButton(RectTransform parent, string text, Vector2 anchorMin, Vector2 anchorMax, UnityEngine.Events.UnityAction onClick)
        {
            var buttonObj = new GameObject($"{text}Button").AddComponent<RectTransform>();
            buttonObj.SetParent(parent, false);
            buttonObj.anchorMin = anchorMin;
            buttonObj.anchorMax = anchorMax;
            buttonObj.offsetMin = new Vector2(5f, 5f);
            buttonObj.offsetMax = new Vector2(-5f, -5f);
            
            var button = buttonObj.gameObject.AddComponent<Button>();
            var image = buttonObj.gameObject.AddComponent<Image>();
            image.color = new Color(0.2f, 0.4f, 0.8f, 1f);
            
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
            textObj.offsetMin = new Vector2(5f, 5f);
            textObj.offsetMax = new Vector2(-5f, -5f);
            
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
                manager.ToggleHeadphones();
                UpdateStatus();
            }
        }

        private void OnPlacementClicked()
        {
            if (manager != null)
            {
                manager.TogglePlacementMode();
                UpdateStatus();
            }
        }

        public void UpdateStatus()
        {
            if (manager == null || statusText == null) return;
            
            string status = manager.GetHeadphoneStatus();
            bool attached = manager.AreHeadphonesAttached();
            bool placement = manager.IsInPlacementMode();
            
            statusText.text = $"{status} | {(attached ? "ON" : "OFF")} | {(placement ? "PLACING" : "NORMAL")}";
        }

        private void Update()
        {
            UpdateStatus();
        }
    }
} 