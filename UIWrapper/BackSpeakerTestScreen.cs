using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using BackSpeakerMod.S1Wrapper;
using BackSpeakerMod.S1Wrapper.Interfaces;
using BackSpeakerMod.NewBackend.Utils;

namespace BackSpeakerMod.UIWrapper
{
    /// <summary>
    /// Test screen builder for BackSpeaker app
    /// </summary>
    public class BackSpeakerTestScreen : MonoBehaviour
    {
        /// <summary>
        /// Create a test screen for the BackSpeaker app
        /// </summary>
        public GameObject? CreateTestScreen()
        {
            try
            {
                var parent = this.transform;
                NewLoggingSystem.Info("Creating BackSpeaker test screen", "TestScreen");
                
                // Main panel
                var mainPanel = new GameObject("BackSpeakerTestScreen");
                mainPanel.transform.SetParent(parent, false);
                
                var mainImage = S1Factory.AddComponent<Image>(mainPanel);
                if (mainImage != null)
                {
                    mainImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
                }
                
                var mainRect = mainPanel.GetComponent<RectTransform>();
                if (mainRect == null)
                {
                    mainRect = S1Factory.AddComponent<RectTransform>(mainPanel);
                }
                
                if (mainRect != null)
                {
                    mainRect.anchorMin = Vector2.zero;
                    mainRect.anchorMax = Vector2.one;
                    mainRect.offsetMin = Vector2.zero;
                    mainRect.offsetMax = Vector2.zero;
                }
                
                // Create vertical layout
                var layoutGroup = S1Factory.AddComponent<VerticalLayoutGroup>(mainPanel);
                if (layoutGroup != null)
                {
                    layoutGroup.spacing = 20;
                    layoutGroup.padding = new RectOffset(20, 20, 20, 20);
                    layoutGroup.childControlHeight = false;
                    layoutGroup.childControlWidth = true;
                    layoutGroup.childForceExpandHeight = false;
                    layoutGroup.childForceExpandWidth = true;
                }
                
                // Title
                CreateTitle(mainPanel.transform);
                
                // Status display
                CreateStatusDisplay(mainPanel.transform);
                
                // Test buttons
                CreateTestButtons(mainPanel.transform);
                
                // Info panel
                CreateInfoPanel(mainPanel.transform);
                
                NewLoggingSystem.Info("✓ Test screen created successfully", "TestScreen");
                return mainPanel;
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to create test screen: {ex}", "TestScreen");
                return null;
            }
        }
        
        private static void CreateTitle(Transform parent)
        {
            var titleGO = new GameObject("Title");
            titleGO.transform.SetParent(parent, false);
            
            var titleText = S1Factory.AddComponent<Text>(titleGO);
            if (titleText != null)
            {
                titleText.text = "BackSpeaker Test Screen";
                titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                titleText.fontSize = 24;
                titleText.fontStyle = FontStyle.Bold;
                titleText.color = Color.white;
                titleText.alignment = TextAnchor.MiddleCenter;
            }
            
            var titleRect = titleGO.GetComponent<RectTransform>();
            if (titleRect == null)
            {
                titleRect = S1Factory.AddComponent<RectTransform>(titleGO);
            }
            
            if (titleRect != null)
            {
                titleRect.sizeDelta = new Vector2(0, 50);
            }
            
            // Add layout element
            var layoutElement = S1Factory.AddComponent<LayoutElement>(titleGO);
            if (layoutElement != null)
            {
                layoutElement.preferredHeight = 50;
            }
        }
        
        private static void CreateStatusDisplay(Transform parent)
        {
            var statusPanel = new GameObject("StatusPanel");
            statusPanel.transform.SetParent(parent, false);
            
            var statusImage = S1Factory.AddComponent<Image>(statusPanel);
            if (statusImage != null)
            {
                statusImage.color = new Color(0.2f, 0.2f, 0.3f, 1f);
            }
            
            var statusText = S1Factory.AddComponent<Text>(statusPanel);
            if (statusText != null)
            {
                statusText.text = GetSystemStatus();
                statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                statusText.fontSize = 14;
                statusText.color = Color.cyan;
                statusText.alignment = TextAnchor.UpperLeft;
            }
            
            var statusRect = statusPanel.GetComponent<RectTransform>();
            if (statusRect == null)
            {
                statusRect = S1Factory.AddComponent<RectTransform>(statusPanel);
            }
            
            if (statusRect != null)
            {
                statusRect.sizeDelta = new Vector2(0, 100);
            }
            
            // Add layout element
            var layoutElement = S1Factory.AddComponent<LayoutElement>(statusPanel);
            if (layoutElement != null)
            {
                layoutElement.preferredHeight = 100;
            }
        }
        
        private static void CreateTestButtons(Transform parent)
        {
            var buttonsPanel = new GameObject("ButtonsPanel");
            buttonsPanel.transform.SetParent(parent, false);
            
            var buttonsImage = S1Factory.AddComponent<Image>(buttonsPanel);
            if (buttonsImage != null)
            {
                buttonsImage.color = new Color(0.15f, 0.15f, 0.2f, 1f);
            }
            
            var buttonsRect = buttonsPanel.GetComponent<RectTransform>();
            if (buttonsRect == null)
            {
                buttonsRect = S1Factory.AddComponent<RectTransform>(buttonsPanel);
            }
            
            if (buttonsRect != null)
            {
                buttonsRect.sizeDelta = new Vector2(0, 80);
            }
            
            // Add horizontal layout
            var horizontalLayout = S1Factory.AddComponent<HorizontalLayoutGroup>(buttonsPanel);
            if (horizontalLayout != null)
            {
                horizontalLayout.spacing = 10;
                horizontalLayout.padding = new RectOffset(10, 10, 10, 10);
                horizontalLayout.childControlHeight = true;
                horizontalLayout.childControlWidth = true;
                horizontalLayout.childForceExpandHeight = false;
                horizontalLayout.childForceExpandWidth = true;
            }
            
            // Create test buttons
            CreateTestButton(buttonsPanel.transform, "Test System");
            
            // Add layout element
            var layoutElement = S1Factory.AddComponent<LayoutElement>(buttonsPanel);
            if (layoutElement != null)
            {
                layoutElement.preferredHeight = 80;
            }
        }
        
        private static void CreateTestButton(Transform parent, string text)
        {
            var buttonGO = new GameObject($"Button_{text}");
            buttonGO.transform.SetParent(parent, false);
            
            var buttonImage = S1Factory.AddComponent<Image>(buttonGO);
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.3f, 0.5f, 0.8f, 1f);
            }
            
            var button = S1Factory.AddComponent<Button>(buttonGO);
            if (button != null)
            {
                // PHASE 1: Add button click handlers for testing
                try
                {
                    var unityAction = S1Factory.ConvertToUnityAction(() => OnTestButtonClick(text));
                    if (unityAction != null) button.onClick.AddListener(unityAction);
                    NewLoggingSystem.Info($"Button '{text}' created with click handler", "TestScreen");
                }
                catch (Exception ex)
                {
                    NewLoggingSystem.Warning($"Failed to setup button: {ex.Message}", "TestScreen");
                }
            }
            
            // Button text
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(buttonGO.transform, false);
            
            var buttonText = S1Factory.AddComponent<Text>(textGO);
            if (buttonText != null)
            {
                buttonText.text = text;
                buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                buttonText.fontSize = 16;
                buttonText.fontStyle = FontStyle.Bold;
                buttonText.color = Color.white;
                buttonText.alignment = TextAnchor.MiddleCenter;
            }
            
            var textRect = textGO.GetComponent<RectTransform>();
            if (textRect == null)
            {
                textRect = S1Factory.AddComponent<RectTransform>(textGO);
            }
            
            if (textRect != null)
            {
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
            }
            
            if (button != null)
            {
                button.targetGraphic = buttonImage;
            }
        }
        
        private static void CreateInfoPanel(Transform parent)
        {
            var infoPanel = new GameObject("InfoPanel");
            infoPanel.transform.SetParent(parent, false);
            
            var infoImage = S1Factory.AddComponent<Image>(infoPanel);
            if (infoImage != null)
            {
                infoImage.color = new Color(0.1f, 0.2f, 0.1f, 1f);
            }
            
            var infoText = S1Factory.AddComponent<Text>(infoPanel);
            if (infoText != null)
            {
                infoText.text = GetInfoText();
                infoText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                infoText.fontSize = 12;
                infoText.color = Color.white;
                infoText.alignment = TextAnchor.UpperLeft;
            }
            
            var infoRect = infoPanel.GetComponent<RectTransform>();
            if (infoRect == null)
            {
                infoRect = S1Factory.AddComponent<RectTransform>(infoPanel);
            }
            
            if (infoRect != null)
            {
                infoRect.sizeDelta = new Vector2(0, 200);
            }
            
            // Add layout element
            var layoutElement = S1Factory.AddComponent<LayoutElement>(infoPanel);
            if (layoutElement != null)
            {
                layoutElement.preferredHeight = 200;
            }
        }
        
        private static string GetSystemStatus()
        {
            try
            {
                var status = S1Factory.GetSystemStatus();
                
                // App orientation status
                var phoneApp = BackSpeakerMod.NewBackend.BackSpeakerMainManager.Instance?.GetPhoneApp();
                string rotationStatus = "Unknown";
                if (phoneApp != null)
                {
                    rotationStatus = phoneApp.IsInPortraitMode() ? "✓ Portrait" : "✗ Landscape";
                }
                
                return $"System Status:\n" +
                       $"Environment: {status.Environment}\n" +
                       $"Player: {(status.HasPlayer ? "✓" : "✗")}\n" +
                       $"Audio: {(status.HasAudioManager ? "✓" : "✗")}\n" +
                       $"Phone: {(status.HasPhone ? "✓" : "✗")}\n" +
                       $"Rotation: {rotationStatus}\n" +
                       $"Jukeboxes: {status.JukeboxCount}";
            }
            catch (Exception ex)
            {
                return $"Status Error: {ex.Message}";
            }
        }
        
        private static string GetInfoText()
        {
            return "BackSpeaker Mod Test App\n\n" +
                   "This test screen verifies BackSpeaker functionality:\n" +
                   "• App creation and UI integration\n" +
                   "• Audio management systems\n" +
                   "• Scene and player detection\n" +
                   "• Component registration\n\n" +
                   "Use the buttons to test different components.";
        }
        
        private static void OnTestButtonClick(string buttonName)
        {
            NewLoggingSystem.Info($"Test button clicked: {buttonName}", "TestScreen");
            
            try
            {
                var mainManager = BackSpeakerMod.NewBackend.BackSpeakerMainManager.Instance;
                var phoneApp = mainManager?.GetPhoneApp();
                
                switch (buttonName)
                {
                    case "Test System":
                        NewLoggingSystem.Info("🔍 Running system tests...", "TestScreen");
                        var systemStatus = S1Factory.GetSystemStatus();
                        NewLoggingSystem.Info($"System Environment: {systemStatus.Environment}", "TestScreen");
                        NewLoggingSystem.Info($"Has Player: {systemStatus.HasPlayer}", "TestScreen");
                        NewLoggingSystem.Info($"Has Audio Manager: {systemStatus.HasAudioManager}", "TestScreen");
                        NewLoggingSystem.Info($"Has Phone: {systemStatus.HasPhone}", "TestScreen");
                        NewLoggingSystem.Info($"Jukebox Count: {systemStatus.JukeboxCount}", "TestScreen");
                        break;
                        
                    default:
                        NewLoggingSystem.Info($"Unknown test button: {buttonName}", "TestScreen");
                        break;
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error in test button handler: {ex}", "TestScreen");
            }
        }
    }
} 