using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.UI.Components
{
    public class HeadphoneControlPanel : MonoBehaviour
    {
        private BackSpeakerManager? manager;
        private RectTransform? container;
        
        // UI elements
        private Button? toggleButton;
        private Button? testCubeButton;
        private Text? statusText;
        private Text? toggleButtonText;
        private Text? testCubeButtonText;
        
        // IL2CPP compatibility
        public HeadphoneControlPanel() : base() { }

        public void Setup(BackSpeakerManager manager, RectTransform container)
        {
            this.manager = manager;
            this.container = container;
            CreateHeadphoneControls();
            // LoggerUtil.Info("HeadphoneControlPanel: Setup completed");
        }

        private void CreateHeadphoneControls()
        {
            // Create main panel container
            var panelObj = new GameObject("HeadphonePanel");
            panelObj.transform.SetParent(container, false);
            
            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(1f, 0f);
            panelRect.pivot = new Vector2(0.5f, 0f);
            panelRect.anchoredPosition = new Vector2(0f, 50f); // Position above playlist button
            panelRect.sizeDelta = new Vector2(-20f, 120f); // Width fits container, increased height for test button
            
            // Add background
            var panelImage = panelObj.AddComponent<Image>();
            panelImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            // Create toggle button (top row)
            var toggleButtonObj = new GameObject("ToggleHeadphonesButton");
            toggleButtonObj.transform.SetParent(panelObj.transform, false);
            
            var toggleButtonRect = toggleButtonObj.AddComponent<RectTransform>();
            toggleButtonRect.anchorMin = new Vector2(0f, 0.67f);
            toggleButtonRect.anchorMax = new Vector2(0.6f, 1f);
            toggleButtonRect.offsetMin = new Vector2(10f, 5f);
            toggleButtonRect.offsetMax = new Vector2(-5f, -5f);
            
            toggleButton = toggleButtonObj.AddComponent<Button>();
            var toggleButtonImage = toggleButtonObj.AddComponent<Image>();
            toggleButtonImage.color = new Color(0.2f, 0.4f, 0.8f, 1f);
            
            // Add button text
            var toggleTextObj = new GameObject("ToggleText");
            toggleTextObj.transform.SetParent(toggleButtonObj.transform, false);
            
            var toggleTextRect = toggleTextObj.AddComponent<RectTransform>();
            toggleTextRect.anchorMin = Vector2.zero;
            toggleTextRect.anchorMax = Vector2.one;
            toggleTextRect.offsetMin = Vector2.zero;
            toggleTextRect.offsetMax = Vector2.zero;
            
            toggleButtonText = toggleTextObj.AddComponent<Text>();
            toggleButtonText.text = "Toggle Headphones";
            toggleButtonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            toggleButtonText.fontSize = 12;
            toggleButtonText.color = Color.white;
            toggleButtonText.alignment = TextAnchor.MiddleCenter;
            
            // Create test cube button (middle row)
            var testCubeButtonObj = new GameObject("TestCubeButton");
            testCubeButtonObj.transform.SetParent(panelObj.transform, false);
            
            var testCubeButtonRect = testCubeButtonObj.AddComponent<RectTransform>();
            testCubeButtonRect.anchorMin = new Vector2(0f, 0.33f);
            testCubeButtonRect.anchorMax = new Vector2(0.6f, 0.67f);
            testCubeButtonRect.offsetMin = new Vector2(10f, 2f);
            testCubeButtonRect.offsetMax = new Vector2(-5f, -2f);
            
            testCubeButton = testCubeButtonObj.AddComponent<Button>();
            var testCubeButtonImage = testCubeButtonObj.AddComponent<Image>();
            testCubeButtonImage.color = new Color(0.8f, 0.4f, 0.2f, 1f); // Orange for test
            
            // Add test button text
            var testCubeTextObj = new GameObject("TestCubeText");
            testCubeTextObj.transform.SetParent(testCubeButtonObj.transform, false);
            
            var testCubeTextRect = testCubeTextObj.AddComponent<RectTransform>();
            testCubeTextRect.anchorMin = Vector2.zero;
            testCubeTextRect.anchorMax = Vector2.one;
            testCubeTextRect.offsetMin = Vector2.zero;
            testCubeTextRect.offsetMax = Vector2.zero;
            
            testCubeButtonText = testCubeTextObj.AddComponent<Text>();
            testCubeButtonText.text = "Placement Mode";
            testCubeButtonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            testCubeButtonText.fontSize = 12;
            testCubeButtonText.color = Color.white;
            testCubeButtonText.alignment = TextAnchor.MiddleCenter;
            
            // Create status text (bottom right)
            var statusTextObj = new GameObject("StatusText");
            statusTextObj.transform.SetParent(panelObj.transform, false);
            
            var statusTextRect = statusTextObj.AddComponent<RectTransform>();
            statusTextRect.anchorMin = new Vector2(0.6f, 0f);
            statusTextRect.anchorMax = new Vector2(1f, 1f);
            statusTextRect.offsetMin = new Vector2(5f, 5f);
            statusTextRect.offsetMax = new Vector2(-10f, -5f);
            
            statusText = statusTextObj.AddComponent<Text>();
            statusText.text = "Loading...";
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = 10;
            statusText.color = Color.yellow;
            statusText.alignment = TextAnchor.MiddleCenter;
            
            // Set up button click handlers
            if (toggleButton != null)
            {
                toggleButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnToggleButtonClicked);
            }
            
            if (testCubeButton != null)
            {
                testCubeButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnTestCubeButtonClicked);
            }
            
            // LoggerUtil.Info("HeadphoneControlPanel: UI elements created");
        }

        private void OnToggleButtonClicked()
        {
            try
            {
                LoggingSystem.Info("Headphone toggle button clicked", "UI");
                if (manager != null)
                {
                    bool attached = manager.ToggleHeadphones();
                    // LoggerUtil.Info($"HeadphoneControlPanel: Toggle clicked, headphones {(attached ? "attached" : "removed")}");
                    UpdateButtonText();
                }
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Error toggling headphones: {ex.Message}", "UI");
            }
        }
        
        private void OnTestCubeButtonClicked()
        {
            try
            {
                if (manager != null)
                {
                    manager.TogglePlacementMode();
                    LoggingSystem.Info($"HeadphoneControlPanel: Placement mode toggled", "UI");
                    UpdateButtonText();
                }
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"HeadphoneControlPanel: Placement mode button error: {ex.Message}", "UI");
            }
        }

        public void UpdateButtonText()
        {
            try
            {
                if (manager == null || toggleButtonText == null || statusText == null || testCubeButtonText == null) return;

                bool attached = manager.AreHeadphonesAttached();
                bool inPlacementMode = manager.IsInPlacementMode();
                string status = manager.GetHeadphoneStatus();
                
                // Update headphone button text
                toggleButtonText.text = attached ? "Remove Headphones" : "Attach Headphones";
                
                // Update placement mode button text and color
                testCubeButtonText.text = inPlacementMode ? "Exit Placement" : "Placement Mode";
                
                // Update button colors based on state
                if (toggleButton != null)
                {
                    var buttonImage = toggleButton.GetComponent<Image>();
                    if (buttonImage != null)
                    {
                        buttonImage.color = attached ? 
                            new Color(0.8f, 0.2f, 0.2f, 1f) : // Red for remove
                            new Color(0.2f, 0.4f, 0.8f, 1f);  // Blue for attach
                    }
                }
                
                if (testCubeButton != null)
                {
                    var testButtonImage = testCubeButton.GetComponent<Image>();
                    if (testButtonImage != null)
                    {
                        testButtonImage.color = inPlacementMode ? 
                            new Color(0.8f, 0.8f, 0.2f, 1f) : // Yellow for active placement mode
                            new Color(0.8f, 0.4f, 0.2f, 1f);  // Orange for inactive
                    }
                }
                
                // Update status text with placement mode info
                if (inPlacementMode)
                {
                    statusText.text = "PLACEMENT MODE\nPress P to exit\nLCtrl to place";
                }
                else
                {
                    statusText.text = status;
                }
                
                // Update status text color based on state
                if (status.Contains("attached"))
                    statusText.color = Color.green;
                else if (status.Contains("Failed") || status.Contains("error"))
                    statusText.color = Color.red;
                else if (status.Contains("Loading"))
                    statusText.color = Color.yellow;
                else
                    statusText.color = Color.white;
            }
            catch (System.Exception _)
            {
                // LoggerUtil.Error($"HeadphoneControlPanel: Update button text error: {ex.Message}");
            }
        }
    }
} 