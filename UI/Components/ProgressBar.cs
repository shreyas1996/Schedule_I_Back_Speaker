using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.UI.Helpers;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.UI.Components
{
    public class ProgressBar : MonoBehaviour
    {
        // IL2CPP compatibility - explicit field initialization
        private BackSpeakerManager? manager = null;
        private Slider? progressSlider = null;
        private Text? timeText = null;
        private bool isDragging = false;

        // IL2CPP compatibility - explicit parameterless constructor
        public ProgressBar() : base() { }

        public void Setup(BackSpeakerManager manager, RectTransform parent)
        {
            try
            {
                this.manager = manager;
                // LoggerUtil.Info("ProgressBar: Setting up modern Spotify-style progress bar");
                
                // Modern progress bar with Spotify styling - positioned below display area
                progressSlider = UIFactory.CreateSlider(
                    parent.transform,
                    "ProgressSlider",
                    new Vector2(0f, -10f), // Clear spacing below artist info
                    new Vector2(300f, 20f), // Wide for easy seeking
                    0f,
                    1f,
                    0f
                );
                
                // Make progress slider interactive for seeking
                progressSlider.onValueChanged.AddListener((UnityEngine.Events.UnityAction<float>)OnProgressChanged);
                
                // Apply Spotify-style colors to the progress slider
                ApplySpotifyProgressStyling(progressSlider);
                
                // Add event handlers for dragging detection
                var eventTrigger = progressSlider.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
                
                var pointerDownEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerDownEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
                pointerDownEntry.callback.AddListener((UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData>)OnPointerDown);
                eventTrigger.triggers.Add(pointerDownEntry);
                
                var pointerUpEntry = new UnityEngine.EventSystems.EventTrigger.Entry();
                pointerUpEntry.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
                pointerUpEntry.callback.AddListener((UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData>)OnPointerUp);
                eventTrigger.triggers.Add(pointerUpEntry);
                
                // LoggerUtil.Info("ProgressBar: Progress slider created with seeking functionality");
                
                // Time display with Spotify styling - shows current time / total time, clearly visible
                timeText = UIFactory.CreateText(
                    parent.transform,
                    "TimeDisplay",
                    "0:00 / 0:00",
                    new Vector2(0f, -40f), // Well below progress bar, clear of controls
                    new Vector2(200f, 25f),
                    14 // Readable font for time info
                );
                
                // Apply Spotify-style text color - make it more visible
                timeText.color = new Color(0.9f, 0.9f, 0.9f, 1f); // Bright gray for better visibility
                timeText.fontStyle = FontStyle.Normal;
                
                // LoggerUtil.Info("ProgressBar: Time display created with enhanced visibility");
                
                // LoggerUtil.Info("ProgressBar: Modern Spotify-style setup completed");
            }
            catch (System.Exception _)
            {
                // LoggerUtil.Error($"ProgressBar: Setup failed: {ex}");
                throw;
            }
        }

        private void ApplySpotifyProgressStyling(Slider slider)
        {
            if (slider == null) return;
            
            try
            {
                // Style the background track (darker gray for progress)
                var backgroundRect = slider.transform.Find("Background");
                if (backgroundRect != null)
                {
                    var backgroundImage = backgroundRect.GetComponent<Image>();
                    if (backgroundImage != null)
                    {
                        backgroundImage.color = new Color(0.25f, 0.25f, 0.25f, 0.6f); // Medium gray track
                    }
                }
                
                // Style the fill area (brighter Spotify green for progress)
                var fillAreaRect = slider.transform.Find("Fill Area");
                if (fillAreaRect != null)
                {
                    var fillRect = fillAreaRect.Find("Fill");
                    if (fillRect != null)
                    {
                        var fillImage = fillRect.GetComponent<Image>();
                        if (fillImage != null)
                        {
                            fillImage.color = new Color(0.11f, 0.73f, 0.33f, 1f); // Bright Spotify green
                        }
                    }
                }
                
                // Style the handle (white with good contrast)
                var handleSlideAreaRect = slider.transform.Find("Handle Slide Area");
                if (handleSlideAreaRect != null)
                {
                    var handleRect = handleSlideAreaRect.Find("Handle");
                    if (handleRect != null)
                    {
                        var handleImage = handleRect.GetComponent<Image>();
                        if (handleImage != null)
                        {
                            handleImage.color = new Color(1f, 1f, 1f, 1f); // Pure white handle
                        }
                        
                        // Make the handle appropriately sized for progress seeking
                        var handleRectTransform = handleRect.GetComponent<RectTransform>();
                        if (handleRectTransform != null)
                        {
                            handleRectTransform.sizeDelta = new Vector2(12f, 12f); // Compact but usable handle
                        }
                    }
                }
                
                // LoggerUtil.Info("ProgressBar: Spotify styling applied to progress slider");
            }
            catch (System.Exception _)
            {
                // LoggerUtil.Error($"ProgressBar: Failed to apply Spotify styling: {ex}");
            }
        }

        private void OnPointerDown(UnityEngine.EventSystems.BaseEventData eventData)
        {
            isDragging = true;
        }

        private void OnPointerUp(UnityEngine.EventSystems.BaseEventData eventData)
        {
            isDragging = false;
        }

        private void OnProgressChanged(float value)
        {
            // Only seek when user is actively dragging (not when we're updating programmatically)
            if (isDragging && manager != null)
            {
                manager.SeekToProgress(value);
                // LoggerUtil.Info($"ProgressBar: User seeked to {value:P0}");
            }
        }

        public void UpdateProgress()
        {
            if (manager == null) return;
            
            try
            {
                // Only update slider if user isn't dragging it
                if (!isDragging && progressSlider != null)
                {
                    progressSlider.value = manager.Progress;
                }
                
                // Always update time display
                if (timeText != null)
                {
                    float currentTime = manager.CurrentTime;
                    float totalTime = manager.TotalTime;
                    
                    string currentTimeStr = FormatTime(currentTime);
                    string totalTimeStr = FormatTime(totalTime);
                    
                    timeText.text = $"{currentTimeStr} / {totalTimeStr}";
                }
            }
            catch (System.Exception _)
            {
                // LoggerUtil.Error($"ProgressBar: UpdateProgress failed: {ex}");
            }
        }

        private string FormatTime(float timeInSeconds)
        {
            if (float.IsNaN(timeInSeconds) || timeInSeconds < 0)
                return "0:00";
                
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
            return $"{minutes}:{seconds:D2}";
        }
    }
} 