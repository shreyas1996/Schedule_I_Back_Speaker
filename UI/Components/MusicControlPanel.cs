using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.Modules;
using BackSpeakerMod.UI.Helpers;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.UI.Components
{
    public class MusicControlPanel : MonoBehaviour
    {
        // IL2CPP compatibility - explicit field initialization
        private BackSpeakerManager manager = null;
        private Button playPauseButton = null;
        private Button nextTrackButton = null;
        private Button prevTrackButton = null;
        private Button repeatButton = null;
        private Button reloadButton = null;
        private Button statusButton = null;

        // IL2CPP compatibility - explicit parameterless constructor
        public MusicControlPanel() : base() { }

        public void Setup(BackSpeakerManager manager, Transform canvasTransform)
        {
            try 
            {
                this.manager = manager;
                LoggerUtil.Info("MusicControlPanel: Setting up modern Spotify-style controls");
                
                // Main control row - proper spacing with Spotify styling
                prevTrackButton = UIFactory.CreateButton(
                    canvasTransform, 
                    "<<", // Previous text
                    new Vector2(-90f, -30f), // More spacing from progress bar and each other
                    new Vector2(60f, 45f) // Good touch targets
                );
                prevTrackButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnPreviousTrack);
                ApplySecondaryButtonStyling(prevTrackButton);
                LoggerUtil.Info("MusicControlPanel: Previous button created");
                
                // Create Play/Pause button (center) - largest, most prominent with Spotify green
                playPauseButton = UIFactory.CreateButton(
                    canvasTransform, 
                    "PLAY", // Start with PLAY text
                    new Vector2(0f, -30f), // Center, good spacing from progress bar
                    new Vector2(80f, 50f) // Largest button for main action
                );
                playPauseButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnPlayPause);
                ApplyPrimaryButtonStyling(playPauseButton);
                LoggerUtil.Info("MusicControlPanel: Play/Pause button created");
                
                // Create Next button (right) - main control row
                nextTrackButton = UIFactory.CreateButton(
                    canvasTransform, 
                    ">>", // Next text
                    new Vector2(90f, -30f), // More spacing from center
                    new Vector2(60f, 45f) // Good touch targets
                );
                nextTrackButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnNextTrack);
                ApplySecondaryButtonStyling(nextTrackButton);
                LoggerUtil.Info("MusicControlPanel: Next button created");
                
                // Repeat mode control - centered below main controls
                repeatButton = UIFactory.CreateButton(
                    canvasTransform, 
                    GetRepeatModeText(manager.RepeatMode), 
                    new Vector2(0f, -80f), // Centered below main controls, good spacing
                    new Vector2(120f, 30f) // Wider button for text readability
                );
                repeatButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnToggleRepeat);
                ApplyTertiaryButtonStyling(repeatButton);
                LoggerUtil.Info("MusicControlPanel: Repeat button created");
                
                // Essential fallback controls - bottom row with better spacing from volume
                reloadButton = UIFactory.CreateButton(
                    canvasTransform, 
                    "RELOAD", 
                    new Vector2(-70f, -140f), // Bottom left, more space from volume slider
                    new Vector2(90f, 30f) // Slightly wider button
                );
                reloadButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnReload);
                ApplyUtilityButtonStyling(reloadButton);
                LoggerUtil.Info("MusicControlPanel: Reload button created");
                
                // Status button (shows status + manual trigger if needed)
                statusButton = UIFactory.CreateButton(
                    canvasTransform, 
                    "CHECKING...", 
                    new Vector2(70f, -140f), // Bottom right, more space from volume slider
                    new Vector2(100f, 30f) // Wider button to fit status text
                );
                statusButton.onClick.AddListener((UnityEngine.Events.UnityAction)OnStatusButtonClick);
                ApplyStatusButtonStyling(statusButton);
                LoggerUtil.Info("MusicControlPanel: Status button created");
                
                LoggerUtil.Info("MusicControlPanel: Modern Spotify-style setup completed");
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"MusicControlPanel: Setup failed: {ex}");
                throw;
            }
        }

        private void ApplyPrimaryButtonStyling(Button button)
        {
            if (button == null) return;
            
            // Spotify green for primary play/pause button
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.11f, 0.73f, 0.33f, 0.9f); // Spotify green
            }
            
            var textComponent = button.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.color = new Color(0f, 0f, 0f, 1f); // Black text on green
                textComponent.fontSize = 16;
                textComponent.fontStyle = FontStyle.Bold;
            }
            
            // Add subtle glow effect to make it more prominent
            UIFactory.ApplyGlow(button.gameObject, new Color(0.11f, 0.73f, 0.33f, 1f), 0.3f);
            
            // Add modern shadow for depth
            UIFactory.ApplyModernShadow(button.gameObject, new Color(0f, 0f, 0f, 0.4f), new Vector2(1f, -2f));
        }

        private void ApplySecondaryButtonStyling(Button button)
        {
            if (button == null) return;
            
            // Dark gray for secondary control buttons
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.15f, 0.15f, 0.15f, 0.85f); // Dark gray
            }
            
            var textComponent = button.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.color = new Color(1f, 1f, 1f, 0.9f); // White text
                textComponent.fontSize = 14;
                textComponent.fontStyle = FontStyle.Bold;
            }
        }

        private void ApplyTertiaryButtonStyling(Button button)
        {
            if (button == null) return;
            
            // Medium gray for repeat/shuffle buttons
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.25f, 0.25f, 0.25f, 0.8f); // Medium gray
            }
            
            var textComponent = button.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.color = new Color(0.9f, 0.9f, 0.9f, 1f); // Light gray text
                textComponent.fontSize = 12;
                textComponent.fontStyle = FontStyle.Normal;
            }
        }

        private void ApplyUtilityButtonStyling(Button button)
        {
            if (button == null) return;
            
            // Blue accent for utility buttons like reload
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.2f, 0.4f, 0.8f, 0.8f); // Blue accent
            }
            
            var textComponent = button.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.color = new Color(1f, 1f, 1f, 1f); // White text
                textComponent.fontSize = 11;
                textComponent.fontStyle = FontStyle.Normal;
            }
        }

        private void ApplyStatusButtonStyling(Button button)
        {
            if (button == null) return;
            
            // Amber/orange for status indicators
            var buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.color = new Color(0.8f, 0.5f, 0.2f, 0.8f); // Amber/orange
            }
            
            var textComponent = button.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.color = new Color(0f, 0f, 0f, 1f); // Black text on orange
                textComponent.fontSize = 10;
                textComponent.fontStyle = FontStyle.Normal;
            }
        }

        private void OnPlayPause()
        {
            manager?.TogglePlayPause();
            UpdateButtonText();
        }

        private void OnNextTrack()
        {
            manager?.NextTrack();
        }

        private void OnPreviousTrack()
        {
            manager?.PreviousTrack();
        }

        private void OnReload()
        {
            LoggerUtil.Info("Reload button clicked - reloading music tracks");
            manager?.ReloadTracks();
            LoggerUtil.Info($"Reload complete - now have {manager?.GetTrackCount() ?? 0} tracks");
        }

        private void OnStatusButtonClick()
        {
            string status = manager?.GetAttachmentStatus() ?? "Unknown";
            LoggerUtil.Info($"Status button clicked. Current status: {status}");
            
            if (status.Contains("Waiting for player"))
            {
                LoggerUtil.Info("Status: Player needs to spawn in game first");
            }
            else if (status.Contains("Player found"))
            {
                LoggerUtil.Info("Status: Manually triggering attachment...");
                manager?.TriggerManualAttachment();
            }
            else if (status.Contains("Ready"))
            {
                LoggerUtil.Info("Status: System already attached and working");
            }
            else
            {
                LoggerUtil.Info($"Status: Current state is {status}");
            }
        }

        private void OnToggleRepeat()
        {
            if (manager == null) return;
            
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
            UpdateStatusButtonText();
            UpdateRepeatButtonText();
        }
        
        private void UpdateStatusButtonText()
        {
            if (statusButton != null && manager != null)
            {
                var textComponent = statusButton.GetComponentInChildren<Text>();
                if (textComponent != null)
                {
                    textComponent.text = manager.GetAttachmentStatus();
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
                    return "No Repeat";
                case RepeatMode.RepeatOne:
                    return "Repeat One";
                case RepeatMode.RepeatAll:
                    return "Repeat All";
                default:
                    return "No Repeat";
            }
        }
    }
} 