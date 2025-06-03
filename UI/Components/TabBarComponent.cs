using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Modules;

namespace BackSpeakerMod.UI.Components
{
    /// <summary>
    /// Tab bar component following design specifications
    /// Contains 3 tabs: Jukebox, Local Music, YouTube Music + Status text
    /// </summary>
    public class TabBarComponent : MonoBehaviour
    {
        // Dependencies
        private BackSpeakerManager? manager;
        private TrackLoader? trackLoader;
        
        // Tab buttons
        private readonly Dictionary<MusicSourceType, Button> tabButtons = new Dictionary<MusicSourceType, Button>();
        private MusicSourceType currentTab = MusicSourceType.Jukebox;
        
        // Status text
        private Text? statusText;
        
        // Events
        public System.Action<MusicSourceType>? OnTabChanged;
        
        public TabBarComponent() : base() { }
        
        public void Setup(BackSpeakerManager manager, TrackLoader trackLoader)
        {
            this.manager = manager;
            this.trackLoader = trackLoader;
            
            CreateTabBarLayout();
            SetActiveTab(MusicSourceType.Jukebox); // Default to Jukebox as per design
        }
        
        private void CreateTabBarLayout()
        {
            // Tab bar background
            var background = gameObject.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.95f);
            
            // Create three tabs as per design
            CreateTab(MusicSourceType.Jukebox, "🎮 Jukebox", 0f, 0.25f);
            CreateTab(MusicSourceType.LocalFolder, "📁 Local Music", 0.25f, 0.5f);
            CreateTab(MusicSourceType.YouTube, "📺 YouTube Music", 0.5f, 0.75f);
            
            // Create status text area (right side)
            CreateStatusText();
        }
        
        private void CreateTab(MusicSourceType sourceType, string tabText, float startX, float endX)
        {
            var tabObj = new GameObject($"Tab_{sourceType}");
            tabObj.transform.SetParent(this.transform, false);
            
            var tabRect = tabObj.AddComponent<RectTransform>();
            tabRect.anchorMin = new Vector2(startX, 0f);
            tabRect.anchorMax = new Vector2(endX, 1f);
            tabRect.offsetMin = new Vector2(2f, 2f);
            tabRect.offsetMax = new Vector2(-2f, -2f);
            
            // Tab button
            var button = tabObj.AddComponent<Button>();
            var buttonImage = tabObj.AddComponent<Image>();
            
            // Tab text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(tabObj.transform, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5f, 0f);
            textRect.offsetMax = new Vector2(-5f, 0f);
            
            var text = textObj.AddComponent<Text>();
            text.text = tabText;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 14;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            // Button setup
            button.targetGraphic = buttonImage;
            button.onClick.AddListener((UnityEngine.Events.UnityAction)delegate() { OnTabClicked(sourceType); });
            
            // Store reference
            tabButtons[sourceType] = button;
            
            // Set initial inactive state
            SetTabVisualState(sourceType, false);
        }
        
        private void CreateStatusText()
        {
            var statusObj = new GameObject("StatusText");
            statusObj.transform.SetParent(this.transform, false);
            
            var statusRect = statusObj.AddComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.75f, 0f);
            statusRect.anchorMax = new Vector2(1f, 1f);
            statusRect.offsetMin = new Vector2(10f, 0f);
            statusRect.offsetMax = new Vector2(-10f, 0f);
            
            statusText = statusObj.AddComponent<Text>();
            statusText.text = "Status: Loading...";
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = 12;
            statusText.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            statusText.alignment = TextAnchor.MiddleRight;
        }
        
        private void OnTabClicked(MusicSourceType sourceType)
        {
            if (sourceType != currentTab)
            {
                SetActiveTab(sourceType);
                OnTabChanged?.Invoke(sourceType);
                
                LoggingSystem.Info($"Tab switched to: {sourceType}", "UI");
            }
        }
        
        public void SetActiveTab(MusicSourceType sourceType)
        {
            // Update visual states
            foreach (var kvp in tabButtons)
            {
                SetTabVisualState(kvp.Key, kvp.Key == sourceType);
            }
            
            currentTab = sourceType;
            UpdateStatusText();
        }
        
        private void SetTabVisualState(MusicSourceType sourceType, bool isActive)
        {
            if (!tabButtons.ContainsKey(sourceType)) return;
            
            var button = tabButtons[sourceType];
            var buttonImage = button.GetComponent<Image>();
            var text = button.GetComponentInChildren<Text>();
            
            if (isActive)
            {
                // Active state - bright colors matching design
                buttonImage.color = sourceType switch
                {
                    MusicSourceType.Jukebox => new Color(0.2f, 0.7f, 0.2f, 0.9f),      // Green
                    MusicSourceType.LocalFolder => new Color(0.2f, 0.4f, 0.8f, 0.9f),  // Blue
                    MusicSourceType.YouTube => new Color(0.8f, 0.2f, 0.2f, 0.9f),      // Red
                    _ => new Color(0.6f, 0.6f, 0.6f, 0.9f)
                };
                text.fontStyle = FontStyle.Bold;
                text.color = Color.white;
            }
            else
            {
                // Inactive state - darker colors
                buttonImage.color = sourceType switch
                {
                    MusicSourceType.Jukebox => new Color(0.1f, 0.3f, 0.1f, 0.7f),      // Dark Green
                    MusicSourceType.LocalFolder => new Color(0.1f, 0.2f, 0.4f, 0.7f),  // Dark Blue
                    MusicSourceType.YouTube => new Color(0.4f, 0.1f, 0.1f, 0.7f),      // Dark Red
                    _ => new Color(0.3f, 0.3f, 0.3f, 0.7f)
                };
                text.fontStyle = FontStyle.Normal;
                text.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            }
        }
        
        private void UpdateStatusText()
        {
            if (statusText == null || manager == null) return;
            
            try
            {
                var trackCount = manager.GetAllTracks().Count;
                
                var statusMessage = currentTab switch
                {
                    MusicSourceType.Jukebox => $"Status: {trackCount} tracks loaded",
                    MusicSourceType.LocalFolder => $"Status: {trackCount} tracks loaded", 
                    MusicSourceType.YouTube => $"Status: {trackCount} cached tracks",
                    _ => "Status: Ready"
                };
                
                statusText.text = statusMessage;
            }
            catch (System.Exception ex)
            {
                statusText.text = "Status: Error";
                LoggingSystem.Error($"Status update failed: {ex.Message}", "UI");
            }
        }
        
        public void UpdateTabs()
        {
            // Update status text periodically
            UpdateStatusText();
        }
        
        public MusicSourceType GetCurrentTab() => currentTab;
    }
} 