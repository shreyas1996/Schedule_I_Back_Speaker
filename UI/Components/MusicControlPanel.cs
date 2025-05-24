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
        private Button repeatButton;
        private Button reloadButton;
        private Button attachButton;

        public void Setup(BackSpeakerManager manager, Transform canvasTransform)
        {
            try 
            {
                this.manager = manager;
                LoggerUtil.Info("MusicControlPanel: Setting up simplified controls");
                
                // Main control row - proper spacing
                prevTrackButton = UIFactory.CreateButton(
                    canvasTransform, 
                    "<<", // Previous text
                    new Vector2(-90f, -30f), // More spacing from progress bar and each other
                    new Vector2(60f, 45f) // Good touch targets
                );
                prevTrackButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnPreviousTrack);
                LoggerUtil.Info("MusicControlPanel: Previous button created");
                
                // Create Play/Pause button (center) - largest, most prominent
                playPauseButton = UIFactory.CreateButton(
                    canvasTransform, 
                    "PLAY", // Start with PLAY text
                    new Vector2(0f, -30f), // Center, good spacing from progress bar
                    new Vector2(80f, 50f) // Largest button for main action
                );
                playPauseButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnPlayPause);
                LoggerUtil.Info("MusicControlPanel: Play/Pause button created");
                
                // Create Next button (right) - main control row
                nextTrackButton = UIFactory.CreateButton(
                    canvasTransform, 
                    ">>", // Next text
                    new Vector2(90f, -30f), // More spacing from center
                    new Vector2(60f, 45f) // Good touch targets
                );
                nextTrackButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnNextTrack);
                LoggerUtil.Info("MusicControlPanel: Next button created");
                
                // Repeat mode control - centered below main controls
                repeatButton = UIFactory.CreateButton(
                    canvasTransform, 
                    GetRepeatModeText(manager.RepeatMode), 
                    new Vector2(0f, -80f), // Centered below main controls, good spacing
                    new Vector2(120f, 30f) // Wider button for text readability
                );
                repeatButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnToggleRepeat);
                LoggerUtil.Info("MusicControlPanel: Repeat button created");
                
                // Essential fallback controls - bottom row with good spacing
                reloadButton = UIFactory.CreateButton(
                    canvasTransform, 
                    "RELOAD", 
                    new Vector2(-60f, -130f), // Bottom left, well separated
                    new Vector2(80f, 30f) // Better button size
                );
                reloadButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnReload);
                LoggerUtil.Info("MusicControlPanel: Reload button created");
                
                attachButton = UIFactory.CreateButton(
                    canvasTransform, 
                    "ATTACH", 
                    new Vector2(60f, -130f), // Bottom right, well separated
                    new Vector2(80f, 30f) // Better button size
                );
                attachButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnAttach);
                LoggerUtil.Info("MusicControlPanel: Attach button created");
                
                LoggerUtil.Info("MusicControlPanel: Simplified setup completed");
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"MusicControlPanel: Setup failed: {ex}");
                throw;
            }
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
            if (manager.IsAudioReady())
            {
                LoggerUtil.Info("Attach button clicked - detaching speaker");
                // TODO: Add detach functionality if needed
                LoggerUtil.Info("Speaker detach not implemented yet");
            }
            else
            {
                LoggerUtil.Info("Attach button clicked - trying to attach speaker");
                manager.TryAttachSpeaker();
                LoggerUtil.Info($"Audio ready: {manager.IsAudioReady()}");
            }
            UpdateAttachButtonText(); // Update button text after action
        }



        private void OnToggleRepeat()
        {
            // Cycle through repeat modes: None -> RepeatOne -> RepeatAll -> None
            switch (manager.RepeatMode)
            {
                case RepeatMode.None:
                    manager.RepeatMode = RepeatMode.RepeatOne;
                    break;
                case RepeatMode.RepeatOne:
                    manager.RepeatMode = RepeatMode.RepeatAll;
                    break;
                case RepeatMode.RepeatAll:
                    manager.RepeatMode = RepeatMode.None;
                    break;
            }
            UpdateRepeatButtonText();
            LoggerUtil.Info($"Repeat mode toggled to: {manager.RepeatMode}");
        }

        public void UpdateButtonText()
        {
            if (playPauseButton != null && manager != null)
            {
                var textComponent = playPauseButton.GetComponentInChildren<Text>();
                if (textComponent != null)
                    textComponent.text = manager.IsPlaying ? "PAUSE" : "PLAY"; // Clear text labels
            }
            UpdateAttachButtonText();
            UpdateRepeatButtonText();
        }
        
        private void UpdateAttachButtonText()
        {
            if (attachButton != null && manager != null)
            {
                var textComponent = attachButton.GetComponentInChildren<Text>();
                if (textComponent != null)
                {
                    textComponent.text = manager.IsAudioReady() ? "UNATTACH" : "ATTACH";
                }
            }
        }



        private void UpdateRepeatButtonText()
        {
            if (repeatButton != null && manager != null)
            {
                var textComponent = repeatButton.GetComponentInChildren<Text>();
                if (textComponent != null)
                    textComponent.text = GetRepeatModeText(manager.RepeatMode);
            }
        }

        private string GetRepeatModeText(RepeatMode mode)
        {
            switch (mode)
            {
                case RepeatMode.None:
                    return "NO REPEAT";
                case RepeatMode.RepeatOne:
                    return "REPEAT 1";
                case RepeatMode.RepeatAll:
                    return "REPEAT ALL";
                default:
                    return "NO REPEAT";
            }
        }
    }
} 