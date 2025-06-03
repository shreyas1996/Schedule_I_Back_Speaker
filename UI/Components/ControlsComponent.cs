using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.UI.Components
{
    public class ControlsComponent : MonoBehaviour
    {
        private BackSpeakerManager? manager;
        private Button? prevButton, playButton, nextButton;
        private Text? playButtonText;
        private Slider? volumeSlider;
        private Text? volumeText;
        
        public ControlsComponent() : base() { }
        
        public void Setup(BackSpeakerManager manager)
        {
            this.manager = manager;
            CreateControls();
            CreateVolumeControl();
        }
        
        private void CreateControls()
        {
            // Previous button
            prevButton = CreateControlButton("⏮️", 0.05f, 0.2f, (UnityEngine.Events.UnityAction)delegate() { manager?.PreviousTrack(); });
            
            // Play/pause button
            playButton = CreateControlButton("⏸️", 0.25f, 0.45f, (UnityEngine.Events.UnityAction)delegate() { manager?.TogglePlayPause(); });
            playButtonText = playButton.GetComponentInChildren<Text>();
            
            // Next button
            nextButton = CreateControlButton("⏭️", 0.5f, 0.65f, (UnityEngine.Events.UnityAction)delegate() { manager?.NextTrack(); });
        }
        
        private void CreateVolumeControl()
        {
            // Volume container - positioned to the right of controls
            var volumeContainer = new GameObject("VolumeContainer");
            volumeContainer.transform.SetParent(this.transform, false);
            
            var volumeRect = volumeContainer.AddComponent<RectTransform>();
            volumeRect.anchorMin = new Vector2(0.7f, 0.3f);
            volumeRect.anchorMax = new Vector2(0.98f, 0.7f);
            volumeRect.offsetMin = Vector2.zero;
            volumeRect.offsetMax = Vector2.zero;
            
            // Horizontal volume slider
            var sliderObj = new GameObject("VolumeSlider");
            sliderObj.transform.SetParent(volumeContainer.transform, false);
            
            var sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0f, 0.3f);
            sliderRect.anchorMax = new Vector2(0.7f, 0.7f);  // 70% width for slider
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;
            
            volumeSlider = sliderObj.AddComponent<Slider>();
            volumeSlider.direction = Slider.Direction.LeftToRight; // Horizontal slider
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = 0.75f; // Default 75% volume
            
            // Slider background
            var sliderBg = sliderObj.AddComponent<Image>();
            sliderBg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            volumeSlider.targetGraphic = sliderBg;
            
            // Fill area for horizontal slider
            var fillArea = new GameObject("FillArea");
            fillArea.transform.SetParent(sliderObj.transform, false);
            var fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;
            
            var fill = new GameObject("Fill");
            fill.transform.SetParent(fillArea.transform, false);
            var fillRect = fill.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            
            var fillImage = fill.AddComponent<Image>();
            fillImage.color = new Color(0.3f, 0.7f, 0.3f, 0.8f); // Green fill
            
            volumeSlider.fillRect = fillRect;
            
            // Small volume handle
            var handleSlideArea = new GameObject("HandleSlideArea");
            handleSlideArea.transform.SetParent(sliderObj.transform, false);
            var handleAreaRect = handleSlideArea.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            handleAreaRect.offsetMin = new Vector2(-5f, 0f);
            handleAreaRect.offsetMax = new Vector2(5f, 0f);
            
            var handle = new GameObject("Handle");
            handle.transform.SetParent(handleSlideArea.transform, false);
            var handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(8f, 16f); // Small handle
            
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            
            volumeSlider.handleRect = handleRect;
            
            // Volume percentage text on the right
            var volumeLabel = new GameObject("VolumeLabel");
            volumeLabel.transform.SetParent(volumeContainer.transform, false);
            
            var labelRect = volumeLabel.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0.75f, 0f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            
            volumeText = volumeLabel.AddComponent<Text>();
            volumeText.text = "75%";
            volumeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            volumeText.fontSize = 10;
            volumeText.color = Color.white;
            volumeText.alignment = TextAnchor.MiddleCenter;
            
            // Volume slider callback
            volumeSlider.onValueChanged.AddListener((UnityEngine.Events.UnityAction<float>)delegate(float value) 
            { 
                OnVolumeChanged(value); 
            });
        }
        
        private Button CreateControlButton(string text, float startX, float endX, UnityEngine.Events.UnityAction action)
        {
            var buttonObj = new GameObject($"Button_{text}");
            buttonObj.transform.SetParent(this.transform, false);
            
            var rect = buttonObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(startX, 0.1f);
            rect.anchorMax = new Vector2(endX, 0.9f);
            rect.offsetMin = new Vector2(2f, 2f);
            rect.offsetMax = new Vector2(-2f, -2f);
            
            var button = buttonObj.AddComponent<Button>();
            var image = buttonObj.AddComponent<Image>();
            image.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
            
            var textObj = new GameObject("Text");
            textObj.transform.SetParent(buttonObj.transform, false);
            var textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            var textComp = textObj.AddComponent<Text>();
            textComp.text = text;
            textComp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComp.fontSize = 20;
            textComp.color = Color.white;
            textComp.alignment = TextAnchor.MiddleCenter;
            
            button.targetGraphic = image;
            button.onClick.AddListener(action);
            
            return button;
        }
        
        public void UpdateControls()
        {
            if (manager != null && playButtonText != null)
            {
                playButtonText.text = manager.IsPlaying ? "⏸️" : "▶️";
            }
            
            // Update volume display
            if (volumeSlider != null && volumeText != null)
            {
                int volumePercent = Mathf.RoundToInt(volumeSlider.value * 100f);
                volumeText.text = $"{volumePercent}%";
            }
        }
        
        private void OnVolumeChanged(float volume)
        {
            // Update volume in manager if available
            manager?.SetVolume(volume);
            
            // Update display
            if (volumeText != null)
            {
                int volumePercent = Mathf.RoundToInt(volume * 100f);
                volumeText.text = $"{volumePercent}%";
            }
        }
    }
} 