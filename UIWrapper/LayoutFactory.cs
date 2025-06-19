using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace BackSpeakerMod.UIWrapper
{
    /// <summary>
    /// Factory for creating standardized UI layouts
    /// Provides reusable layout patterns for BackSpeaker UI
    /// </summary>
    public static class LayoutFactory
    {
        #region Main Screen Layouts

        /// <summary>
        /// Create the main BackSpeaker screen layout structure
        /// </summary>
        public static BackSpeakerLayout CreateMainScreenLayout(Transform parent, LayoutConfig config = null)
        {
            config = config ?? new LayoutConfig();
            
            var layout = new BackSpeakerLayout();

            // Create main container
            layout.MainContainer = S1UIFactory.CreatePanel(parent, Color.clear);
            layout.MainContainer.name = "BackSpeakerMainContainer";
            var containerRect = layout.MainContainer.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(containerRect, AnchorPresets.StretchAll);
            containerRect.offsetMin = new Vector2(config.Padding, config.Padding);
            containerRect.offsetMax = new Vector2(-config.Padding, -config.Padding);

            // Create header area
            layout.HeaderArea = CreateHeaderArea(layout.MainContainer.transform, config);

            // Create content area
            layout.ContentArea = CreateContentArea(layout.MainContainer.transform, config);

            // Create footer area
            layout.FooterArea = CreateFooterArea(layout.MainContainer.transform, config);

            return layout;
        }

        private static GameObject CreateHeaderArea(Transform parent, LayoutConfig config)
        {
            var headerArea = S1UIFactory.CreatePanel(parent, new Color(0.1f, 0.1f, 0.1f, 0.3f));
            headerArea.name = "HeaderArea";
            
            var headerRect = headerArea.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(headerRect, AnchorPresets.TopCenter);
            headerRect.anchoredPosition = new Vector2(0, 0);
            headerRect.sizeDelta = new Vector2(-20, config.HeaderHeight);

            return headerArea;
        }

        private static GameObject CreateContentArea(Transform parent, LayoutConfig config)
        {
            var contentArea = S1UIFactory.CreatePanel(parent, Color.clear);
            contentArea.name = "ContentArea";
            
            var contentRect = contentArea.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(contentRect, AnchorPresets.StretchAll);
            contentRect.offsetMin = new Vector2(0, config.FooterHeight);
            contentRect.offsetMax = new Vector2(0, -config.HeaderHeight);

            return contentArea;
        }

        private static GameObject CreateFooterArea(Transform parent, LayoutConfig config)
        {
            var footerArea = S1UIFactory.CreatePanel(parent, new Color(0.1f, 0.1f, 0.1f, 0.3f));
            footerArea.name = "FooterArea";
            
            var footerRect = footerArea.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(footerRect, AnchorPresets.BottomCenter);
            footerRect.anchoredPosition = new Vector2(0, 0);
            footerRect.sizeDelta = new Vector2(-20, config.FooterHeight);

            return footerArea;
        }

        #endregion

        #region Content Layouts

        /// <summary>
        /// Create a track info layout
        /// </summary>
        public static TrackInfoLayout CreateTrackInfoLayout(Transform parent, TrackInfoConfig config = null)
        {
            config = config ?? new TrackInfoConfig();
            
            var layout = new TrackInfoLayout();

            // Main container
            layout.Container = S1UIFactory.CreatePanel(parent, new Color(0.15f, 0.15f, 0.15f, 0.8f));
            layout.Container.name = "TrackInfoContainer";
            var containerRect = layout.Container.GetComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(config.Width, config.Height);

            // Album art area
            layout.AlbumArtArea = CreateAlbumArtArea(layout.Container.transform, config);

            // Text info area
            layout.TextInfoArea = CreateTextInfoArea(layout.Container.transform, config);

            return layout;
        }

        private static GameObject CreateAlbumArtArea(Transform parent, TrackInfoConfig config)
        {
            var albumArea = S1UIFactory.CreatePanel(parent, new Color(0.2f, 0.2f, 0.2f, 1f));
            albumArea.name = "AlbumArtArea";
            
            var albumRect = albumArea.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(albumRect, AnchorPresets.MiddleLeft);
            albumRect.anchoredPosition = new Vector2(config.AlbumArtSize / 2 + 10, 0);
            albumRect.sizeDelta = new Vector2(config.AlbumArtSize, config.AlbumArtSize);

            return albumArea;
        }

        private static GameObject CreateTextInfoArea(Transform parent, TrackInfoConfig config)
        {
            var textArea = S1UIFactory.CreatePanel(parent, Color.clear);
            textArea.name = "TextInfoArea";
            
            var textRect = textArea.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(textRect, AnchorPresets.StretchAll);
            textRect.offsetMin = new Vector2(config.AlbumArtSize + 20, 5);
            textRect.offsetMax = new Vector2(-10, -5);

            return textArea;
        }

        /// <summary>
        /// Create a control buttons layout
        /// </summary>
        public static ControlsLayout CreateControlsLayout(Transform parent, ControlsConfig config = null)
        {
            config = config ?? new ControlsConfig();
            
            var layout = new ControlsLayout();

            // Main container
            layout.Container = S1UIFactory.CreatePanel(parent, Color.clear);
            layout.Container.name = "ControlsContainer";
            var containerRect = layout.Container.GetComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(config.Width, config.Height);

            // Create button containers
            var buttonSpacing = (config.Width - (config.ButtonCount * config.ButtonSize)) / (config.ButtonCount + 1);
            
            for (int i = 0; i < config.ButtonCount; i++)
            {
                var buttonContainer = S1UIFactory.CreatePanel(layout.Container.transform, Color.clear);
                buttonContainer.name = $"ButtonContainer_{i}";
                
                var buttonRect = buttonContainer.GetComponent<RectTransform>();
                S1UIFactory.SetAnchors(buttonRect, AnchorPresets.MiddleLeft);
                
                var xPos = buttonSpacing + (i * (config.ButtonSize + buttonSpacing)) + (config.ButtonSize / 2);
                buttonRect.anchoredPosition = new Vector2(xPos, 0);
                buttonRect.sizeDelta = new Vector2(config.ButtonSize, config.ButtonSize);

                layout.ButtonContainers.Add(buttonContainer);
            }

            return layout;
        }

        #endregion

        #region Popup Layouts

        /// <summary>
        /// Create a popup layout with optional content areas
        /// </summary>
        public static PopupLayout CreatePopupLayout(Transform parent, PopupConfig config = null)
        {
            config = config ?? new PopupConfig();
            
            var layout = new PopupLayout();

            // Background overlay
            layout.BackgroundOverlay = S1UIFactory.CreatePanel(parent, new Color(0, 0, 0, 0.5f));
            layout.BackgroundOverlay.name = "PopupBackgroundOverlay";
            var overlayRect = layout.BackgroundOverlay.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(overlayRect, AnchorPresets.StretchAll);

            // Main popup container
            layout.PopupContainer = S1UIFactory.CreatePanel(layout.BackgroundOverlay.transform, config.BackgroundColor);
            layout.PopupContainer.name = "PopupContainer";
            var popupRect = layout.PopupContainer.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(popupRect, AnchorPresets.MiddleCenter);
            popupRect.sizeDelta = new Vector2(config.Width, config.Height);

            // Title bar (optional)
            if (config.HasTitleBar)
            {
                layout.TitleBar = CreatePopupTitleBar(layout.PopupContainer.transform, config);
            }

            // Content area
            layout.ContentArea = CreatePopupContentArea(layout.PopupContainer.transform, config);

            // Button area (optional)
            if (config.HasButtonArea)
            {
                layout.ButtonArea = CreatePopupButtonArea(layout.PopupContainer.transform, config);
            }

            return layout;
        }

        private static GameObject CreatePopupTitleBar(Transform parent, PopupConfig config)
        {
            var titleBar = S1UIFactory.CreatePanel(parent, new Color(0.2f, 0.2f, 0.2f, 1f));
            titleBar.name = "PopupTitleBar";
            
            var titleRect = titleBar.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(titleRect, AnchorPresets.TopCenter);
            titleRect.anchoredPosition = new Vector2(0, 0);
            titleRect.sizeDelta = new Vector2(-20, config.TitleBarHeight);

            return titleBar;
        }

        private static GameObject CreatePopupContentArea(Transform parent, PopupConfig config)
        {
            var contentArea = S1UIFactory.CreatePanel(parent, Color.clear);
            contentArea.name = "PopupContentArea";
            
            var contentRect = contentArea.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(contentRect, AnchorPresets.StretchAll);
            
            var topOffset = config.HasTitleBar ? -config.TitleBarHeight : -10;
            var bottomOffset = config.HasButtonArea ? config.ButtonAreaHeight : 10;
            
            contentRect.offsetMin = new Vector2(10, bottomOffset);
            contentRect.offsetMax = new Vector2(-10, topOffset);

            return contentArea;
        }

        private static GameObject CreatePopupButtonArea(Transform parent, PopupConfig config)
        {
            var buttonArea = S1UIFactory.CreatePanel(parent, new Color(0.15f, 0.15f, 0.15f, 1f));
            buttonArea.name = "PopupButtonArea";
            
            var buttonRect = buttonArea.GetComponent<RectTransform>();
            S1UIFactory.SetAnchors(buttonRect, AnchorPresets.BottomCenter);
            buttonRect.anchoredPosition = new Vector2(0, 0);
            buttonRect.sizeDelta = new Vector2(-20, config.ButtonAreaHeight);

            return buttonArea;
        }

        #endregion

        #region Utility Layouts

        /// <summary>
        /// Create a horizontal button group
        /// </summary>
        public static ButtonGroupLayout CreateHorizontalButtonGroup(Transform parent, ButtonGroupConfig config = null)
        {
            config = config ?? new ButtonGroupConfig();
            
            var layout = new ButtonGroupLayout();

            layout.Container = S1UIFactory.CreatePanel(parent, Color.clear);
            layout.Container.name = "ButtonGroupContainer";
            var containerRect = layout.Container.GetComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(config.TotalWidth, config.ButtonHeight);

            var buttonSpacing = config.ButtonSpacing;
            var buttonWidth = (config.TotalWidth - ((config.ButtonCount - 1) * buttonSpacing)) / config.ButtonCount;

            // Default button labels
            var defaultLabels = new[] { "Button 1", "Button 2", "Button 3", "Button 4", "Button 5" };

            for (int i = 0; i < config.ButtonCount; i++)
            {
                var buttonContainer = S1UIFactory.CreatePanel(layout.Container.transform, Color.clear);
                buttonContainer.name = $"ButtonSlot_{i}";
                
                var buttonRect = buttonContainer.GetComponent<RectTransform>();
                S1UIFactory.SetAnchors(buttonRect, AnchorPresets.MiddleLeft);
                
                var xPos = (i * (buttonWidth + buttonSpacing)) + (buttonWidth / 2);
                buttonRect.anchoredPosition = new Vector2(xPos, 0);
                buttonRect.sizeDelta = new Vector2(buttonWidth, config.ButtonHeight);

                layout.ButtonSlots.Add(buttonContainer);
                
                // Create actual button
                var buttonLabel = i < defaultLabels.Length ? defaultLabels[i] : $"Button {i + 1}";
                var button = S1UIFactory.CreateButton(buttonContainer.transform, buttonLabel, 
                    null, new Vector2(buttonWidth, config.ButtonHeight));
                var actualButtonRect = button.GetComponent<RectTransform>();
                S1UIFactory.SetAnchors(actualButtonRect, AnchorPresets.StretchAll);
                actualButtonRect.offsetMin = Vector2.zero;
                actualButtonRect.offsetMax = Vector2.zero;

                layout.Buttons.Add(button);
            }

            return layout;
        }

        /// <summary>
        /// Create a list layout for scrollable content
        /// </summary>
        public static ListLayout CreateListLayout(Transform parent, ListConfig config = null)
        {
            config = config ?? new ListConfig();
            
            var layout = new ListLayout();

            // Main container
            layout.Container = S1UIFactory.CreatePanel(parent, Color.clear);
            layout.Container.name = "ListContainer";
            var containerRect = layout.Container.GetComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(config.Width, config.Height);

            // Create scroll view
            layout.ScrollView = S1UIFactory.CreateScrollView(layout.Container.transform, 
                new Vector2(config.Width, config.Height));

            // Content area is already created by CreateScrollView
            layout.ContentArea = layout.ScrollView.content.gameObject;

            return layout;
        }

        /// <summary>
        /// Create a scrollable list (alias for CreateListLayout)
        /// </summary>
        public static ScrollRect CreateScrollableList(Transform parent, Vector2? size = null)
        {
            return S1UIFactory.CreateScrollView(parent, size);
        }

        #endregion
    }
} 