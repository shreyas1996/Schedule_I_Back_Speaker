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
            LoggerUtil.Info("VolumeControl: Setting up modern volume control");
            
            // Modern compact volume control - positioned between repeat and bottom buttons
            volumeSlider = UIFactory.CreateSlider(
                parent.transform,
                "VolumeSlider",
                new Vector2(0f, -105f), // Between repeat button and bottom buttons
                new Vector2(140f, 20f), // Wide for better usability
                0f,
                1f,
                manager.CurrentVolume
            );
            
            volumeSlider.onValueChanged.AddListener((UnityEngine.Events.UnityAction<float>)OnVolumeChanged);
            
            LoggerUtil.Info("VolumeControl: Modern setup completed");
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