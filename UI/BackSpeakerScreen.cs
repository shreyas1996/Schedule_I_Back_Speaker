using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.UI.Components;

namespace BackSpeakerMod.UI
{
    /// <summary>
    /// Main BackSpeaker UI screen following design specifications
    /// Total Height: 655px (Title: 55px, Tabs: 50px, Content: 550px)
    /// </summary>
    public class BackSpeakerScreen : MonoBehaviour
    {
        // Design Constants from Designs/Design.ms
        private const float SCREEN_WIDTH = 1200f;
        private const float SCREEN_HEIGHT = 655f;
        private const float TITLE_BAR_HEIGHT = 55f;
        private const float TAB_BAR_HEIGHT = 50f;
        private const float CONTENT_AREA_HEIGHT = 550f;
        
        // Dependencies
        private BackSpeakerManager? manager;
        
        // UI Components
        private GameObject? titleBar;
        private TabBarComponent? tabBar;
        private ContentAreaComponent? contentArea;
        
        // Public property to access content area
        public ContentAreaComponent? ContentArea => contentArea;
        
        public BackSpeakerScreen() : base() { }

        public void Setup(BackSpeakerManager manager)
        {
            try
            {
                this.manager = manager;
                
                SetupScreenContainer();
                CreateUILayout();
                
                // Subscribe to track reload events BEFORE triggering reload
                if (manager != null)
                {
                    manager.OnTracksReloaded += OnTracksReloaded;
                }
                
                // Trigger initial track loading through the manager
                LoggingSystem.Info("Triggering initial track loading through manager", "UI");
                manager?.ReloadTracks();
                
                LoggingSystem.Info("BackSpeaker UI created following design specifications", "UI");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"BackSpeaker UI setup failed: {ex.Message}", "UI");
                throw;
            }
        }
        
        private void SetupScreenContainer()
        {
            // Setup main screen container to fit within phone app bounds
            var screenRect = GetComponent<RectTransform>();
            if (screenRect == null)
            {
                screenRect = gameObject.AddComponent<RectTransform>();
            }
            
            // CRITICAL: Anchor to parent container, not full screen
            screenRect.anchorMin = Vector2.zero;
            screenRect.anchorMax = Vector2.one;
            screenRect.offsetMin = Vector2.zero;
            screenRect.offsetMax = Vector2.zero;
            
            // Ensure we stay within container bounds
            screenRect.anchoredPosition = Vector2.zero;
            screenRect.sizeDelta = Vector2.zero;
        }
        
        private void CreateUILayout()
        {
            // Create Title Bar (55px height)
            CreateTitleBar();
            
            // Create Tab Bar (50px height) 
            CreateTabBar();
            
            // Create Content Area (550px height)
            CreateContentArea();
        }
        
        private void CreateTitleBar()
        {
            titleBar = new GameObject("TitleBar");
            titleBar.transform.SetParent(this.transform, false);
            
            var titleRect = titleBar.AddComponent<RectTransform>();
            // Use relative positioning instead of fixed pixels
            titleRect.anchorMin = new Vector2(0f, 0.9f); // Top 10% of container
            titleRect.anchorMax = new Vector2(1f, 1f);   // Full width
            titleRect.offsetMin = new Vector2(5f, 0f);   // Small margin
            titleRect.offsetMax = new Vector2(-5f, 0f);  // Small margin
            titleRect.anchoredPosition = Vector2.zero;
            
            // Title bar background - make it transparent to not block topbar
            var titleBg = titleBar.AddComponent<Image>();
            titleBg.color = new Color(0.15f, 0.15f, 0.15f, 0.3f); // More transparent
            
            // Remove the title text creation to avoid blocking the topbar text
            // The topbar already shows "BackSpeaker" so we don't need duplicate text
        }
        
        private void CreateTabBar()
        {
            var tabBarObj = new GameObject("TabBar");
            tabBarObj.transform.SetParent(this.transform, false);
            
            var tabBarRect = tabBarObj.AddComponent<RectTransform>();
            // Position tab bar in second 10% of container (below title)
            tabBarRect.anchorMin = new Vector2(0f, 0.8f);  // 80% from bottom
            tabBarRect.anchorMax = new Vector2(1f, 0.9f);  // 90% from bottom
            tabBarRect.offsetMin = new Vector2(5f, 0f);    // Small margin
            tabBarRect.offsetMax = new Vector2(-5f, 0f);   // Small margin
            tabBarRect.anchoredPosition = Vector2.zero;
            
            tabBar = tabBarObj.AddComponent<TabBarComponent>();
            tabBar.Setup(manager!);
        }
        
        private void CreateContentArea()
        {
            var contentAreaObj = new GameObject("ContentArea");
            contentAreaObj.transform.SetParent(this.transform, false);
            
            var contentRect = contentAreaObj.AddComponent<RectTransform>();
            // Use bottom 80% of container for content (below title and tabs)
            contentRect.anchorMin = new Vector2(0f, 0f);   // Bottom
            contentRect.anchorMax = new Vector2(1f, 0.8f); // 80% height
            contentRect.offsetMin = new Vector2(5f, 5f);   // Small margins
            contentRect.offsetMax = new Vector2(-5f, 0f);  // Small margins
            contentRect.anchoredPosition = Vector2.zero;
            
            contentArea = contentAreaObj.AddComponent<ContentAreaComponent>();
            contentArea.Setup(manager!, tabBar!);
        }
        
        private void OnTracksReloaded()
        {
            try
            {
                LoggingSystem.Info("Tracks reloaded - updating UI", "UI");
                
                // Only update if content area is ready and not destroyed
                if (contentArea != null && contentArea.gameObject != null)
                {
                    contentArea.UpdateContent();
                }
                else
                {
                    LoggingSystem.Debug("ContentArea not ready for update, skipping", "UI");
                }
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Error updating UI after track reload: {ex.Message}", "UI");
            }
        }
        
        public void Update()
        {
            // Update components regularly for real-time display
            tabBar?.UpdateTabs();
            contentArea?.UpdateContent();
        }
        
        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks and old handlers firing
            if (manager != null)
            {
                manager.OnTracksReloaded -= OnTracksReloaded;
                LoggingSystem.Debug("Unsubscribed from manager events", "UI");
            }
            
            // Clear references
            manager = null;
            titleBar = null;
            tabBar = null;
            contentArea = null;
        }
    }
} 