using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.UI.Helpers;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.UI.Components
{
    public class MusicControlPanel : MonoBehaviour
    {
        private BackSpeakerManager manager;
        private Button playPauseButton;
        private Button nextTrackButton;
        private Button prevTrackButton;
        private Button reloadButton;
        private Button attachButton;

        public void Setup(BackSpeakerManager manager, Transform canvasTransform)
        {
            this.manager = manager;
            LoggerUtil.Info("MusicControlPanel: Setting up control buttons");
            
            // Create Previous button (left) - centered positioning
            prevTrackButton = UIFactory.CreateButton(
                canvasTransform, 
                "<<", 
                new Vector2(-100f, 50f), // Center-left positioning
                new Vector2(80f, 40f) // Bigger buttons
            );
            prevTrackButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnPreviousTrack);
            
            // Create Play/Pause button (center)
            playPauseButton = UIFactory.CreateButton(
                canvasTransform, 
                "PLAY", 
                new Vector2(0f, 50f), // Dead center
                new Vector2(80f, 40f)
            );
            playPauseButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnPlayPause);
            
            // Create Next button (right)
            nextTrackButton = UIFactory.CreateButton(
                canvasTransform, 
                ">>", 
                new Vector2(100f, 50f), // Center-right positioning
                new Vector2(80f, 40f)
            );
            nextTrackButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnNextTrack);
            
            // Create Reload button (bottom left)
            reloadButton = UIFactory.CreateButton(
                canvasTransform, 
                "RELOAD", 
                new Vector2(-70f, 100f), // Bottom left
                new Vector2(120f, 30f) // Wider button
            );
            reloadButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnReload);
            
            // Create Attach Speaker button (bottom right)
            attachButton = UIFactory.CreateButton(
                canvasTransform, 
                "ATTACH", 
                new Vector2(70f, 100f), // Bottom right
                new Vector2(120f, 30f) // Wider button
            );
            attachButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnAttach);
            
            LoggerUtil.Info("MusicControlPanel: Setup completed");
        }



        private void OnPlayPause()
        {
            manager.TogglePlayPause();
            UpdateButtonText();
        }

        private void OnNextTrack()
        {
            manager.NextTrack();
        }

        private void OnPreviousTrack()
        {
            manager.PreviousTrack();
        }

        private void OnReload()
        {
            LoggerUtil.Info("Reload button clicked - reloading music tracks");
            manager.ReloadTracks();
            LoggerUtil.Info($"Reload complete - now have {manager.GetTrackCount()} tracks");
        }

        private void OnAttach()
        {
            LoggerUtil.Info("Attach button clicked - trying to attach speaker");
            manager.TryAttachSpeaker();
            LoggerUtil.Info($"Audio ready: {manager.IsAudioReady()}");
        }

        public void UpdateButtonText()
        {
            if (playPauseButton != null && manager != null)
            {
                var textComponent = playPauseButton.GetComponentInChildren<Text>();
                if (textComponent != null)
                    textComponent.text = manager.IsPlaying ? "PAUSE" : "PLAY";
            }
        }
    }
} 