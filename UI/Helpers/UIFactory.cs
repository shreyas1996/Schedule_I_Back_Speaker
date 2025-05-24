using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Utils;

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
            
            LoggerUtil.Info($"UIFactory: Created text '{name}' with text '{text}' at {anchorPosition} size {size}");
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
                textComponent.fontSize = 20; // Drones font size
                textComponent.color = Color.white; // White text for dark theme
            }
            
            LoggerUtil.Info($"UIFactory: Created button '{text}' at {anchorPosition} size {size}");
            return button;
        }
        
        public static Slider CreateSlider(Transform parent, string name, Vector2 anchorPosition, Vector2 size, float minValue = 0f, float maxValue = 1f, float defaultValue = 0.5f)
        {
            var sliderObj = new GameObject(name);
            sliderObj.AddComponent<CanvasRenderer>();
            var rectTransform = sliderObj.AddComponent<RectTransform>();
            sliderObj.transform.SetParent(parent, false);
            
            // Use center anchoring for better positioning
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchorPosition;
            rectTransform.sizeDelta = size;
            
            var slider = sliderObj.AddComponent<Slider>();
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = defaultValue;
            
            // Add dark theme styling to slider
            var background = sliderObj.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark background
            
            LoggerUtil.Info($"UIFactory: Created slider '{name}' at {anchorPosition} size {size}");
            return slider;
        }
    }
} 