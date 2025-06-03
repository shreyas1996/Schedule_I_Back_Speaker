using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.UI.Helpers
{
    public static class UIFactory
    {
        public static Text CreateText(Transform parent, string name, string text, Vector2 anchorPosition, Vector2 size, int fontSize = 20)
        {
            var textObj = new GameObject(name);
            textObj.AddComponent<CanvasRenderer>();
            var rectTransform = textObj.AddComponent<RectTransform>();
            textObj.transform.SetParent(parent, false);
            
            // Use center anchoring for better positioning
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchorPosition;
            rectTransform.sizeDelta = size;
            
            var textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            textComponent.fontSize = fontSize;
            textComponent.color = Color.white; // White text for dark theme
            
            return textComponent;
        }
        
        public static Button CreateButton(Transform parent, string text, Vector2 anchorPosition, Vector2 size)
        {
            var buttonObj = new GameObject("Button_" + text);
            buttonObj.transform.SetParent(parent, false);
            buttonObj.transform.SetAsLastSibling();
            buttonObj.AddComponent<CanvasRenderer>();
            
            var rectTransform = buttonObj.AddComponent<RectTransform>();
            // Use center anchoring for better positioning
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchorPosition;
            rectTransform.sizeDelta = size;
            
            var image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.3f, 0.3f, 0.3f, 0.9f); // Dark gray buttons
            var button = buttonObj.AddComponent<Button>();
            button.targetGraphic = image;
            
            if (!string.IsNullOrEmpty(text))
            {
                var textObj = new GameObject("Text");
                textObj.AddComponent<RectTransform>();
                textObj.transform.SetParent(buttonObj.transform, false);
                var textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                textObj.AddComponent<CanvasRenderer>();
                var textComponent = textObj.AddComponent<Text>();
                textComponent.text = text;
                textComponent.alignment = TextAnchor.MiddleCenter;
                textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                textComponent.fontSize = 20; 
                textComponent.color = Color.white; // White text for dark theme
            }
            
            return button;
        }
        
        public static Slider CreateSlider(Transform parent, string name, Vector2 anchorPosition, Vector2 size, float minValue = 0f, float maxValue = 1f, float defaultValue = 0.5f)
        {
            // Create main slider object
            var sliderObj = new GameObject(name);
            sliderObj.AddComponent<CanvasRenderer>();
            var rectTransform = sliderObj.AddComponent<RectTransform>();
            sliderObj.transform.SetParent(parent, false);
            
            // Position the slider
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchorPosition;
            rectTransform.sizeDelta = size;
            
            // Add background image to main slider
            var background = sliderObj.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark background
            background.sprite = null; // Use solid color
            
            // Create the Slider component
            var slider = sliderObj.AddComponent<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = defaultValue;
            slider.direction = Slider.Direction.LeftToRight;
            
            // Create Handle Slide Area
            var handleSlideAreaObj = new GameObject("Handle Slide Area");
            handleSlideAreaObj.transform.SetParent(sliderObj.transform, false);
            var handleSlideAreaRect = handleSlideAreaObj.AddComponent<RectTransform>();
            
            // Handle slide area fills the entire slider
            handleSlideAreaRect.anchorMin = Vector2.zero;
            handleSlideAreaRect.anchorMax = Vector2.one;
            handleSlideAreaRect.sizeDelta = Vector2.zero;
            handleSlideAreaRect.offsetMin = Vector2.zero;
            handleSlideAreaRect.offsetMax = Vector2.zero;
            
            // Create Handle
            var handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(handleSlideAreaObj.transform, false);
            handleObj.AddComponent<CanvasRenderer>();
            var handleRect = handleObj.AddComponent<RectTransform>();
            
            // Handle positioning and size
            handleRect.anchorMin = new Vector2(0f, 0.25f);
            handleRect.anchorMax = new Vector2(0f, 0.75f);
            handleRect.sizeDelta = new Vector2(20f, 0f); // Handle width
            handleRect.offsetMin = new Vector2(-10f, 0f);
            handleRect.offsetMax = new Vector2(10f, 0f);
            
            // Handle visual styling
            var handleImage = handleObj.AddComponent<Image>();
            handleImage.color = new Color(0.8f, 0.8f, 0.8f, 1f); // Light gray handle
            handleImage.sprite = null; // Use solid color
            
            // Assign references to slider
            slider.targetGraphic = handleImage;
            slider.handleRect = handleRect;
            
            return slider;
        }

        public static void ApplyModernShadow(GameObject target, Color shadowColor, Vector2 offset = default, float blur = 2f)
        {
            if (target == null) return;
            
            try
            {
                // Create shadow container
                var shadowObj = new GameObject($"{target.name}_Shadow");
                shadowObj.transform.SetParent(target.transform.parent, false);
                shadowObj.transform.SetSiblingIndex(target.transform.GetSiblingIndex()); // Behind original
                
                // Copy the RectTransform properties
                var originalRect = target.GetComponent<RectTransform>();
                var shadowRect = shadowObj.AddComponent<RectTransform>();
                
                if (originalRect != null)
                {
                    shadowRect.anchorMin = originalRect.anchorMin;
                    shadowRect.anchorMax = originalRect.anchorMax;
                    shadowRect.anchoredPosition = originalRect.anchoredPosition + (offset != default ? offset : new Vector2(2f, -2f));
                    shadowRect.sizeDelta = originalRect.sizeDelta;
                    shadowRect.pivot = originalRect.pivot;
                }
                
                // Add shadow image
                var shadowImage = shadowObj.AddComponent<Image>();
                var originalImage = target.GetComponent<Image>();
                
                if (originalImage != null && originalImage.sprite != null)
                {
                    shadowImage.sprite = originalImage.sprite;
                }
                
                shadowImage.color = shadowColor;
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"UIFactory: Failed to apply shadow to {target.name}: {ex}", "UIFactory");
            }
        }

        public static void ApplyGlow(GameObject target, Color glowColor, float intensity = 0.5f)
        {
            if (target == null) return;
            
            try
            {
                // Create glow effect by creating multiple shadow layers
                for (int i = 0; i < 3; i++)
                {
                    var glowObj = new GameObject($"{target.name}_Glow_{i}");
                    glowObj.transform.SetParent(target.transform.parent, false);
                    glowObj.transform.SetSiblingIndex(target.transform.GetSiblingIndex());
                    
                    var originalRect = target.GetComponent<RectTransform>();
                    var glowRect = glowObj.AddComponent<RectTransform>();
                    
                    if (originalRect != null)
                    {
                        glowRect.anchorMin = originalRect.anchorMin;
                        glowRect.anchorMax = originalRect.anchorMax;
                        glowRect.anchoredPosition = originalRect.anchoredPosition;
                        glowRect.sizeDelta = originalRect.sizeDelta + Vector2.one * (i + 1) * 2f; // Expanding layers
                        glowRect.pivot = originalRect.pivot;
                    }
                    
                    var glowImage = glowObj.AddComponent<Image>();
                    var originalImage = target.GetComponent<Image>();
                    
                    if (originalImage != null && originalImage.sprite != null)
                    {
                        glowImage.sprite = originalImage.sprite;
                    }
                    
                    float layerIntensity = intensity / (i + 1); // Decreasing intensity for each layer
                    glowImage.color = new Color(glowColor.r, glowColor.g, glowColor.b, layerIntensity);
                }
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"UIFactory: Failed to apply glow to {target.name}: {ex}", "UIFactory");
            }
        }

        public static void ApplyModernBorder(GameObject target, Color borderColor, float thickness = 1f)
        {
            if (target == null) return;
            
            try
            {
                var borderObj = new GameObject($"{target.name}_Border");
                borderObj.transform.SetParent(target.transform.parent, false);
                borderObj.transform.SetSiblingIndex(target.transform.GetSiblingIndex());
                
                var originalRect = target.GetComponent<RectTransform>();
                var borderRect = borderObj.AddComponent<RectTransform>();
                
                if (originalRect != null)
                {
                    borderRect.anchorMin = originalRect.anchorMin;
                    borderRect.anchorMax = originalRect.anchorMax;
                    borderRect.anchoredPosition = originalRect.anchoredPosition;
                    borderRect.sizeDelta = originalRect.sizeDelta + Vector2.one * thickness * 2f;
                    borderRect.pivot = originalRect.pivot;
                }
                
                var borderImage = borderObj.AddComponent<Image>();
                borderImage.color = borderColor;
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"UIFactory: Failed to apply border to {target.name}: {ex}", "UIFactory");
            }
        }
    }
} 