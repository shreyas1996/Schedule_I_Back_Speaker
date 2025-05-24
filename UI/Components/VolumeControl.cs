using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.UI.Helpers;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.UI.Components
{
    public class VolumeControl : MonoBehaviour
    {
        private BackSpeakerManager manager;
        private Slider volumeSlider;

        public void Setup(BackSpeakerManager manager, RectTransform parent)
        {
            this.manager = manager;
            LoggerUtil.Info("VolumeControl: Setting up volume slider");
            
            // Get canvas dimensions for proper sizing
            Rect canvasRect = parent.rect;
            float canvasWidth = canvasRect.width;
            
            volumeSlider = UIFactory.CreateSlider(
                parent.transform,
                "VolumeSlider",
                new Vector2(0f, -100f), // Below the controls
                new Vector2(250f, 25f), // Fixed width, smaller height
                0f,
                1f,
                manager.CurrentVolume
            );
            
            volumeSlider.onValueChanged.AddListener((UnityEngine.Events.UnityAction<float>)OnVolumeChanged);
            
            LoggerUtil.Info("VolumeControl: Setup completed");
        }

        private void OnVolumeChanged(float volume)
        {
            manager.SetVolume(volume);
        }

        public void UpdateVolume()
        {
            if (volumeSlider != null && manager != null)
                volumeSlider.value = manager.CurrentVolume;
        }
    }
} 