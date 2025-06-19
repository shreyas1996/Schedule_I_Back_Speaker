using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
using BackSpeakerMod.NewBackend.Utils;
using BackSpeakerMod.UIWrapper;
using BackSpeakerMod.S1Wrapper;

namespace BackSpeakerMod.UIWrapper
{
    /// <summary>
    /// Builder for BackSpeaker-specific UI components
    /// Builds on top of LayoutFactory to create complete functional components
    /// </summary>
    public static class ComponentBuilder
    {
        #region Track Info Components

        /// <summary>
        /// Create a complete track info component with all elements
        /// </summary>
        public static TrackInfoComponent CreateTrackInfoComponent(Transform parent, TrackInfoComponentConfig config = null)
        {
            config = config ?? new TrackInfoComponentConfig();
            
            var component = new TrackInfoComponent();

            // Create layout
            var layout = LayoutFactory.CreateTrackInfoLayout(parent, config.LayoutConfig);
            component.Layout = layout;

            // Create album art image
            component.AlbumArtImage = CreateAlbumArtImage(layout.AlbumArtArea.transform);

            // Create text elements
            component.TrackTitleText = CreateTrackTitleText(layout.TextInfoArea.transform, config);
            component.ArtistText = CreateArtistText(layout.TextInfoArea.transform, config);
            component.AlbumText = CreateAlbumText(layout.TextInfoArea.transform, config);
            component.DurationText = CreateDurationText(layout.TextInfoArea.transform, config);

            // Position the layout
            var layoutRect = layout.Container.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(layoutRect, config.AnchorPreset);
            layoutRect.anchoredPosition = config.Position;

            return component;
        }

        private static Image CreateAlbumArtImage(Transform parent)
        {
            var imageObj = new GameObject("AlbumArt");
            imageObj.transform.SetParent(parent, false);

            var rectTransform = imageObj.AddComponent<RectTransform>();
            S1UIFactory.SetAnchors(rectTransform, AnchorPresets.StretchAll);
            rectTransform.offsetMin = new Vector2(5, 5);
            rectTransform.offsetMax = new Vector2(-5, -5);

            var image = imageObj.AddComponent<Image>();
            image.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            return image;
        }

        private static Text CreateTrackTitleText(Transform parent, TrackInfoComponentConfig config)
        {
            var titleText = S1UIFactory.CreateText(parent, "Track Title", config.TitleFontSize, config.TitleColor, TextAnchor.UpperLeft);
            titleText.gameObject.name = "TrackTitle";
            titleText.fontStyle = FontStyle.Bold;

            var titleRect = titleText.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(titleRect, AnchorPresets.TopLeft);
            titleRect.anchoredPosition = new Vector2(0, -5);
            titleRect.sizeDelta = new Vector2(0, config.TitleFontSize + 5);

            return titleText;
        }

        private static Text CreateArtistText(Transform parent, TrackInfoComponentConfig config)
        {
            var artistText = S1UIFactory.CreateText(parent, "Artist", config.SubtextFontSize, config.SubtextColor, TextAnchor.UpperLeft);
            artistText.gameObject.name = "Artist";

            var artistRect = artistText.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(artistRect, AnchorPresets.TopLeft);
            artistRect.anchoredPosition = new Vector2(0, -25);
            artistRect.sizeDelta = new Vector2(0, config.SubtextFontSize + 3);

            return artistText;
        }

        private static Text CreateAlbumText(Transform parent, TrackInfoComponentConfig config)
        {
            var albumText = S1UIFactory.CreateText(parent, "Album", config.SubtextFontSize, config.SubtextColor, TextAnchor.UpperLeft);
            albumText.gameObject.name = "Album";

            var albumRect = albumText.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(albumRect, AnchorPresets.TopLeft);
            albumRect.anchoredPosition = new Vector2(0, -42);
            albumRect.sizeDelta = new Vector2(0, config.SubtextFontSize + 3);

            return albumText;
        }

        private static Text CreateDurationText(Transform parent, TrackInfoComponentConfig config)
        {
            var durationText = S1UIFactory.CreateText(parent, "0:00", config.SubtextFontSize, config.SubtextColor, TextAnchor.UpperRight);
            durationText.gameObject.name = "Duration";

            var durationRect = durationText.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(durationRect, AnchorPresets.TopRight);
            durationRect.anchoredPosition = new Vector2(0, -5);
            durationRect.sizeDelta = new Vector2(60, config.SubtextFontSize + 3);

            return durationText;
        }

        #endregion

        #region Control Components

        /// <summary>
        /// Create a complete music controls component
        /// </summary>
        public static ControlsComponent CreateControlsComponent(Transform parent, ControlsComponentConfig config = null)
        {
            config = config ?? new ControlsComponentConfig();
            
            var component = new ControlsComponent();

            // Create layout
            var layout = LayoutFactory.CreateControlsLayout(parent, config.LayoutConfig);
            component.Layout = layout;

            // Create buttons
            if (layout.ButtonContainers.Count > 0)
                component.PreviousButton = CreateControlButton(layout.ButtonContainers[0].transform, "⏮ Prev", config.OnPrevious, config);
            if (layout.ButtonContainers.Count > 1)
                component.PlayPauseButton = CreateControlButton(layout.ButtonContainers[1].transform, "▶ Play", config.OnPlayPause, config);
            if (layout.ButtonContainers.Count > 2)
                component.StopButton = CreateControlButton(layout.ButtonContainers[2].transform, "⏹ Stop", config.OnStop, config);
            if (layout.ButtonContainers.Count > 3)
                component.NextButton = CreateControlButton(layout.ButtonContainers[3].transform, "⏭ Next", config.OnNext, config);
            if (layout.ButtonContainers.Count > 4)
                component.ShuffleButton = CreateControlButton(layout.ButtonContainers[4].transform, "🔀 Shuffle", config.OnShuffle, config);

            // Position the layout
            var layoutRect = layout.Container.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(layoutRect, config.AnchorPreset);
            layoutRect.anchoredPosition = config.Position;

            return component;
        }

        private static Button CreateControlButton(Transform parent, string symbol, System.Action onClick, ControlsComponentConfig config)
        {
            var button = S1UIFactory.CreateButton(parent, symbol, (UnityAction)(() => onClick?.Invoke()), null);
            
            var buttonRect = button.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(buttonRect, AnchorPresets.StretchAll);
            buttonRect.offsetMin = new Vector2(2, 2);
            buttonRect.offsetMax = new Vector2(-2, -2);

            return button;
        }

        #endregion

        #region Progress Bar Components

        /// <summary>
        /// Create a progress bar component
        /// </summary>
        public static ProgressBarComponent CreateProgressBarComponent(Transform parent, ProgressBarComponentConfig config = null)
        {
            config = config ?? new ProgressBarComponentConfig();
            
            var component = new ProgressBarComponent();

            // Create container
            component.Container = S1UIFactory.CreatePanel(parent, Color.clear);
            component.Container.name = "ProgressBarContainer";
            var containerRect = component.Container.GetComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(config.Width, config.Height);

            // Create background
            var backgroundObj = S1UIFactory.CreatePanel(component.Container.transform, config.BackgroundColor);
            backgroundObj.name = "ProgressBackground";
            component.Background = backgroundObj.GetComponent<Image>();
            var bgRect = backgroundObj.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(bgRect, AnchorPresets.StretchAll);
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // Create fill
            var fillObj = S1UIFactory.CreatePanel(backgroundObj.transform, config.FillColor);
            fillObj.name = "ProgressFill";
            component.Fill = fillObj.GetComponent<Image>();
            var fillRect = fillObj.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(fillRect, AnchorPresets.StretchAll);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fillRect.anchorMax = new Vector2(0, 1); // Start at 0% width

            // Create time labels
            component.CurrentTimeText = S1UIFactory.CreateText(component.Container.transform, "0:00", 10, Color.white, TextAnchor.MiddleLeft);
            var currentTimeRect = component.CurrentTimeText.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(currentTimeRect, AnchorPresets.BottomLeft);
            currentTimeRect.anchoredPosition = new Vector2(0, -15);
            currentTimeRect.sizeDelta = new Vector2(40, 12);

            component.TotalTimeText = S1UIFactory.CreateText(component.Container.transform, "0:00", 10, Color.white, TextAnchor.MiddleRight);
            var totalTimeRect = component.TotalTimeText.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(totalTimeRect, AnchorPresets.BottomRight);
            totalTimeRect.anchoredPosition = new Vector2(0, -15);
            totalTimeRect.sizeDelta = new Vector2(40, 12);

            // Position the layout
            var layoutRect = component.Container.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(layoutRect, config.AnchorPreset);
            layoutRect.anchoredPosition = config.Position;

            return component;
        }

        #endregion

        #region Tab Bar Components

        /// <summary>
        /// Create a tab bar component
        /// </summary>
        public static TabBarComponent CreateTabBarComponent(Transform parent, TabBarComponentConfig config = null)
        {
            config = config ?? new TabBarComponentConfig();
            
            var component = new TabBarComponent();

            // Create button group layout
            var buttonGroupConfig = new ButtonGroupConfig
            {
                ButtonCount = config.TabNames.Length,
                TotalWidth = config.Width,
                ButtonHeight = config.Height,
                ButtonSpacing = config.TabSpacing
            };

            var layout = LayoutFactory.CreateHorizontalButtonGroup(parent, buttonGroupConfig);
            component.Layout = layout;

            // Create tab buttons
            for (int i = 0; i < config.TabNames.Length; i++)
            {
                var tabName = config.TabNames[i];
                var tabIndex = i;
                
                var button = S1UIFactory.CreateButton(layout.ButtonSlots[i].transform, tabName, 
                    (UnityAction)(() => config.OnTabClick?.Invoke(tabIndex)), null);
                
                var buttonRect = button.GetComponent<RectTransform>();
                S1UIFactory.SetAnchors(buttonRect, AnchorPresets.StretchAll);
                buttonRect.offsetMin = new Vector2(1, 1);
                buttonRect.offsetMax = new Vector2(-1, -1);

                component.TabButtons.Add(button);
            }

            // Position the layout
            var layoutRect = layout.Container.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(layoutRect, config.AnchorPreset);
            layoutRect.anchoredPosition = config.Position;

            return component;
        }

        #endregion

        #region Popup Components

        /// <summary>
        /// Create a styled button with enhanced functionality
        /// </summary>
        public static Button CreateStyledButton(Transform parent, string text, System.Action onClick = null, 
            Vector2? size = null, Color? backgroundColor = null, Color? textColor = null)
        {
            try
            {
                var button = S1UIFactory.CreateButton(parent, text, 
                    onClick != null ? (UnityAction)onClick : null, size);
                
                if (button != null)
                {
                    // Apply custom styling
                    if (backgroundColor.HasValue)
                    {
                        var image = button.GetComponent<Image>();
                        if (image != null)
                        {
                            image.color = backgroundColor.Value;
                        }
                    }

                    if (textColor.HasValue)
                    {
                        var textComponent = button.GetComponentInChildren<Text>();
                        if (textComponent != null)
                        {
                            textComponent.color = textColor.Value;
                        }
                    }
                }

                return button;
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to create styled button: {ex.Message}", "ComponentBuilder");
                return null;
            }
        }

        /// <summary>
        /// Create a popup with enhanced functionality
        /// </summary>
        public static PopupComponent CreatePopup(Transform parent, string title, System.Action onClose = null)
        {
            try
            {
                var popup = new PopupComponent();
                
                // Create popup layout
                popup.Layout = LayoutFactory.CreatePopupLayout(parent, new PopupConfig());
                
                // Create title
                var titleObj = S1UIFactory.CreateText(popup.Layout.PopupContainer.transform, title, 16, Color.white);
                titleObj.name = "PopupTitle";

                // Create close button
                popup.CloseButton = S1UIFactory.CreateButton(popup.Layout.PopupContainer.transform, "X", 
                    onClose != null ? (UnityAction)onClose : null, new Vector2(30, 30));
                popup.CloseButton.name = "CloseButton";

                return popup;
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to create popup: {ex.Message}", "ComponentBuilder");
                return null;
            }
        }

        /// <summary>
        /// Create a confirmation dialog
        /// </summary>
        public static PopupComponent CreateConfirmationDialog(Transform parent, string message, 
            System.Action onConfirm = null, System.Action onCancel = null)
        {
            try
            {
                var popup = new PopupComponent();
                
                // Create popup layout
                popup.Layout = LayoutFactory.CreatePopupLayout(parent, new PopupConfig());
                
                // Create message text
                var messageObj = S1UIFactory.CreateText(popup.Layout.PopupContainer.transform, message, 14, Color.white);
                messageObj.name = "Message";

                // Create confirm button
                var confirmButton = S1UIFactory.CreateButton(popup.Layout.PopupContainer.transform, "Confirm", 
                    onConfirm != null ? (UnityAction)onConfirm : null, new Vector2(80, 30));
                confirmButton.name = "ConfirmButton";

                // Create cancel button  
                var cancelButton = S1UIFactory.CreateButton(popup.Layout.PopupContainer.transform, "Cancel", 
                    onCancel != null ? (UnityAction)onCancel : null, new Vector2(80, 30));
                cancelButton.name = "CancelButton";

                return popup;
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to create confirmation dialog: {ex.Message}", "ComponentBuilder");
                return null;
            }
        }

        #endregion
    }
} 