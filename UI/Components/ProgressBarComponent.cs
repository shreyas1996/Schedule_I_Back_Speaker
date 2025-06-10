using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.UI.Helpers;

namespace BackSpeakerMod.UI.Components
{
    /// <summary>
    /// Progress bar component following design specifications
    /// Layout: Progress bar with current/total time display
    /// </summary>
    public class ProgressBarComponent : MonoBehaviour
    {
        private BackSpeakerManager? manager;
        private Slider? progressSlider;
        private Text? timeText;
        private bool isUpdatingSlider = false; // Prevent feedback loops
        
        public ProgressBarComponent() : base() { }
        
        public void Setup(BackSpeakerManager manager)
        {
            this.manager = manager;
            CreateProgressBar();
        }
        
        private void CreateProgressBar()
        {
            // Progress slider
            var sliderObj = new GameObject("ProgressSlider");
            sliderObj.transform.SetParent(this.transform, false);
            
            var sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0f, 0.3f);
            sliderRect.anchorMax = new Vector2(0.75f, 0.7f);
            sliderRect.offsetMin = new Vector2(10f, 0f);
            sliderRect.offsetMax = new Vector2(-10f, 0f);
            
            progressSlider = sliderObj.AddComponent<Slider>();
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
            
            // Add seek functionality
            progressSlider.onValueChanged.AddListener((UnityEngine.Events.UnityAction<float>)delegate(float value) 
            { 
                OnSeek(value); 
            });
            
            // Background
            var background = sliderObj.AddComponent<Image>();
            background.color = new Color(0.3f, 0.3f, 0.3f, 1f);
            progressSlider.targetGraphic = background;
            
            // Handle area
            var handleSlideArea = new GameObject("HandleSlideArea");
            handleSlideArea.transform.SetParent(sliderObj.transform, false);
            var handleAreaRect = handleSlideArea.AddComponent<RectTransform>();
            handleAreaRect.sizeDelta = new Vector2(-10f, 0f); // Smaller handle area
            handleAreaRect.anchorMin = Vector2.zero;
            handleAreaRect.anchorMax = Vector2.one;
            
            // Much smaller handle
            var handle = new GameObject("Handle");
            handle.transform.SetParent(handleSlideArea.transform, false);
            var handleRect = handle.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(8f, 12f); // Small handle: 8px wide, 12px tall
            
            var handleImage = handle.AddComponent<Image>();
            handleImage.color = new Color(0.9f, 0.9f, 0.9f, 1f);
            
            progressSlider.handleRect = handleRect;
            
            // Time display
            CreateTimeDisplay();
        }
        
        private void CreateTimeDisplay()
        {
            var timeObj = new GameObject("TimeDisplay");
            timeObj.transform.SetParent(this.transform, false);
            
            var timeRect = timeObj.AddComponent<RectTransform>();
            timeRect.anchorMin = new Vector2(0.75f, 0f);
            timeRect.anchorMax = new Vector2(1f, 1f);
            timeRect.offsetMin = new Vector2(10f, 0f);
            timeRect.offsetMax = new Vector2(-10f, 0f);
            
            timeText = timeObj.AddComponent<Text>();
            timeText.text = "0:00 / 0:00";
            FontHelper.SetSafeFont(timeText);
            timeText.fontSize = 11;
            timeText.color = Color.white;
            timeText.alignment = TextAnchor.MiddleRight;
        }
        
        public void UpdateProgress()
        {
            if (manager == null || isUpdatingSlider) return;
            
            try
            {
                var progress = manager.Progress;
                var currentTime = manager.CurrentTime;
                var totalTime = manager.TotalTime;
                
                isUpdatingSlider = true;
                progressSlider!.value = progress;
                isUpdatingSlider = false;
                
                var currentMin = Mathf.FloorToInt(currentTime / 60f);
                var currentSec = Mathf.FloorToInt(currentTime % 60f);
                var totalMin = Mathf.FloorToInt(totalTime / 60f);
                var totalSec = Mathf.FloorToInt(totalTime % 60f);
                
                timeText!.text = $"[{currentMin}:{currentSec:00} / {totalMin}:{totalSec:00}]";
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Progress update failed: {ex.Message}", "UI");
                timeText!.text = "[0:00 / 0:00]";
                isUpdatingSlider = false;
            }
        }
        
        private void OnSeek(float value)
        {
            if (manager == null || isUpdatingSlider) return;
            
            try
            {
                LoggingSystem.Info($"Seeking to {value:F2} ({value * 100:F0}%)", "UI");
                manager.SeekToProgress(value);
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Seek failed: {ex.Message}", "UI");
            }
        }
    }
} 