using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Features.Spheres.Managers;
using BackSpeakerMod.Configuration;
using UnityEngine.Events;

namespace BackSpeakerMod.UI.Components
{
    /// <summary>
    /// Sphere control panel integrated into BackSpeaker app UI
    /// Replaces the headphone control panel
    /// </summary>
    public class SphereControlPanel : MonoBehaviour
    {
        private BackSpeakerManager? manager;
        private SphereManager? sphereManager;
        private Button? toggleButton;
        private Button? placementButton;
        private Button? detachButton;
        private Text? statusText;
        private Toggle? autoAttachToggle;
        private Toggle? glowToggle;
        private Toggle? rotationToggle;
        
        // IL2CPP compatibility
        public SphereControlPanel() : base() { }

        public void Setup(BackSpeakerManager manager, RectTransform container)
        {
            this.manager = manager;
            this.sphereManager = manager?.GetSphereManager(); // Get sphere manager from main manager
            CreateSphereUI(container);
        }

        private void CreateSphereUI(RectTransform container)
        {
            // Create main panel - same layout as HeadphoneControlPanel
            var panel = new GameObject("SpherePanel").AddComponent<RectTransform>();
            panel.SetParent(container, false);
            panel.anchorMin = new Vector2(0f, 0f);
            panel.anchorMax = new Vector2(1f, 0f);
            panel.anchoredPosition = new Vector2(0f, 50f);
            panel.sizeDelta = new Vector2(-20f, 120f); // Slightly taller for more controls
            
            // Background with sphere theme
            var bg = panel.gameObject.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.1f, 0.2f, 0.8f); // Darker purple tint for spheres
            
            // Main control buttons (top row)
            toggleButton = CreateButton(panel, "Attach Sphere", new Vector2(0f, 0.6f), new Vector2(0.33f, 1f), OnToggleClicked);
            placementButton = CreateButton(panel, "Placement", new Vector2(0.33f, 0.6f), new Vector2(0.66f, 1f), OnPlacementClicked);
            detachButton = CreateButton(panel, "Detach", new Vector2(0.66f, 0.6f), new Vector2(1f, 1f), OnDetachClicked);
            
            // Settings toggles (middle row)
            autoAttachToggle = CreateToggle(panel, "Auto-attach", new Vector2(0f, 0.3f), new Vector2(0.33f, 0.6f), FeatureFlags.Spheres.AutoAttachOnSpawn);
            glowToggle = CreateToggle(panel, "Glow Effect", new Vector2(0.33f, 0.3f), new Vector2(0.66f, 0.6f), FeatureFlags.Spheres.EnableGlowEffect);
            rotationToggle = CreateToggle(panel, "Rotation", new Vector2(0.66f, 0.3f), new Vector2(1f, 0.6f), FeatureFlags.Spheres.EnableRotation);
            
            // Status text (bottom)
            statusText = CreateText(panel, "Sphere system ready", new Vector2(0f, 0f), new Vector2(1f, 0.3f));
            statusText.color = new Color(0.7f, 0.9f, 1f, 1f); // Light blue for sphere theme
            
            // Set up toggle callbacks
            autoAttachToggle.onValueChanged.AddListener((UnityEngine.Events.UnityAction<bool>)((value) => { FeatureFlags.Spheres.AutoAttachOnSpawn = value; }));
            glowToggle.onValueChanged.AddListener((UnityEngine.Events.UnityAction<bool>)((value) => { FeatureFlags.Spheres.EnableGlowEffect = value; }));
            rotationToggle.onValueChanged.AddListener((UnityEngine.Events.UnityAction<bool>)((value) => { FeatureFlags.Spheres.EnableRotation = value; }));
        }

        private Button CreateButton(RectTransform parent, string text, Vector2 anchorMin, Vector2 anchorMax, System.Action onClick)
        {
            var buttonObj = new GameObject($"Button_{text}");
            buttonObj.transform.SetParent(parent, false);
            
            var rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = new Vector2(2f, 2f);
            rect.offsetMax = new Vector2(-2f, -2f);
            
            var button = buttonObj.AddComponent<Button>();
            var image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.3f, 0.2f, 0.4f, 0.9f); // Purple theme for spheres
            button.targetGraphic = image;
            
            // Button text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.font = UnityEngine.Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComponent.fontSize = 12;
            textComponent.color = Color.white;
            
            button.onClick.AddListener((UnityEngine.Events.UnityAction)onClick);
            
            return button;
        }

        private Toggle CreateToggle(RectTransform parent, string text, Vector2 anchorMin, Vector2 anchorMax, bool defaultValue)
        {
            var toggleObj = new GameObject($"Toggle_{text}");
            toggleObj.transform.SetParent(parent, false);
            
            var rect = toggleObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = new Vector2(2f, 2f);
            rect.offsetMax = new Vector2(-2f, -2f);
            
            var toggle = toggleObj.AddComponent<Toggle>();
            toggle.isOn = defaultValue;
            
            // Toggle background
            var bg = toggleObj.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.2f, 0.3f, 0.8f);
            
            // Create checkmark area
            var checkmarkObj = new GameObject("Checkmark");
            checkmarkObj.transform.SetParent(toggleObj.transform, false);
            var checkRect = checkmarkObj.AddComponent<RectTransform>();
            checkRect.anchorMin = new Vector2(0f, 0.5f);
            checkRect.anchorMax = new Vector2(0.3f, 0.5f);
            checkRect.anchoredPosition = Vector2.zero;
            checkRect.sizeDelta = new Vector2(16f, 16f);
            
            var checkImage = checkmarkObj.AddComponent<Image>();
            checkImage.color = new Color(0.7f, 0.9f, 1f, 1f);
            
            // Toggle text label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(toggleObj.transform, false);
            var labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.3f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            
            var labelText = labelObj.AddComponent<Text>();
            labelText.text = text;
            labelText.alignment = TextAnchor.MiddleLeft;
            labelText.font = UnityEngine.Resources.GetBuiltinResource<Font>("Arial.ttf");
            labelText.fontSize = 10;
            labelText.color = Color.white;
            
            toggle.targetGraphic = checkImage;
            toggle.graphic = checkImage;
            
            return toggle;
        }

        private Text CreateText(RectTransform parent, string text, Vector2 anchorMin, Vector2 anchorMax)
        {
            var textObj = new GameObject("StatusText");
            textObj.transform.SetParent(parent, false);
            
            var rect = textObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = new Vector2(5f, 2f);
            rect.offsetMax = new Vector2(-5f, -2f);
            
            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.font = UnityEngine.Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComponent.fontSize = 11;
            textComponent.color = Color.white;
            
            return textComponent;
        }

        // Button handlers
        private void OnToggleClicked()
        {
            if (sphereManager == null)
            {
                LoggingSystem.Warning("SphereManager not available", "SphereUI");
                return;
            }
            
            sphereManager.ToggleSphereAttachment();
            LoggingSystem.Debug("Sphere toggle clicked", "SphereUI");
        }

        private void OnPlacementClicked()
        {
            if (sphereManager == null)
            {
                LoggingSystem.Warning("SphereManager not available", "SphereUI");
                return;
            }
            
            sphereManager.StartSphereePlacement();
            LoggingSystem.Debug("Sphere placement clicked", "SphereUI");
        }

        private void OnDetachClicked()
        {
            if (sphereManager == null)
            {
                LoggingSystem.Warning("SphereManager not available", "SphereUI");
                return;
            }
            
            sphereManager.DetachSphere();
            LoggingSystem.Debug("Sphere detach clicked", "SphereUI");
        }

        public void UpdateStatus()
        {
            if (statusText == null || sphereManager == null) return;
            
            try
            {
                string status = sphereManager.GetStatus();
                statusText.text = status;
                
                // Update button states based on sphere state
                bool isAttached = sphereManager.IsAttached;
                if (toggleButton != null)
                {
                    var buttonText = toggleButton.GetComponentInChildren<Text>();
                    if (buttonText != null)
                    {
                        buttonText.text = isAttached ? "Toggle Off" : "Attach Sphere";
                    }
                }
                
                if (detachButton != null)
                {
                    detachButton.interactable = isAttached;
                }
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Error updating sphere status: {ex.Message}", "SphereUI");
            }
        }
    }
} 