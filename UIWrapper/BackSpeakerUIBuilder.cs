using UnityEngine;
using UnityEngine.Events;
using System;
using BackSpeakerMod.UIWrapper;
using BackSpeakerMod.S1Wrapper;

namespace BackSpeakerMod.UIWrapper
{
    /// <summary>
    /// Main builder for complete BackSpeaker UI layouts
    /// Combines all wrapper components to create full application interfaces
    /// </summary>
    public static class BackSpeakerUIBuilder
    {
        #region Complete Screen Builders

        /// <summary>
        /// Create a complete BackSpeaker main screen with all components
        /// </summary>
        public static BackSpeakerMainScreen CreateMainScreen(Transform parent, MainScreenConfig config = null)
        {
            config = config ?? new MainScreenConfig();
            
            var screen = new BackSpeakerMainScreen();

            // Create main layout
            screen.MainLayout = LayoutFactory.CreateMainScreenLayout(parent, config.LayoutConfig);

            // Create track info component in header
            if (config.ShowTrackInfo)
            {
                screen.TrackInfo = ComponentBuilder.CreateTrackInfoComponent(screen.MainLayout.HeaderArea.transform, config.TrackInfoConfig);
            }

            // Create content components
            if (config.ShowControls)
            {
                screen.Controls = ComponentBuilder.CreateControlsComponent(screen.MainLayout.ContentArea.transform, config.ControlsConfig);
            }

            if (config.ShowProgressBar)
            {
                screen.ProgressBar = ComponentBuilder.CreateProgressBarComponent(screen.MainLayout.ContentArea.transform, config.ProgressBarConfig);
            }

            if (config.ShowTabBar)
            {
                screen.TabBar = ComponentBuilder.CreateTabBarComponent(screen.MainLayout.ContentArea.transform, config.TabBarConfig);
            }

            // Position main layout
            var mainRect = screen.MainLayout.MainContainer.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(mainRect, AnchorPresets.StretchAll);

            return screen;
        }

        /// <summary>
        /// Create a complete popup screen
        /// </summary>
        public static BackSpeakerPopupScreen CreatePopupScreen(Transform parent, PopupScreenConfig config = null)
        {
            config = config ?? new PopupScreenConfig();
            
            var screen = new BackSpeakerPopupScreen();

            // Create popup component
            screen.Popup = ComponentBuilder.CreatePopup(parent, "Popup", null);

            // Add custom content based on popup type
            switch (config.PopupType)
            {
                case PopupType.YouTubeAdd:
                    CreateYouTubeAddContent(screen.Popup.Layout.ContentArea.transform, config);
                    break;
                case PopupType.PlaylistSelection:
                    CreatePlaylistSelectionContent(screen.Popup.Layout.ContentArea.transform, config);
                    break;
                case PopupType.Settings:
                    CreateSettingsContent(screen.Popup.Layout.ContentArea.transform, config);
                    break;
                case PopupType.TrackDetails:
                    CreateTrackDetailsContent(screen.Popup.Layout.ContentArea.transform, config);
                    break;
            }

            return screen;
        }

        #endregion

        #region Popup Content Creators

        private static void CreateYouTubeAddContent(Transform parent, PopupScreenConfig config)
        {
            // URL input field
            var urlInput = S1UIFactory.CreateInputField(parent, "Enter YouTube URL...", new Vector2(350, 35));
            var urlRect = urlInput.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(urlRect, AnchorPresets.TopCenter);
            urlRect.anchoredPosition = new Vector2(0, -30);

            // Title input field
            var titleInput = S1UIFactory.CreateInputField(parent, "Track title (optional)...", new Vector2(350, 35));
            var titleRect = titleInput.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(titleRect, AnchorPresets.TopCenter);
            titleRect.anchoredPosition = new Vector2(0, -80);

            // Playlist selection
            var playlistLabel = S1UIFactory.CreateText(parent, "Add to playlist:", 14, Color.white);
            var playlistLabelRect = playlistLabel.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(playlistLabelRect, AnchorPresets.TopLeft);
            playlistLabelRect.anchoredPosition = new Vector2(20, -130);

            // Playlist dropdown (simplified as button for now)
            var playlistButton = S1UIFactory.CreateButton(parent, "Select Playlist ▼", (UnityAction)(() => config.OnPlaylistSelect?.Invoke()), new Vector2(200, 35));
            var playlistButtonRect = playlistButton.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(playlistButtonRect, AnchorPresets.TopLeft);
            playlistButtonRect.anchoredPosition = new Vector2(20, -160);
        }

        private static void CreatePlaylistSelectionContent(Transform parent, PopupScreenConfig config)
        {
            // Create list layout for playlists
            var listLayout = LayoutFactory.CreateListLayout(parent, new ListConfig { Width = 350, Height = 200 });
            var listRect = listLayout.Container.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(listRect, AnchorPresets.TopCenter);
            listRect.anchoredPosition = new Vector2(0, -20);

            // Add playlist items (this would be populated dynamically)
            for (int i = 0; i < 5; i++)
            {
                var playlistItem = S1UIFactory.CreateButton(listLayout.ContentArea.transform, $"Playlist {i + 1}", 
                    (UnityAction)(() => config.OnPlaylistItemClick?.Invoke(i.ToString())), new Vector2(320, 35));
                
                var itemRect = playlistItem.GetComponent<RectTransform>();
                S1UIFactory.SetAnchors(itemRect, AnchorPresets.TopCenter);
                itemRect.anchoredPosition = new Vector2(0, -(i * 40 + 20));
            }
        }

        private static void CreateSettingsContent(Transform parent, PopupScreenConfig config)
        {
            // Volume setting
            var volumeLabel = S1UIFactory.CreateText(parent, "Volume:", 14, Color.white);
            var volumeLabelRect = volumeLabel.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(volumeLabelRect, AnchorPresets.TopLeft);
            volumeLabelRect.anchoredPosition = new Vector2(20, -30);

            // Volume slider would go here (simplified as text for now)
            var volumeValue = S1UIFactory.CreateText(parent, "75%", 14, Color.white);
            var volumeValueRect = volumeValue.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(volumeValueRect, AnchorPresets.TopRight);
            volumeValueRect.anchoredPosition = new Vector2(-20, -30);

            // Auto-play setting
            var autoPlayLabel = S1UIFactory.CreateText(parent, "Auto-play next track:", 14, Color.white);
            var autoPlayLabelRect = autoPlayLabel.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(autoPlayLabelRect, AnchorPresets.TopLeft);
            autoPlayLabelRect.anchoredPosition = new Vector2(20, -70);

            var autoPlayToggle = S1UIFactory.CreateButton(parent, "ON", (UnityAction)(() => config.OnAutoPlayToggle?.Invoke()), new Vector2(50, 30));
            var autoPlayToggleRect = autoPlayToggle.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(autoPlayToggleRect, AnchorPresets.TopRight);
            autoPlayToggleRect.anchoredPosition = new Vector2(-20, -70);
        }

        private static void CreateTrackDetailsContent(Transform parent, PopupScreenConfig config)
        {
            // Track info display
            var trackInfo = ComponentBuilder.CreateTrackInfoComponent(parent, ComponentPresets.DefaultTrackInfo);
            var trackRect = trackInfo.Layout.Container.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(trackRect, AnchorPresets.TopCenter);
            trackRect.anchoredPosition = new Vector2(0, -20);

            // Additional details
            var detailsLabel = S1UIFactory.CreateText(parent, "Additional Details:", 16, Color.white, UnityEngine.TextAnchor.MiddleLeft);
            var detailsLabelRect = detailsLabel.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(detailsLabelRect, AnchorPresets.TopLeft);
            detailsLabelRect.anchoredPosition = new Vector2(20, -120);

            // File path, size, etc.
            var filePath = S1UIFactory.CreateText(parent, "File: /path/to/music.mp3", 12, new Color(0.8f, 0.8f, 0.8f), UnityEngine.TextAnchor.MiddleLeft);
            var filePathRect = filePath.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(filePathRect, AnchorPresets.TopLeft);
            filePathRect.anchoredPosition = new Vector2(20, -150);
            filePathRect.sizeDelta = new Vector2(350, 15);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Create a complete BackSpeaker app with S1PhoneAppBuilder integration
        /// </summary>
        public static GameObject CreateCompleteBackSpeakerApp(string appName, string displayName, MainScreenConfig mainScreenConfig = null)
        {
            return S1PhoneAppBuilder.CreateBackSpeakerApp(appName, displayName, container =>
            {
                var mainScreen = CreateMainScreen(container, mainScreenConfig);
                
                // Setup event handlers
                SetupMainScreenEvents(mainScreen);
            });
        }

        private static void SetupMainScreenEvents(BackSpeakerMainScreen mainScreen)
        {
            // Setup tab bar events
            if (mainScreen.TabBar != null)
            {
                // Tab switching logic would go here
            }

            // Setup control events
            if (mainScreen.Controls != null)
            {
                // Music control logic would go here
            }
        }

        /// <summary>
        /// Show a popup over the main screen
        /// </summary>
        public static BackSpeakerPopupScreen ShowPopup(Transform parent, PopupType popupType, PopupScreenConfig config = null)
        {
            config = config ?? new PopupScreenConfig { PopupType = popupType };
            config.PopupType = popupType;

            return CreatePopupScreen(parent, config);
        }

        #endregion
    }

    #region Screen Structures

    /// <summary>
    /// Complete main screen structure
    /// </summary>
    public class BackSpeakerMainScreen
    {
        public BackSpeakerLayout? MainLayout { get; set; }
        public TrackInfoComponent? TrackInfo { get; set; }
        public ControlsComponent? Controls { get; set; }
        public ProgressBarComponent? ProgressBar { get; set; }
        public TabBarComponent? TabBar { get; set; }
    }

    /// <summary>
    /// Complete popup screen structure
    /// </summary>
    public class BackSpeakerPopupScreen
    {
        public PopupComponent? Popup { get; set; }
        
        public void Show() => Popup?.Show();
        public void Hide() => Popup?.Hide();
    }

    #endregion

    #region Configuration Classes

    /// <summary>
    /// Main screen configuration
    /// </summary>
    public class MainScreenConfig
    {
        public bool ShowTrackInfo { get; set; } = true;
        public bool ShowControls { get; set; } = true;
        public bool ShowProgressBar { get; set; } = true;
        public bool ShowTabBar { get; set; } = false;

        public LayoutConfig? LayoutConfig { get; set; }
        public TrackInfoComponentConfig? TrackInfoConfig { get; set; }
        public ControlsComponentConfig? ControlsConfig { get; set; }
        public ProgressBarComponentConfig? ProgressBarConfig { get; set; }
        public TabBarComponentConfig? TabBarConfig { get; set; }
    }

    /// <summary>
    /// Popup screen configuration
    /// </summary>
    public class PopupScreenConfig
    {
        public PopupType PopupType { get; set; } = PopupType.Confirmation;
        public PopupComponentConfig? PopupConfig { get; set; }
        
        // Event handlers (can be null)
        public System.Action? OnPlaylistSelect { get; set; }
        public System.Action<string>? OnPlaylistItemClick { get; set; }
        public System.Action? OnAutoPlayToggle { get; set; }
    }

    /// <summary>
    /// Types of popups available
    /// </summary>
    public enum PopupType
    {
        Standard,
        YouTubeAdd,
        PlaylistSelection,
        Settings,
        TrackDetails,
        Error,
        Confirmation
    }

    #endregion
} 