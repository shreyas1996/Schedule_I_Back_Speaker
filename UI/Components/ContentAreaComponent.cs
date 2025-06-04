using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Modules;

namespace BackSpeakerMod.UI.Components
{
    /// <summary>
    /// Content area component following design specifications exactly
    /// Layout: TrackInfo(120px) + Progress(30px) + Controls(50px) + Actions(60px) + Playlist(50px) + Help(40px)
    /// </summary>
    public class ContentAreaComponent : MonoBehaviour
    {
        // Design constants from Designs/Design.ms
        private const float TRACK_INFO_HEIGHT = 120f;
        private const float PROGRESS_BAR_HEIGHT = 30f;
        private const float CONTROLS_HEIGHT = 50f;
        private const float ACTION_BUTTONS_HEIGHT = 60f;
        private const float PLAYLIST_TOGGLE_HEIGHT = 50f;
        private const float HELP_TEXT_HEIGHT = 40f;
        private const float PADDING = 10f;
        
        // Dependencies
        private BackSpeakerManager? manager;
        private TabBarComponent? tabBar;
        
        // UI Components
        private TrackInfoComponent? trackInfo;
        private ProgressBarComponent? progressBar;
        private ControlsComponent? controls;
        private ActionButtonsComponent? actionButtons;
        private PlaylistToggleComponent? playlistToggle;
        private HelpTextComponent? helpText;
        
        private MusicSourceType currentTab = MusicSourceType.Jukebox;
        
        // Public property to access playlist toggle
        public PlaylistToggleComponent? PlaylistToggle => playlistToggle;
        
        public ContentAreaComponent() : base() { }
        
        public void Setup(BackSpeakerManager manager, TabBarComponent tabBar)
        {
            this.manager = manager;
            this.tabBar = tabBar;
            
            // Subscribe to tab changes
            tabBar.OnTabChanged += OnTabChanged;
            
            CreateContentLayout();
            
            // Initialize with jukebox tab
            OnTabChanged(MusicSourceType.Jukebox);
        }
        
        private void CreateContentLayout()
        {
            // Content area background
            var background = gameObject.AddComponent<Image>();
            background.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            
            float currentY = -PADDING; // Start from top with padding
            
            // Track Info Panel (120px height)
            CreateTrackInfoPanel(ref currentY);
            
            // Progress Bar Panel (30px height)
            CreateProgressBarPanel(ref currentY);
            
            // Controls Panel (50px height)
            CreateControlsPanel(ref currentY);
            
            // Action Buttons Panel (60px height)
            CreateActionButtonsPanel(ref currentY);
            
            // Playlist Toggle Panel (50px height)
            CreatePlaylistTogglePanel(ref currentY);
            
            // Help Text Panel (40px height)
            CreateHelpTextPanel(ref currentY);
        }
        
        private void CreateTrackInfoPanel(ref float currentY)
        {
            var panelObj = new GameObject("TrackInfoPanel");
            panelObj.transform.SetParent(this.transform, false);
            
            var panelRect = SetupPanelRect(panelObj, currentY, TRACK_INFO_HEIGHT);
            currentY -= (TRACK_INFO_HEIGHT + PADDING);
            
            trackInfo = panelObj.AddComponent<TrackInfoComponent>();
            trackInfo.Setup(manager!);
        }
        
        private void CreateProgressBarPanel(ref float currentY)
        {
            var panelObj = new GameObject("ProgressBarPanel");
            panelObj.transform.SetParent(this.transform, false);
            
            var panelRect = SetupPanelRect(panelObj, currentY, PROGRESS_BAR_HEIGHT);
            currentY -= (PROGRESS_BAR_HEIGHT + PADDING);
            
            progressBar = panelObj.AddComponent<ProgressBarComponent>();
            progressBar.Setup(manager!);
        }
        
        private void CreateControlsPanel(ref float currentY)
        {
            var panelObj = new GameObject("ControlsPanel");
            panelObj.transform.SetParent(this.transform, false);
            
            var panelRect = SetupPanelRect(panelObj, currentY, CONTROLS_HEIGHT);
            currentY -= (CONTROLS_HEIGHT + PADDING);
            
            controls = panelObj.AddComponent<ControlsComponent>();
            controls.Setup(manager!);
        }
        
        private void CreateActionButtonsPanel(ref float currentY)
        {
            var panelObj = new GameObject("ActionButtonsPanel");
            panelObj.transform.SetParent(this.transform, false);
            
            var panelRect = SetupPanelRect(panelObj, currentY, ACTION_BUTTONS_HEIGHT);
            currentY -= (ACTION_BUTTONS_HEIGHT + PADDING);
            
            actionButtons = panelObj.AddComponent<ActionButtonsComponent>();
            actionButtons.Setup(manager!);
        }
        
        private void CreatePlaylistTogglePanel(ref float currentY)
        {
            var panelObj = new GameObject("PlaylistTogglePanel");
            panelObj.transform.SetParent(this.transform, false);
            
            var panelRect = SetupPanelRect(panelObj, currentY, PLAYLIST_TOGGLE_HEIGHT);
            currentY -= (PLAYLIST_TOGGLE_HEIGHT + PADDING);
            
            playlistToggle = panelObj.AddComponent<PlaylistToggleComponent>();
            playlistToggle.Setup(manager!);
        }
        
        private void CreateHelpTextPanel(ref float currentY)
        {
            var panelObj = new GameObject("HelpTextPanel");
            panelObj.transform.SetParent(this.transform, false);
            
            var panelRect = SetupPanelRect(panelObj, currentY, HELP_TEXT_HEIGHT);
            
            helpText = panelObj.AddComponent<HelpTextComponent>();
            helpText.Setup();
        }
        
        private RectTransform SetupPanelRect(GameObject panelObj, float yPosition, float height)
        {
            var panelRect = panelObj.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 1f); // Anchor to top
            panelRect.anchorMax = new Vector2(1f, 1f);
            panelRect.pivot = new Vector2(0.5f, 1f);
            panelRect.sizeDelta = new Vector2(0f, height);
            panelRect.anchoredPosition = new Vector2(0f, yPosition);
            
            return panelRect;
        }
        
        private void OnTabChanged(MusicSourceType newTab)
        {
            currentTab = newTab;
            
            // Switch active session in the manager
            if (manager != null)
            {
                manager.SetMusicSource(newTab);
                LoggingSystem.Info($"Content area switched to {newTab} session", "UI");
            }
            
            // Update all components for the new tab
            actionButtons?.UpdateForTab(newTab);
            playlistToggle?.UpdateForTab(newTab);
            helpText?.UpdateForTab(newTab);
            
            // Force content update to show session-specific data
            UpdateContent();
        }
        
        public void UpdateContent()
        {
            // Update all components
            trackInfo?.UpdateDisplay();
            progressBar?.UpdateProgress();
            controls?.UpdateControls();
            actionButtons?.UpdateButtons();
            playlistToggle?.UpdatePlaylist();
        }
        
        private void OnDestroy()
        {
            if (tabBar != null)
            {
                tabBar.OnTabChanged -= OnTabChanged;
            }
        }
    }
} 