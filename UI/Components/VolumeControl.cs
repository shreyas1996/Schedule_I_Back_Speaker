using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.UI.Helpers;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.UI.Components
{
    public class VolumeControl : MonoBehaviour
    {
        // IL2CPP compatibility - explicit field initialization
        private BackSpeakerManager manager = null;
        private Slider volumeSlider = null;

        // IL2CPP compatibility - explicit parameterless constructor
        public VolumeControl() : base() { }

        public void Setup(BackSpeakerManager manager, RectTransform parent)
        {
            try
            {
                this.manager = manager;
                LoggerUtil.Info("VolumeControl: Setting up modern Spotify-style volume control");
                
                // Modern compact volume control with Spotify styling
                volumeSlider = UIFactory.CreateSlider(
                    parent.transform,
                    "VolumeSlider",
                    new Vector2(0f, -110f), // Better spacing: 30px from repeat, 20px from bottom buttons
                    new Vector2(140f, 20f), // Wide for better usability
                    0f,
                    1f,
                    manager?.CurrentVolume ?? 0.5f
                );
                
                volumeSlider.onValueChanged.AddListener((UnityEngine.Events.UnityAction<float>)OnVolumeChanged);
                
                // Apply Spotify-style colors to the volume slider
                ApplySpotifySliderStyling(volumeSlider);
                
                LoggerUtil.Info("VolumeControl: Modern Spotify-style setup completed");
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"VolumeControl: Setup failed: {ex}");
                throw;
            }
        }

        private void ApplySpotifySliderStyling(Slider slider)
        {
            if (slider == null) return;
            
            try
            {
                // Style the background track (dark gray)
                var backgroundRect = slider.transform.Find("Background");
                if (backgroundRect != null)
                {
                    var backgroundImage = backgroundRect.GetComponent<Image>();
                    if (backgroundImage != null)
                    {
                        backgroundImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f); // Dark gray track
                    }
                }
                
                // Style the fill area (Spotify green)
                var fillAreaRect = slider.transform.Find("Fill Area");
                if (fillAreaRect != null)
                {
                    var fillRect = fillAreaRect.Find("Fill");
                    if (fillRect != null)
                    {
                        var fillImage = fillRect.GetComponent<Image>();
                        if (fillImage != null)
                        {
                            fillImage.color = new Color(0.11f, 0.73f, 0.33f, 0.9f); // Spotify green
                        }
                    }
                }
                
                // Style the handle (white with slight green tint)
                var handleSlideAreaRect = slider.transform.Find("Handle Slide Area");
                if (handleSlideAreaRect != null)
                {
                    var handleRect = handleSlideAreaRect.Find("Handle");
                    if (handleRect != null)
                    {
                        var handleImage = handleRect.GetComponent<Image>();
                        if (handleImage != null)
                        {
                            handleImage.color = new Color(0.9f, 1f, 0.9f, 1f); // Slightly green-tinted white
                        }
                        
                        // Make the handle slightly larger for better touch targets
                        var handleRectTransform = handleRect.GetComponent<RectTransform>();
                        if (handleRectTransform != null)
                        {
                            handleRectTransform.sizeDelta = new Vector2(16f, 16f); // Slightly larger handle
                        }
                    }
                }
                
                LoggerUtil.Info("VolumeControl: Spotify styling applied to slider");
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"VolumeControl: Failed to apply Spotify styling: {ex}");
            }
        }

        private void OnVolumeChanged(float volume)
        {
            manager?.SetVolume(volume);
        }

        public void UpdateVolume()
        {
            if (volumeSlider != null && manager != null)
                volumeSlider.value = manager.CurrentVolume;
        }
    }
} 