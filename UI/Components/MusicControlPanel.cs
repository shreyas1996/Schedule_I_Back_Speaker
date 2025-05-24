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

        public void Setup(BackSpeakerManager manager, Transform canvasTransform)
        {
            this.manager = manager;
            LoggerUtil.Info("MusicControlPanel: Setting up control buttons");
            
            // Create Previous button (left)
            prevTrackButton = UIFactory.CreateButton(
                canvasTransform, 
                "<<", 
                new Vector2(10f, -200f), // Top-left positioning like Drones
                new Vector2(80f, 30f) // Same size as Drones buttons
            );
            prevTrackButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnPreviousTrack);
            
            // Create Play/Pause button (center)
            playPauseButton = UIFactory.CreateButton(
                canvasTransform, 
                "PLAY", 
                new Vector2(100f, -200f), // Next to previous button
                new Vector2(80f, 30f)
            );
            playPauseButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnPlayPause);
            
            // Create Next button (right)
            nextTrackButton = UIFactory.CreateButton(
                canvasTransform, 
                ">>", 
                new Vector2(190f, -200f), // Next to play button
                new Vector2(80f, 30f)
            );
            nextTrackButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnNextTrack);
            
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