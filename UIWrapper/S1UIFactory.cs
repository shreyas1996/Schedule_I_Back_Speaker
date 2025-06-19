using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using BackSpeakerMod.NewBackend.Utils;

namespace BackSpeakerMod.UIWrapper
{
    /// <summary>
    /// Factory for creating basic UI elements with Schedule One compatibility
    /// Handles core Unity UI element creation with consistent styling
    /// </summary>
    public static class S1UIFactory
    {
        #region Basic UI Elements

        /// <summary>
        /// Create a button with text and click handler
        /// </summary>
        public static Button CreateButton(Transform parent, string text, UnityAction onClick = null, Vector2? size = null)
        {
            try
            {
                // Create button GameObject
                var buttonObj = new GameObject("Button");
                buttonObj.transform.SetParent(parent, false);
                
                var rectTransform = buttonObj.AddComponent<RectTransform>();
                rectTransform.sizeDelta = size ?? new Vector2(100, 30);
                
                var button = buttonObj.AddComponent<Button>();
                var image = buttonObj.AddComponent<Image>();
                
                // Set button colors
                var colors = button.colors;
                colors.normalColor = new Color(0.2f, 0.6f, 1f, 0.8f);
                colors.highlightedColor = new Color(0.3f, 0.7f, 1f, 1f);
                colors.pressedColor = new Color(0.1f, 0.5f, 0.9f, 1f);
                button.colors = colors;
                button.targetGraphic = image;
                
                // Add text
                var textObj = new GameObject("Text");
                textObj.transform.SetParent(buttonObj.transform, false);
                
                var textComponent = textObj.AddComponent<Text>();
                textComponent.text = text;
                textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                textComponent.fontSize = 12;
                textComponent.color = Color.white;
                textComponent.alignment = TextAnchor.MiddleCenter;
                
                var textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                
                // Set click handler
                if (onClick != null)
                {
                    button.onClick.AddListener(onClick);
                }
                
                return button;
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to create button: {ex.Message}", "S1UIFactory");
                return null;
            }
        }

        /// <summary>
        /// Create a text label with customizable styling
        /// </summary>
        public static Text CreateText(Transform parent, string text, int fontSize = 14, Color? color = null, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(parent, false);

            var rectTransform = textObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 30);

            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = GetDefaultFont();
            textComponent.fontSize = fontSize;
            textComponent.color = color ?? Color.white;
            textComponent.alignment = alignment;

            return textComponent;
        }

        /// <summary>
        /// Create a panel with background
        /// </summary>
        public static GameObject CreatePanel(Transform parent, Color backgroundColor, Vector2? size = null)
        {
            var panelObj = new GameObject("Panel");
            panelObj.transform.SetParent(parent, false);

            var rectTransform = panelObj.AddComponent<RectTransform>();
            if (size.HasValue)
            {
                rectTransform.sizeDelta = size.Value;
            }
            else
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }

            var image = panelObj.AddComponent<Image>();
            image.color = backgroundColor;

            return panelObj;
        }

        /// <summary>
        /// Create an input field with placeholder
        /// </summary>
        public static InputField CreateInputField(Transform parent, string placeholder = "", Vector2? size = null)
        {
            var inputObj = new GameObject("InputField");
            inputObj.transform.SetParent(parent, false);

            var rectTransform = inputObj.AddComponent<RectTransform>();
            if (size.HasValue)
            {
                rectTransform.sizeDelta = size.Value;
            }
            else
            {
                rectTransform.sizeDelta = new Vector2(200, 30);
            }

            var image = inputObj.AddComponent<Image>();
            image.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

            var inputField = inputObj.AddComponent<InputField>();

            // Create text area
            var textAreaObj = new GameObject("Text Area");
            textAreaObj.transform.SetParent(inputObj.transform, false);
            var textAreaRect = textAreaObj.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(10, 6);
            textAreaRect.offsetMax = new Vector2(-10, -7);

            // Create placeholder
            var placeholderText = CreateText(textAreaObj.transform, placeholder, 14, new Color(0.5f, 0.5f, 0.5f, 1f));
            placeholderText.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            placeholderText.GetComponent<RectTransform>().anchorMax = Vector2.one;
            placeholderText.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            placeholderText.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            // Create actual text
            var actualText = CreateText(textAreaObj.transform, "", 14, Color.white);
            actualText.GetComponent<RectTransform>().anchorMin = Vector2.zero;
            actualText.GetComponent<RectTransform>().anchorMax = Vector2.one;
            actualText.GetComponent<RectTransform>().offsetMin = Vector2.zero;
            actualText.GetComponent<RectTransform>().offsetMax = Vector2.zero;

            inputField.textComponent = actualText;
            inputField.placeholder = placeholderText;

            return inputField;
        }

        /// <summary>
        /// Create a scroll view with content area
        /// </summary>
        public static ScrollRect CreateScrollView(Transform parent, Vector2? size = null)
        {
            var scrollObj = new GameObject("ScrollView");
            scrollObj.transform.SetParent(parent, false);

            var rectTransform = scrollObj.AddComponent<RectTransform>();
            if (size.HasValue)
            {
                rectTransform.sizeDelta = size.Value;
            }
            else
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.offsetMin = Vector2.zero;
                rectTransform.offsetMax = Vector2.zero;
            }

            var scrollRect = scrollObj.AddComponent<ScrollRect>();
            var image = scrollObj.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            // Create viewport
            var viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(scrollObj.transform, false);
            var viewportRect = viewportObj.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            var viewportMask = viewportObj.AddComponent<Mask>();
            viewportMask.showMaskGraphic = false;
            var viewportImage = viewportObj.AddComponent<Image>();
            viewportImage.color = Color.clear;

            // Create content
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            var contentRect = contentObj.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1);
            contentRect.sizeDelta = new Vector2(0, 300);

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            return scrollRect;
        }

        /// <summary>
        /// Create a slider with proper styling
        /// </summary>
        public static Slider CreateSlider(Transform parent, float minValue = 0f, float maxValue = 1f, float currentValue = 0.5f, UnityAction<float> onValueChanged = null)
        {
            var sliderObj = new GameObject("Slider");
            sliderObj.transform.SetParent(parent, false);
            
            var rectTransform = sliderObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 20);
            
            var slider = sliderObj.AddComponent<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = currentValue;
            
            if (onValueChanged != null)
                slider.onValueChanged.AddListener(onValueChanged);
            
            // Create background
            var backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(sliderObj.transform, false);
            var backgroundImage = backgroundObj.AddComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            var backgroundRect = backgroundObj.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            
            // Create fill area
            var fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            var fillAreaRect = fillAreaObj.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;
            
            // Create fill
            var fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            var fillImage = fillObj.AddComponent<Image>();
            fillImage.color = new Color(0.3f, 0.6f, 1f, 0.8f);
            fillImage.type = Image.Type.Filled;
            var fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            
            // Create handle slide area
            var handleSlideAreaObj = new GameObject("Handle Slide Area");
            handleSlideAreaObj.transform.SetParent(sliderObj.transform, false);
            var handleSlideAreaRect = handleSlideAreaObj.GetComponent<RectTransform>();
            handleSlideAreaRect.anchorMin = Vector2.zero;
            handleSlideAreaRect.anchorMax = Vector2.one;
            handleSlideAreaRect.offsetMin = new Vector2(10, 0);
            handleSlideAreaRect.offsetMax = new Vector2(-10, 0);
            
            // Create handle
            var handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(handleSlideAreaObj.transform, false);
            var handleImage = handleObj.AddComponent<Image>();
            handleImage.color = Color.white;
            var handleRect = handleObj.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(20, 20);
            
            // Assign references
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
            
            return slider;
        }

        /// <summary>
        /// Create a toggle with proper styling
        /// </summary>
        public static Toggle CreateToggle(Transform parent, string labelText = "", bool isOn = false, UnityAction<bool> onValueChanged = null)
        {
            var toggleObj = new GameObject("Toggle");
            toggleObj.transform.SetParent(parent, false);
            
            var rectTransform = toggleObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(150, 20);
            
            var toggle = toggleObj.AddComponent<Toggle>();
            toggle.isOn = isOn;
            
            if (onValueChanged != null)
                toggle.onValueChanged.AddListener(onValueChanged);
            
            // Create background
            var backgroundObj = new GameObject("Background");
            backgroundObj.transform.SetParent(toggleObj.transform, false);
            var backgroundImage = backgroundObj.AddComponent<Image>();
            backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            var backgroundRect = backgroundObj.GetComponent<RectTransform>();
            backgroundRect.anchorMin = new Vector2(0, 0.5f);
            backgroundRect.anchorMax = new Vector2(0, 0.5f);
            backgroundRect.anchoredPosition = new Vector2(10, 0);
            backgroundRect.sizeDelta = new Vector2(20, 20);
            
            // Create checkmark
            var checkmarkObj = new GameObject("Checkmark");
            checkmarkObj.transform.SetParent(backgroundObj.transform, false);
            var checkmarkImage = checkmarkObj.AddComponent<Image>();
            checkmarkImage.color = new Color(0.3f, 0.6f, 1f, 1f);
            var checkmarkRect = checkmarkObj.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = Vector2.zero;
            checkmarkRect.anchorMax = Vector2.one;
            checkmarkRect.offsetMin = Vector2.zero;
            checkmarkRect.offsetMax = Vector2.zero;
            
            // Create label
            if (!string.IsNullOrEmpty(labelText))
            {
                var labelObj = new GameObject("Label");
                labelObj.transform.SetParent(toggleObj.transform, false);
                var labelComponent = CreateText(labelObj.transform, labelText, 14, Color.white);
                var labelRect = labelObj.GetComponent<RectTransform>();
                labelRect.anchorMin = new Vector2(0, 0);
                labelRect.anchorMax = new Vector2(1, 1);
                labelRect.offsetMin = new Vector2(35, 0);
                labelRect.offsetMax = Vector2.zero;
            }
            
            // Assign references
            toggle.targetGraphic = backgroundImage;
            toggle.graphic = checkmarkImage;
            
            return toggle;
        }

        /// <summary>
        /// Create a dropdown with proper styling
        /// </summary>
        public static Dropdown CreateDropdown(Transform parent, System.Collections.Generic.List<string> options = null, int selectedIndex = 0, UnityAction<int> onValueChanged = null)
        {
            var dropdownObj = new GameObject("Dropdown");
            dropdownObj.transform.SetParent(parent, false);
            
            var rectTransform = dropdownObj.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(160, 30);
            
            var backgroundImage = dropdownObj.AddComponent<Image>();
            backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            var dropdown = dropdownObj.AddComponent<Dropdown>();
            
            // Create label
            var labelObj = new GameObject("Label");
            labelObj.transform.SetParent(dropdownObj.transform, false);
            var labelComponent = CreateText(labelObj.transform, "Option", 14, Color.white);
            var labelRect = labelObj.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(10, 2);
            labelRect.offsetMax = new Vector2(-25, -2);
            
            // Create arrow
            var arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(dropdownObj.transform, false);
            var arrowImage = arrowObj.AddComponent<Image>();
            arrowImage.color = Color.white;
            var arrowRect = arrowObj.GetComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1, 0.5f);
            arrowRect.anchorMax = new Vector2(1, 0.5f);
            arrowRect.anchoredPosition = new Vector2(-15, 0);
            arrowRect.sizeDelta = new Vector2(20, 20);
            
            // Create template
            var templateObj = new GameObject("Template");
            templateObj.transform.SetParent(dropdownObj.transform, false);
            templateObj.SetActive(false);
            var templateImage = templateObj.AddComponent<Image>();
            templateImage.color = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            var templateRect = templateObj.GetComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0, 0);
            templateRect.anchorMax = new Vector2(1, 0);
            templateRect.anchoredPosition = new Vector2(0, 2);
            templateRect.sizeDelta = new Vector2(0, 150);
            
            // Create viewport
            var viewportObj = new GameObject("Viewport");
            viewportObj.transform.SetParent(templateObj.transform, false);
            var viewportRect = viewportObj.GetComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            
            // Create content
            var contentObj = new GameObject("Content");
            contentObj.transform.SetParent(viewportObj.transform, false);
            var contentRect = contentObj.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0, 28);
            
            // Create item
            var itemObj = new GameObject("Item");
            itemObj.transform.SetParent(contentObj.transform, false);
            var itemToggle = itemObj.AddComponent<Toggle>();
            var itemRect = itemObj.GetComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0, 0.5f);
            itemRect.anchorMax = new Vector2(1, 0.5f);
            itemRect.anchoredPosition = Vector2.zero;
            itemRect.sizeDelta = new Vector2(0, 20);
            
            // Create item background
            var itemBackgroundObj = new GameObject("Item Background");
            itemBackgroundObj.transform.SetParent(itemObj.transform, false);
            var itemBackgroundImage = itemBackgroundObj.AddComponent<Image>();
            itemBackgroundImage.color = new Color(0.25f, 0.25f, 0.25f, 0.8f);
            var itemBackgroundRect = itemBackgroundObj.GetComponent<RectTransform>();
            itemBackgroundRect.anchorMin = Vector2.zero;
            itemBackgroundRect.anchorMax = Vector2.one;
            itemBackgroundRect.offsetMin = Vector2.zero;
            itemBackgroundRect.offsetMax = Vector2.zero;
            
            // Create item checkmark
            var itemCheckmarkObj = new GameObject("Item Checkmark");
            itemCheckmarkObj.transform.SetParent(itemObj.transform, false);
            var itemCheckmarkImage = itemCheckmarkObj.AddComponent<Image>();
            itemCheckmarkImage.color = new Color(0.3f, 0.6f, 1f, 1f);
            var itemCheckmarkRect = itemCheckmarkObj.GetComponent<RectTransform>();
            itemCheckmarkRect.anchorMin = new Vector2(0, 0.5f);
            itemCheckmarkRect.anchorMax = new Vector2(0, 0.5f);
            itemCheckmarkRect.anchoredPosition = new Vector2(10, 0);
            itemCheckmarkRect.sizeDelta = new Vector2(10, 10);
            
            // Create item label
            var itemLabelObj = new GameObject("Item Label");
            itemLabelObj.transform.SetParent(itemObj.transform, false);
            var itemLabelComponent = CreateText(itemLabelObj.transform, "Option A", 14, Color.white);
            var itemLabelRect = itemLabelObj.GetComponent<RectTransform>();
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.offsetMin = new Vector2(20, 1);
            itemLabelRect.offsetMax = new Vector2(-10, -2);
            
            // Setup toggle references
            itemToggle.targetGraphic = itemBackgroundImage;
            itemToggle.graphic = itemCheckmarkImage;
            
            // Assign dropdown references
            dropdown.targetGraphic = backgroundImage;
            dropdown.captionText = labelComponent;
            dropdown.template = templateRect;
            dropdown.itemText = itemLabelComponent;
            
            // Set dropdown options
            if (options != null && options.Count > 0)
            {
#if IL2CPP
                var dropdownOptions = new Il2CppSystem.Collections.Generic.List<Dropdown.OptionData>();
                foreach (var option in options)
                {
                    dropdownOptions.Add(new Dropdown.OptionData(option));
                }
                dropdown.options = dropdownOptions;
#else
                var dropdownOptions = new System.Collections.Generic.List<Dropdown.OptionData>();
                foreach (var option in options)
                {
                    dropdownOptions.Add(new Dropdown.OptionData(option));
                }
                dropdown.options = dropdownOptions;
#endif
            }
            
            if (onValueChanged != null)
                dropdown.onValueChanged.AddListener(onValueChanged);
            
            return dropdown;
        }

        /// <summary>
        /// Create a scrollable list layout
        /// </summary>
        public static ScrollRect CreateScrollableList(Transform parent, Vector2? size = null)
        {
            return CreateScrollView(parent, size);
        }

        #endregion

        #region Styling

        private static void ApplyButtonStyle(Button button, Image image, ButtonStyle style)
        {
            ColorBlock colors = ColorBlock.defaultColorBlock;

            switch (style)
            {
                case ButtonStyle.Default:
                    colors.normalColor = new Color(0.25f, 0.25f, 0.25f, 0.8f);
                    colors.highlightedColor = new Color(0.35f, 0.35f, 0.35f, 0.8f);
                    colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);
                    break;
                case ButtonStyle.Primary:
                    colors.normalColor = new Color(0.2f, 0.6f, 1f, 0.8f);
                    colors.highlightedColor = new Color(0.3f, 0.7f, 1f, 0.8f);
                    colors.pressedColor = new Color(0.1f, 0.5f, 0.9f, 0.8f);
                    break;
                case ButtonStyle.Success:
                    colors.normalColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
                    colors.highlightedColor = new Color(0.3f, 0.9f, 0.3f, 0.8f);
                    colors.pressedColor = new Color(0.1f, 0.7f, 0.1f, 0.8f);
                    break;
                case ButtonStyle.Warning:
                    colors.normalColor = new Color(1f, 0.8f, 0.2f, 0.8f);
                    colors.highlightedColor = new Color(1f, 0.9f, 0.3f, 0.8f);
                    colors.pressedColor = new Color(0.9f, 0.7f, 0.1f, 0.8f);
                    break;
                case ButtonStyle.Danger:
                    colors.normalColor = new Color(0.8f, 0.2f, 0.2f, 0.8f);
                    colors.highlightedColor = new Color(0.9f, 0.3f, 0.3f, 0.8f);
                    colors.pressedColor = new Color(0.7f, 0.1f, 0.1f, 0.8f);
                    break;
            }

            button.colors = colors;
        }

        private static Text CreateButtonText(Transform buttonParent, string text, ButtonStyle style)
        {
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonParent, false);
            
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = GetDefaultFont();
            textComponent.fontSize = 14;
            textComponent.color = GetTextColorForButtonStyle(style);
            textComponent.alignment = TextAnchor.MiddleCenter;

            return textComponent;
        }

        private static Color GetTextColorForButtonStyle(ButtonStyle style)
        {
            switch (style)
            {
                case ButtonStyle.Warning:
                    return Color.black;
                default:
                    return Color.white;
            }
        }

        private static Font GetDefaultFont()
        {
            return Resources.GetBuiltinResource<Font>("Arial.ttf");
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Find child by name recursively
        /// </summary>
        public static Transform? FindChildRecursive(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;

                var found = FindChildRecursive(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        /// <summary>
        /// Create a simple colored sprite
        /// </summary>
        public static Sprite CreateColoredSprite(Color color, int width = 64, int height = 64)
        {
            var texture = new Texture2D(width, height);
            var pixels = new Color[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
            texture.SetPixels(pixels);
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
        }

        /// <summary>
        /// Set anchors for common UI layouts
        /// </summary>
        public static void SetAnchors(RectTransform rectTransform, AnchorPresets preset)
        {
            switch (preset)
            {
                case AnchorPresets.TopLeft:
                    rectTransform.anchorMin = new Vector2(0, 1);
                    rectTransform.anchorMax = new Vector2(0, 1);
                    break;
                case AnchorPresets.TopCenter:
                    rectTransform.anchorMin = new Vector2(0.5f, 1);
                    rectTransform.anchorMax = new Vector2(0.5f, 1);
                    break;
                case AnchorPresets.TopRight:
                    rectTransform.anchorMin = new Vector2(1, 1);
                    rectTransform.anchorMax = new Vector2(1, 1);
                    break;
                case AnchorPresets.MiddleLeft:
                    rectTransform.anchorMin = new Vector2(0, 0.5f);
                    rectTransform.anchorMax = new Vector2(0, 0.5f);
                    break;
                case AnchorPresets.MiddleCenter:
                    rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    break;
                case AnchorPresets.MiddleRight:
                    rectTransform.anchorMin = new Vector2(1, 0.5f);
                    rectTransform.anchorMax = new Vector2(1, 0.5f);
                    break;
                case AnchorPresets.BottomLeft:
                    rectTransform.anchorMin = new Vector2(0, 0);
                    rectTransform.anchorMax = new Vector2(0, 0);
                    break;
                case AnchorPresets.BottomCenter:
                    rectTransform.anchorMin = new Vector2(0.5f, 0);
                    rectTransform.anchorMax = new Vector2(0.5f, 0);
                    break;
                case AnchorPresets.BottomRight:
                    rectTransform.anchorMin = new Vector2(1, 0);
                    rectTransform.anchorMax = new Vector2(1, 0);
                    break;
                case AnchorPresets.StretchAll:
                    rectTransform.anchorMin = Vector2.zero;
                    rectTransform.anchorMax = Vector2.one;
                    rectTransform.offsetMin = Vector2.zero;
                    rectTransform.offsetMax = Vector2.zero;
                    break;
            }
        }

        #endregion
    }

    #region Enums

    public enum ButtonStyle
    {
        Default,
        Primary,
        Success,
        Warning,
        Danger
    }

    public enum AnchorPresets
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
        StretchAll
    }

    #endregion
} 