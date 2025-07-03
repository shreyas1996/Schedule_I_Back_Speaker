using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using BackSpeakerMod.NewBackend.Utils;
using BackSpeakerMod.S1Wrapper;

namespace BackSpeakerMod.NewFrontend.UI
{
    /// <summary>
    /// Modern UI Factory for BackSpeaker Music App
    /// Creates styled components with music app aesthetics
    /// </summary>
    public static class ModernUIFactory
    {
        #region Color Scheme
        
        public static class Colors
        {
            public static readonly Color Primary = new Color(0.1f, 0.8f, 0.4f, 1f);      // Spotify Green
            public static readonly Color PrimaryDark = new Color(0.08f, 0.6f, 0.3f, 1f); // Darker Spotify Green
            public static readonly Color Secondary = new Color(0.2f, 0.2f, 0.2f, 1f);    // Dark Gray
            public static readonly Color CardBackground = new Color(0.15f, 0.15f, 0.15f, 1f); // Card Background
            public static readonly Color Background = new Color(0.08f, 0.08f, 0.1f, 1f); // Very Dark
            public static readonly Color Surface = new Color(0.15f, 0.15f, 0.18f, 1f);   // Card Background
            public static readonly Color TextPrimary = new Color(1f, 1f, 1f, 1f);        // White
            public static readonly Color TextSecondary = new Color(0.7f, 0.7f, 0.7f, 1f);// Light Gray
            public static readonly Color TextMuted = new Color(0.5f, 0.5f, 0.5f, 1f);    // Muted Gray
            public static readonly Color Accent = new Color(1f, 0.3f, 0.3f, 1f);         // Red for buttons
            public static readonly Color Success = new Color(0.2f, 0.8f, 0.2f, 1f);      // Green
            public static readonly Color Warning = new Color(1f, 0.8f, 0.2f, 1f);        // Orange
        }
        
        #endregion
        
        #region Basic Components
        
        /// <summary>
        /// Create a modern styled button with rounded corners effect
        /// </summary>
        public static Button CreateModernButton(Transform parent, string text, UnityAction? onClick = null, 
            ButtonStyle style = ButtonStyle.Primary, Vector2? size = null)
        {
            var buttonObj = new GameObject($"ModernButton_{text.Replace(" ", "")}");
            buttonObj.transform.SetParent(parent, false);
            
            var rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size ?? new Vector2(140, 50); // Larger default size
            
            var button = buttonObj.AddComponent<Button>();
            var image = buttonObj.AddComponent<Image>();
            
            // Style based on button type
            ApplyModernButtonStyle(button, image, style);
            
            // Create text
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            
            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = GetModernFont();
            textComponent.fontSize = 16; // Larger font size
            textComponent.fontStyle = FontStyle.Bold;
            textComponent.color = GetTextColorForStyle(style);
            textComponent.alignment = TextAnchor.MiddleCenter;
            
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 0); // Add padding
            textRect.offsetMax = new Vector2(-10, 0);
            
            if (onClick != null)
                button.onClick.AddListener(onClick);
            
            return button;
        }
        
        /// <summary>
        /// Create a card-style panel with modern styling
        /// </summary>
        public static GameObject CreateCard(Transform parent, Vector2? size = null, bool withShadow = true)
        {
            var cardObj = new GameObject("Card");
            cardObj.transform.SetParent(parent, false);
            
            var rectTransform = cardObj.AddComponent<RectTransform>();
            if (size.HasValue)
                rectTransform.sizeDelta = size.Value;
            
            var image = cardObj.AddComponent<Image>();
            image.color = Colors.Surface;
            
            // Add subtle shadow effect by creating a background
            if (withShadow)
            {
                var shadowObj = new GameObject("Shadow");
                shadowObj.transform.SetParent(cardObj.transform, false);
                shadowObj.transform.SetAsFirstSibling();
                
                var shadowRect = shadowObj.AddComponent<RectTransform>();
                shadowRect.anchorMin = Vector2.zero;
                shadowRect.anchorMax = Vector2.one;
                shadowRect.offsetMin = new Vector2(2, -2);
                shadowRect.offsetMax = new Vector2(2, -2);
                
                var shadowImage = shadowObj.AddComponent<Image>();
                shadowImage.color = new Color(0, 0, 0, 0.3f);
            }
            
            return cardObj;
        }
        
        /// <summary>
        /// Create modern text with proper styling
        /// </summary>
        public static Text CreateModernText(Transform parent, string text, int fontSize = 14, 
            TextStyle style = TextStyle.Primary, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var textObj = new GameObject("ModernText");
            textObj.transform.SetParent(parent, false);
            
            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = GetModernFont();
            textComponent.fontSize = fontSize;
            textComponent.color = GetTextColorForTextStyle(style);
            textComponent.alignment = alignment;
            
            return textComponent;
        }
        
                /// <summary>
        /// Create a modern slider for volume/progress controls with circular knob
        /// </summary>
        public static Slider CreateModernSlider(Transform parent, float minValue = 0f, float maxValue = 1f, 
            float currentValue = 0.5f, UnityAction<float>? onValueChanged = null)
        {
            var sliderObj = new GameObject("ModernSlider");
            sliderObj.transform.SetParent(parent, false);
            
            var rectTransform = sliderObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(250, 30); // Larger for better touch
            
            var slider = sliderObj.AddComponent<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = currentValue;
            
            // Background Track (Full width)
            var backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(sliderObj.transform, false);
            var bgRect = backgroundObj.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0.5f);
            bgRect.anchorMax = new Vector2(1, 0.5f);
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = new Vector2(0, 4); // Thin track
            
            var bgImage = backgroundObj.AddComponent<Image>();
            bgImage.color = new Color(0.4f, 0.4f, 0.4f, 1f); // Gray track
            
            // Fill Area (for progress indication)
            var fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            var fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0.5f);
            fillAreaRect.anchorMax = new Vector2(1, 0.5f);
            fillAreaRect.pivot = new Vector2(0.5f, 0.5f);
            fillAreaRect.anchoredPosition = Vector2.zero;
            fillAreaRect.sizeDelta = new Vector2(-20, 4); // Account for handle size
            
            // Fill (Progress bar)
            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            var fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.anchoredPosition = Vector2.zero;
            fillRect.sizeDelta = Vector2.zero;
            
            var fillImage = fillObj.AddComponent<Image>();
            fillImage.color = Colors.Primary; // Green progress
            
            // Handle Slide Area
            var handleSlideAreaObj = new GameObject("Handle Slide Area");
            handleSlideAreaObj.transform.SetParent(sliderObj.transform, false);
            var handleSlideRect = handleSlideAreaObj.AddComponent<RectTransform>();
            handleSlideRect.anchorMin = Vector2.zero;
            handleSlideRect.anchorMax = Vector2.one;
            handleSlideRect.offsetMin = new Vector2(15, 0); // Handle padding
            handleSlideRect.offsetMax = new Vector2(-15, 0);
            
            // Handle (Circular knob)
            var handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(handleSlideAreaObj.transform, false);
            var handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(24, 24); // Circular size
            
            var handleImage = handleObj.AddComponent<Image>();
            handleImage.color = Colors.Primary;
            
            // Make handle appear circular by using a simple circle texture approach
            // Since we can't easily create circular images, we'll use a square that looks round
            var handleButton = handleObj.AddComponent<Button>();
            handleButton.targetGraphic = handleImage;
            handleButton.transition = Selectable.Transition.ColorTint;
            var handleColors = handleButton.colors;
            handleColors.normalColor = Colors.Primary;
            handleColors.highlightedColor = new Color(Colors.Primary.r * 1.2f, Colors.Primary.g * 1.2f, Colors.Primary.b * 1.2f, 1f);
            handleColors.pressedColor = new Color(Colors.Primary.r * 0.8f, Colors.Primary.g * 0.8f, Colors.Primary.b * 0.8f, 1f);
            handleButton.colors = handleColors;
            
            // Setup slider references
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            
            if (onValueChanged != null)
                slider.onValueChanged.AddListener(onValueChanged);
            
            return slider;
        }
        
        /// <summary>
        /// Create an icon button (for play/pause/skip controls)
        /// </summary>
        public static Button CreateIconButton(Transform parent, string iconText, UnityAction? onClick = null, 
            Vector2? size = null, bool isCircular = true)
        {
            var size2 = size ?? new Vector2(50, 50);
            var button = CreateModernButton(parent, iconText, onClick, ButtonStyle.Icon, size2);
            
            if (isCircular)
            {
                // Make it circular by adjusting the image (pseudo-circular effect)
                var image = button.GetComponent<Image>();
                if (image != null)
                {
                    image.color = Colors.Primary;
                }
            }
            
            return button;
        }
        
        #endregion
        
        #region Layout Helpers
        
        /// <summary>
        /// Create a horizontal layout container
        /// </summary>
        public static GameObject CreateHorizontalLayout(Transform parent, int spacing = 10, 
            RectOffset? padding = null, bool controlSizes = true)
        {
            var layoutObj = new GameObject("HorizontalLayout");
            layoutObj.transform.SetParent(parent, false);
            
            var rectTransform = layoutObj.AddComponent<RectTransform>();
            var layout = layoutObj.AddComponent<HorizontalLayoutGroup>();
            
            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset(10, 10, 10, 10);
            layout.childControlHeight = controlSizes;
            layout.childControlWidth = controlSizes;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            
            return layoutObj;
        }
        
        /// <summary>
        /// Create a vertical layout container
        /// </summary>
        public static GameObject CreateVerticalLayout(Transform parent, int spacing = 10, 
            RectOffset? padding = null, bool controlSizes = true)
        {
            var layoutObj = new GameObject("VerticalLayout");
            layoutObj.transform.SetParent(parent, false);
            
            var rectTransform = layoutObj.AddComponent<RectTransform>();
            var layout = layoutObj.AddComponent<VerticalLayoutGroup>();
            
            layout.spacing = spacing;
            layout.padding = padding ?? new RectOffset(10, 10, 10, 10);
            layout.childControlHeight = controlSizes;
            layout.childControlWidth = controlSizes;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
            
            return layoutObj;
        }
        
        #endregion
        
        #region Advanced Components
        
        /// <summary>
        /// Create a collapsible section with modern styling
        /// </summary>
        public static CollapsibleSection CreateCollapsibleSection(Transform parent, string title, bool startExpanded = false)
        {
            var sectionObj = new GameObject($"CollapsibleSection_{title.Replace(" ", "")}");
            sectionObj.transform.SetParent(parent, false);
            
            var collapsible = S1Factory.RegisterAndAddComponent<CollapsibleSection>(sectionObj);
            collapsible?.Initialize(title, startExpanded);
            
            return collapsible!;
        }
        
        /// <summary>
        /// Create a hamburger menu button with proper icon
        /// </summary>
        public static Button CreateHamburgerButton(Transform parent, UnityAction? onClick = null)
        {
            return CreateIconButton(parent, "≡", onClick, new Vector2(50, 50), false);
        }
        
        /// <summary>
        /// Create an album art display
        /// </summary>
        public static Image CreateAlbumArt(Transform parent, Vector2? size = null)
        {
            var artObj = new GameObject("AlbumArt");
            artObj.transform.SetParent(parent, false);
            
            var rectTransform = artObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = size ?? new Vector2(200, 200);
            
            var image = artObj.AddComponent<Image>();
            image.color = Colors.Secondary; // Default background
            
            return image;
        }
        
        #endregion
        
        #region Style Application
        
        private static void ApplyModernButtonStyle(Button button, Image image, ButtonStyle style)
        {
            var colors = button.colors;
            
            switch (style)
            {
                case ButtonStyle.Primary:
                    colors.normalColor = Colors.Primary;
                    colors.highlightedColor = new Color(Colors.Primary.r * 1.2f, Colors.Primary.g * 1.2f, Colors.Primary.b * 1.2f, 1f);
                    colors.pressedColor = new Color(Colors.Primary.r * 0.8f, Colors.Primary.g * 0.8f, Colors.Primary.b * 0.8f, 1f);
                    break;
                case ButtonStyle.Secondary:
                    colors.normalColor = Colors.Secondary;
                    colors.highlightedColor = new Color(Colors.Secondary.r * 1.3f, Colors.Secondary.g * 1.3f, Colors.Secondary.b * 1.3f, 1f);
                    colors.pressedColor = new Color(Colors.Secondary.r * 0.7f, Colors.Secondary.g * 0.7f, Colors.Secondary.b * 0.7f, 1f);
                    break;
                case ButtonStyle.Icon:
                    colors.normalColor = Colors.Primary;
                    colors.highlightedColor = new Color(Colors.Primary.r, Colors.Primary.g, Colors.Primary.b, 0.8f);
                    colors.pressedColor = new Color(Colors.Primary.r * 0.9f, Colors.Primary.g * 0.9f, Colors.Primary.b * 0.9f, 1f);
                    break;
                case ButtonStyle.Danger:
                    colors.normalColor = Colors.Accent;
                    colors.highlightedColor = new Color(Colors.Accent.r * 1.2f, Colors.Accent.g * 1.2f, Colors.Accent.b * 1.2f, 1f);
                    colors.pressedColor = new Color(Colors.Accent.r * 0.8f, Colors.Accent.g * 0.8f, Colors.Accent.b * 0.8f, 1f);
                    break;
                case ButtonStyle.Ghost:
                    colors.normalColor = Color.clear;
                    colors.highlightedColor = new Color(Colors.Primary.r, Colors.Primary.g, Colors.Primary.b, 0.1f);
                    colors.pressedColor = new Color(Colors.Primary.r, Colors.Primary.g, Colors.Primary.b, 0.2f);
                    break;
            }
            
            button.colors = colors;
            button.targetGraphic = image;
        }
        
        private static Color GetTextColorForStyle(ButtonStyle style)
        {
            switch (style)
            {
                case ButtonStyle.Primary:
                case ButtonStyle.Icon:
                case ButtonStyle.Danger:
                case ButtonStyle.Ghost:
                    return Colors.TextPrimary;
                case ButtonStyle.Secondary:
                    return Colors.TextSecondary;
                default:
                    return Colors.TextPrimary;
            }
        }
        
        private static Color GetTextColorForTextStyle(TextStyle style)
        {
            switch (style)
            {
                case TextStyle.Primary:
                    return Colors.TextPrimary;
                case TextStyle.Secondary:
                    return Colors.TextSecondary;
                case TextStyle.Muted:
                    return Colors.TextMuted;
                default:
                    return Colors.TextPrimary;
            }
        }
        
        private static Font GetModernFont()
        {
            // Try to get a better font, fallback to default
            var modernFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            return modernFont ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
        }
        
        /// <summary>
        /// Create a modern input field
        /// </summary>
        public static InputField CreateModernInputField(Transform parent, string placeholder = "", 
            Vector2? size = null)
        {
            var inputFieldObj = new GameObject("ModernInputField");
            inputFieldObj.transform.SetParent(parent, false);
            
            var image = S1Factory.RegisterAndAddComponent<Image>(inputFieldObj);
            if (image != null)
            {
                image.color = Colors.CardBackground;
            }
            
            var inputField = S1Factory.RegisterAndAddComponent<InputField>(inputFieldObj);
            if (inputField == null) return null!;
            
            // Text component for content
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(inputFieldObj.transform, false);
            var textComponent = S1Factory.RegisterAndAddComponent<Text>(textObj);
            if (textComponent != null)
            {
                textComponent.text = "";
                textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                textComponent.fontSize = 14;
                textComponent.color = Colors.TextPrimary;
            }
            
            // Placeholder
            var placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(inputFieldObj.transform, false);
            var placeholderComponent = S1Factory.RegisterAndAddComponent<Text>(placeholderObj);
            if (placeholderComponent != null)
            {
                placeholderComponent.text = placeholder;
                placeholderComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                placeholderComponent.fontSize = 14;
                placeholderComponent.color = Colors.TextSecondary;
            }
            
            inputField.textComponent = textComponent;
            inputField.placeholder = placeholderComponent;
            
            // Setup RectTransforms
            var rectTransform = inputFieldObj.GetComponent<RectTransform>();
            rectTransform.sizeDelta = size ?? new Vector2(200, 35);
            
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5, 0);
            textRect.offsetMax = new Vector2(-5, 0);
            
            var placeholderRect = placeholderObj.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(5, 0);
            placeholderRect.offsetMax = new Vector2(-5, 0);
            
            return inputField;
        }
        
        /// <summary>
        /// Create a modern toggle
        /// </summary>
        public static Toggle CreateModernToggle(Transform parent, string label = "", 
            UnityAction<bool>? onValueChanged = null, Vector2? size = null)
        {
            var toggleObj = new GameObject("ModernToggle");
            toggleObj.transform.SetParent(parent, false);
            
            var toggle = S1Factory.RegisterAndAddComponent<Toggle>(toggleObj);
            
            // Background
            var background = new GameObject("Background");
            background.transform.SetParent(toggleObj.transform, false);
            var backgroundImage = S1Factory.RegisterAndAddComponent<Image>(background);
            backgroundImage!.color = Colors.Secondary;
            
            // Checkmark
            var checkmark = new GameObject("Checkmark");
            checkmark.transform.SetParent(background.transform, false);
            var checkImage = S1Factory.RegisterAndAddComponent<Image>(checkmark);
            checkImage!.color = Colors.Primary;
            
            // Label
            if (!string.IsNullOrEmpty(label))
            {
                var labelObj = new GameObject("Label");
                labelObj.transform.SetParent(toggleObj.transform, false);
                var labelText = S1Factory.RegisterAndAddComponent<Text>(labelObj);
                labelText!.text = label;
                labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                labelText.fontSize = 14;
                labelText.color = Colors.TextPrimary;
                
                toggle.graphic = checkImage;
            }
            
            if (onValueChanged != null)
                toggle.onValueChanged.AddListener(onValueChanged);
            
            return toggle;
        }
        
        /// <summary>
        /// Create a scrollable container
        /// </summary>
        public static ScrollRect? CreateScrollableContainer(Transform parent, Vector2? size = null, bool needsContent = true)
        {
            var scrollRectObj = new GameObject("ScrollContainer");
            if (scrollRectObj == null) return null;
            
            scrollRectObj.transform.SetParent(parent, false);
            
            var scrollRect = S1Factory.RegisterAndAddComponent<ScrollRect>(scrollRectObj);
            var scrollImage = S1Factory.RegisterAndAddComponent<Image>(scrollRectObj);
            
            if (scrollImage != null)
            {
                scrollImage.color = Colors.Background;
            }
            
            if (!needsContent) return scrollRect;
            
            var contentObj = new GameObject("Content");
            if (contentObj == null) return scrollRect;
            
            if (scrollRectObj != null)
            {
                contentObj.transform.SetParent(scrollRectObj.transform, false);
            }
            var contentLayout = S1Factory.RegisterAndAddComponent<VerticalLayoutGroup>(contentObj);
            
            if (scrollRect != null)
            {
                var contentRect = contentObj?.GetComponent<RectTransform>();
                var viewportRect = scrollRectObj?.GetComponent<RectTransform>();
                
                if (contentRect != null)
                    scrollRect.content = contentRect;
                
                if (viewportRect != null)
                    scrollRect.viewport = viewportRect;
            }
            
            if (size.HasValue)
            {
                var rectTransform = scrollRectObj?.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.sizeDelta = size.Value;
                }
            }
            
            return scrollRect;
        }
        
        /// <summary>
        /// Create a dialog window
        /// </summary>
        public static GameObject? CreateDialog(Transform parent, string title, Vector2? size = null)
        {
            var overlay = new GameObject("DialogOverlay");
            overlay.transform.SetParent(parent, false);
            
            var overlayRect = S1Factory.RegisterAndAddComponent<RectTransform>(overlay);
            overlayRect!.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            
            var overlayImage = S1Factory.RegisterAndAddComponent<Image>(overlay);
            if (overlayImage != null)
            {
                overlayImage.color = new Color(0, 0, 0, 0.5f);
            }
            
            var dialog = new GameObject("Dialog");
            if (dialog != null)
            {
                dialog.transform.SetParent(overlay.transform, false);
            }
            
            var dialogRect = dialog != null ? S1Factory.RegisterAndAddComponent<RectTransform>(dialog) : null;
            dialogRect!.anchorMin = new Vector2(0.5f, 0.5f);
            dialogRect.anchorMax = new Vector2(0.5f, 0.5f);
            dialogRect.sizeDelta = size ?? new Vector2(400, 300);
            
            var dialogImage = dialog != null ? S1Factory.RegisterAndAddComponent<Image>(dialog) : null;
            if (dialogImage != null)
            {
                dialogImage.color = Colors.CardBackground;
            }
            
            // Title
            if (!string.IsNullOrEmpty(title) && dialog != null)
            {
                var titleObj = new GameObject("Title");
                titleObj.transform.SetParent(dialog.transform, false);
                var titleText = CreateModernText(titleObj.transform, title, 16, TextStyle.Primary, TextAnchor.MiddleCenter);
                if (titleText != null)
                {
                    titleText.fontStyle = FontStyle.Bold;
                }
                
                var titleRect = titleObj?.GetComponent<RectTransform>();
                if (titleRect != null)
                {
                    titleRect.anchorMin = new Vector2(0, 0.85f);
                    titleRect.anchorMax = new Vector2(1, 1);
                    titleRect.offsetMin = Vector2.zero;
                    titleRect.offsetMax = Vector2.zero;
                }
            }
            
            return dialog;
        }
        
        #endregion
        
        #region Song Context Menus
        
        /// <summary>
        /// Context menu options for song actions
        /// </summary>
        public enum SongContextAction
        {
            AddToQueue,
            AddToPlaylist,
            Download,
            RemoveFromPlaylist,
            RemoveFromQueue
        }
        
        /// <summary>
        /// Create a 3-dot context menu for song actions
        /// </summary>
        public static GameObject CreateSongContextMenu(Transform parent, NewSongDetails song, 
            System.Action<SongContextAction, NewSongDetails> onActionSelected, Vector2? position = null,
            bool showRemoveFromPlaylist = false, bool showRemoveFromQueue = false)
        {
            // Create overlay
            var overlay = new GameObject("SongContextMenuOverlay");
            overlay.transform.SetParent(parent, false);
            
            var overlayRect = overlay.AddComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            
            // Semi-transparent background
            var overlayImage = overlay.AddComponent<Image>();
            overlayImage.color = new Color(0, 0, 0, 0.3f);
            
            // Close menu when overlay is clicked
            var overlayButton = overlay.AddComponent<Button>();
            overlayButton.onClick.AddListener(() => UnityEngine.Object.DestroyImmediate(overlay));
            
            // Create menu card
            var menuCard = CreateCard(overlay.transform, new Vector2(200, 0), true);
            var menuRect = menuCard?.GetComponent<RectTransform>();
            
            // Position menu
            if (menuRect != null)
            {
                if (position.HasValue)
                {
                    menuRect.anchoredPosition = position.Value;
                }
                else
                {
                    menuRect.anchorMin = new Vector2(0.5f, 0.5f);
                    menuRect.anchorMax = new Vector2(0.5f, 0.5f);
                    menuRect.pivot = new Vector2(0.5f, 0.5f);
                }
            }
            
            // Add vertical layout to menu
            if (menuCard != null)
            {
                var menuLayout = menuCard.AddComponent<VerticalLayoutGroup>();
                if (menuLayout != null)
                {
                    menuLayout.padding = new RectOffset(5, 5, 5, 5);
                    menuLayout.spacing = 2;
                    menuLayout.childControlHeight = false;
                    menuLayout.childControlWidth = true;
                    menuLayout.childForceExpandHeight = false;
                    menuLayout.childForceExpandWidth = true;
                }
                
                var menuContentFitter = menuCard.AddComponent<ContentSizeFitter>();
                if (menuContentFitter != null)
                {
                    menuContentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                }
            }
            
            // Menu header
            Text? headerText = null;
            if (menuCard != null)
            {
                headerText = CreateModernText(menuCard.transform, song.title, 14, TextStyle.Primary, TextAnchor.MiddleCenter);
            }
            if (headerText != null)
            {
                headerText.color = Colors.Primary;
            }
            var headerLayout = headerText?.gameObject?.AddComponent<LayoutElement>();
            if (headerLayout != null)
            {
                headerLayout.preferredHeight = 25;
            }
            
            // Menu items
            var menuItems = new List<(string text, SongContextAction action)>
            {
                ("▶ Add to Queue", SongContextAction.AddToQueue),
                ("📁 Add to Playlist", SongContextAction.AddToPlaylist),
                ("📥 Download", SongContextAction.Download)
            };
            
            if (showRemoveFromPlaylist)
            {
                menuItems.Add(("🗑 Remove from Playlist", SongContextAction.RemoveFromPlaylist));
            }
            
            if (showRemoveFromQueue)
            {
                menuItems.Add(("❌ Remove from Queue", SongContextAction.RemoveFromQueue));
            }
            
            foreach (var item in menuItems)
            {
                var menuButton = CreateModernButton(menuCard?.transform, item.text, 
                    S1Factory.ConvertToUnityAction(() => {
                        onActionSelected?.Invoke(item.action, song);
                        UnityEngine.Object.DestroyImmediate(overlay);
                    }), ButtonStyle.Ghost, new Vector2(0, 35));
                
                // Style menu button
                var buttonText = menuButton.GetComponentInChildren<Text>();
                if (buttonText != null)
                {
                    buttonText.alignment = TextAnchor.MiddleLeft;
                    buttonText.fontSize = 12;
                }
            }
            
            return overlay;
        }
        
        /// <summary>
        /// Create a 3-dot icon button for triggering context menus
        /// </summary>
        public static Button CreateContextMenuButton(Transform parent, System.Action? onClick = null, Vector2? size = null)
        {
            var button = CreateIconButton(parent, "⋮", S1Factory.ConvertToUnityAction(onClick), size, false);
            
            // Style the 3-dot button
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(Colors.TextSecondary.r, Colors.TextSecondary.g, Colors.TextSecondary.b, 0.7f);
            }
            
            // Hover effect
            var buttonColors = button.colors;
            buttonColors.highlightedColor = Colors.Primary;
            buttonColors.pressedColor = Colors.PrimaryDark;
            button.colors = buttonColors;
            
            return button;
        }
        
        #endregion
    }
    
    #region Enums
    
    public enum ButtonStyle
    {
        Primary,
        Secondary,
        Icon,
        Danger,
        Ghost
    }
    
    public enum TextStyle
    {
        Primary,
        Secondary,
        Muted
    }
    
    #endregion
    
    #region Custom Components
    
    /// <summary>
    /// Collapsible section component for modern UI
    /// </summary>
    public class CollapsibleSection : MonoBehaviour
    {
        private GameObject? headerObj;
        private GameObject? contentObj;
        private Button? toggleButton;
        private Text? titleText;
        private Text? arrowText;
        private bool isExpanded;
        
        public bool IsExpanded => isExpanded;
        public Transform? ContentContainer => contentObj?.transform;
        
        public void Initialize(string title, bool startExpanded = false)
        {
            isExpanded = startExpanded;
            CreateHeader(title);
            CreateContent();
            UpdateVisibility();
        }
        
        private void CreateHeader(string title)
        {
            headerObj = new GameObject("Header");
            headerObj.transform.SetParent(this.transform, false);
            
            var headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.sizeDelta = new Vector2(0, 40);
            
            toggleButton = headerObj.AddComponent<Button>();
            var headerImage = headerObj.AddComponent<Image>();
            headerImage.color = ModernUIFactory.Colors.Surface;
            
            toggleButton.targetGraphic = headerImage;
            toggleButton.onClick.AddListener(S1Factory.ConvertToUnityAction(() => Toggle()));
            
            // Header layout
            var headerLayout = headerObj.AddComponent<HorizontalLayoutGroup>();
            headerLayout.padding = new RectOffset(10, 10, 5, 5);
            headerLayout.childControlWidth = false;
            headerLayout.childForceExpandWidth = false;
            
            // Title
            titleText = ModernUIFactory.CreateModernText(headerObj.transform, title, 16, TextStyle.Primary);
            var titleLayout = titleText.gameObject.AddComponent<LayoutElement>();
            titleLayout.flexibleWidth = 1;
            
            // Arrow
            arrowText = ModernUIFactory.CreateModernText(headerObj.transform, "▼", 14, TextStyle.Secondary);
            var arrowLayout = arrowText.gameObject.AddComponent<LayoutElement>();
            arrowLayout.preferredWidth = 20;
        }
        
        private void CreateContent()
        {
            contentObj = new GameObject("Content");
            contentObj.transform.SetParent(this.transform, false);
            
            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 0);
            contentRect.anchorMax = new Vector2(1, 0);
            contentRect.pivot = new Vector2(0.5f, 1);
            
            // Add vertical layout for content
            var contentLayout = contentObj.AddComponent<VerticalLayoutGroup>();
            contentLayout.padding = new RectOffset(10, 10, 10, 10);
            contentLayout.spacing = 5;
            
            // Add content size fitter
            var sizeFitter = contentObj.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }
        
        public void Toggle()
        {
            isExpanded = !isExpanded;
            UpdateVisibility();
        }
        
        private void UpdateVisibility()
        {
            if (contentObj != null)
                contentObj.SetActive(isExpanded);
                
            if (arrowText != null)
                arrowText.text = isExpanded ? "▲" : "▼";
        }
    }
    
    #endregion
} 