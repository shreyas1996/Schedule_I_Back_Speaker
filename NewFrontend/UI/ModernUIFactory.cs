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
            public static readonly Color Secondary = new Color(0.2f, 0.2f, 0.2f, 1f);    // Dark Gray
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
        public static Button CreateModernButton(Transform parent, string text, UnityAction onClick = null, 
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
            float currentValue = 0.5f, UnityAction<float> onValueChanged = null)
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
        public static Button CreateIconButton(Transform parent, string iconText, UnityAction onClick = null, 
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
            RectOffset padding = null, bool controlSizes = true)
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
            RectOffset padding = null, bool controlSizes = true)
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
            collapsible.Initialize(title, startExpanded);
            
            return collapsible;
        }
        
        /// <summary>
        /// Create a hamburger menu button with proper icon
        /// </summary>
        public static Button CreateHamburgerButton(Transform parent, UnityAction onClick = null)
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
        
        #endregion
    }
    
    #region Enums
    
    public enum ButtonStyle
    {
        Primary,
        Secondary,
        Icon,
        Danger
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
        private GameObject headerObj;
        private GameObject contentObj;
        private Button toggleButton;
        private Text titleText;
        private Text arrowText;
        private bool isExpanded;
        
        public bool IsExpanded => isExpanded;
        public Transform ContentContainer => contentObj?.transform;
        
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