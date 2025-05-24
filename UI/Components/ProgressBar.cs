using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.UI.Helpers;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.UI.Components
{
    public class ProgressBar : MonoBehaviour
    {
        private BackSpeakerManager manager;
        private Slider progressSlider;
        private Text currentTimeText;
        private Text totalTimeText;
        private bool isDragging = false;

        public void Setup(BackSpeakerManager manager, RectTransform parent)
        {
            this.manager = manager;
            LoggerUtil.Info("ProgressBar: Setting up progress controls");
            
            // Modern progress bar layout - positioned between song info and controls
            
            // Create current time text (left)
            currentTimeText = UIFactory.CreateText(
                parent,
                "CurrentTime",
                "0:00",
                new Vector2(-140f, 35f), // Better positioning for modern layout
                new Vector2(60f, 20f),
                14 // Smaller, cleaner font
            );
            currentTimeText.alignment = TextAnchor.MiddleLeft;
            
            // Create progress slider (center) - Spotify-style
            progressSlider = UIFactory.CreateSlider(
                parent,
                "ProgressSlider",
                new Vector2(0f, 35f), // Positioned between song info and main controls
                new Vector2(280f, 20f), // Wider for better interaction
                0f, 1f, 0f
            );
            
            // Add event listeners for seeking
            progressSlider.onValueChanged.AddListener((UnityEngine.Events.UnityAction<float>)OnProgressChanged);
            
            // We need to detect when user starts/stops dragging
            var eventTrigger = progressSlider.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            
            // Pointer down - start dragging
            var pointerDown = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerDown.eventID = UnityEngine.EventSystems.EventTriggerType.PointerDown;
            pointerDown.callback.AddListener((UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData>)OnDragStart);
            eventTrigger.triggers.Add(pointerDown);
            
            // Pointer up - stop dragging
            var pointerUp = new UnityEngine.EventSystems.EventTrigger.Entry();
            pointerUp.eventID = UnityEngine.EventSystems.EventTriggerType.PointerUp;
            pointerUp.callback.AddListener((UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData>)OnDragEnd);
            eventTrigger.triggers.Add(pointerUp);
            
            // Create total time text (right)
            totalTimeText = UIFactory.CreateText(
                parent,
                "TotalTime",
                "0:00",
                new Vector2(140f, 35f), // Positioned to the right
                new Vector2(60f, 20f),
                14 // Smaller, cleaner font
            );
            totalTimeText.alignment = TextAnchor.MiddleRight;
            
            LoggerUtil.Info("ProgressBar: Setup completed");
        }

        private void OnDragStart(UnityEngine.EventSystems.BaseEventData eventData)
        {
            isDragging = true;
            LoggerUtil.Info("ProgressBar: Started dragging");
        }

        private void OnDragEnd(UnityEngine.EventSystems.BaseEventData eventData)
        {
            isDragging = false;
            LoggerUtil.Info($"ProgressBar: Stopped dragging at {progressSlider.value:F2}");
            // Seek to the new position
            manager.SeekToProgress(progressSlider.value);
        }

        private void OnProgressChanged(float value)
        {
            // Only seek if user is actively dragging (not during automatic updates)
            if (isDragging)
            {
                // Don't seek on every frame while dragging, wait for drag end
                // But update the time display
                float targetTime = value * manager.TotalTime;
                currentTimeText.text = FormatTime(targetTime);
            }
        }

        public void UpdateProgress()
        {
            if (manager == null) return;
            
            try
            {
                // Update slider position (but not while user is dragging)
                if (progressSlider != null && !isDragging)
                {
                    float progress = manager.Progress;
                    if (!float.IsNaN(progress) && !float.IsInfinity(progress))
                    {
                        progressSlider.value = progress;
                    }
                }
                
                // Always update time displays (even while dragging for real-time feedback)
                if (currentTimeText != null)
                {
                    float currentTime = isDragging ? (progressSlider.value * manager.TotalTime) : manager.CurrentTime;
                    currentTimeText.text = FormatTime(currentTime);
                }
                
                if (totalTimeText != null)
                {
                    totalTimeText.text = FormatTime(manager.TotalTime);
                }
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"ProgressBar: UpdateProgress failed: {ex}");
            }
        }
        
        // Call this every frame for smooth updates
        void Update()
        {
            UpdateProgress();
        }

        private string FormatTime(float seconds)
        {
            if (float.IsNaN(seconds) || float.IsInfinity(seconds))
                return "0:00";
                
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return $"{minutes}:{secs:D2}";
        }
    }
} 