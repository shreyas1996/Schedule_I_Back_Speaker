using UnityEngine;
using UnityEngine.UI;
using System;
using BackSpeakerMod.Core;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.UI.Components;
using BackSpeakerMod.UI.Helpers;

namespace BackSpeakerMod.UI
{
    public class BackSpeakerScreen : MonoBehaviour
    {
        // IL2CPP compatibility - explicit field initialization
        private BackSpeakerManager? manager = null;
        private RectTransform? parentContainer = null;
        
        // UI Components
        private DisplayPanel? displayPanel = null;
        private ProgressBar? progressBar = null;
        private MusicControlPanel? musicControlPanel = null;
        private VolumeControl? volumeControl = null;
        private PlaylistPanel? playlistPanel = null;
        private HeadphoneControlPanel? headphoneControlPanel = null;
        
        // Layout management
        private RectTransform? controlsContainer = null;
        private bool isPlaylistOpen = false;
        private Vector2 controlsClosedPosition = Vector2.zero;
        private Vector2 controlsOpenPosition = new Vector2(-200f, 0f); // Center in left 50% area (25% from original center)
        
        // IL2CPP compatibility - explicit parameterless constructor
        public BackSpeakerScreen() : base() { }

        public void Setup(BackSpeakerManager? manager)
        {
            try
            {
                this.manager = manager;
                
                // Find the parent container (should be the app's main container)
                FindParentContainer();
                CreateControlsContainer();
                SetupUIComponents();
                
                LoggingSystem.Info("BackSpeaker UI initialized", "UI");
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"BackSpeaker UI setup failed: {ex.Message}", "UI");
                throw;
            }
        }

        private void FindParentContainer()
        {
            // The BackSpeakerScreen is created as a child of the app canvas
            // We need to find the appropriate container to work within
            Transform current = this.transform;
            
            // Look for the Container or similar structure
            while (current != null)
            {
                var container = current.FindChild("Container");
                if (container != null)
                {
                    parentContainer = container.GetComponent<RectTransform>();
                    break;
                }
                current = current.parent;
            }
            
            // Fallback to using this transform if no container found
            if (parentContainer == null)
            {
                parentContainer = this.GetComponent<RectTransform>();
                if (parentContainer == null)
                {
                    parentContainer = this.gameObject.AddComponent<RectTransform>();
                }
            }
            
            // Set up this object's RectTransform to fill the parent
            var myRect = this.GetComponent<RectTransform>();
            if (myRect == null)
            {
                myRect = this.gameObject.AddComponent<RectTransform>();
            }
            
            myRect.anchorMin = Vector2.zero;
            myRect.anchorMax = Vector2.one;
            myRect.offsetMin = Vector2.zero;
            myRect.offsetMax = Vector2.zero;
        }

        private void CreateControlsContainer()
        {
            // Create a container for all the music controls so we can move them as a group
            var containerObj = new GameObject("ControlsContainer");
            containerObj.transform.SetParent(this.transform, false);
            
            controlsContainer = containerObj.AddComponent<RectTransform>();
            controlsContainer.anchorMin = new Vector2(0.5f, 0.5f);
            controlsContainer.anchorMax = new Vector2(0.5f, 0.5f);
            controlsContainer.pivot = new Vector2(0.5f, 0.5f);
            controlsContainer.anchoredPosition = controlsClosedPosition;
            controlsContainer.sizeDelta = new Vector2(400f, 600f);
        }

        private void SetupUIComponents()
        {
            // Setup display panel (album art, track info) - top section
            displayPanel = gameObject.AddComponent<DisplayPanel>();
            displayPanel.Setup(manager!, controlsContainer!);
            
            // Setup progress bar - below display, proper spacing
            progressBar = gameObject.AddComponent<ProgressBar>();
            progressBar.Setup(manager!, controlsContainer!);
            
            // Setup music controls - below progress bar, proper spacing  
            musicControlPanel = gameObject.AddComponent<MusicControlPanel>();
            musicControlPanel.Setup(manager!, controlsContainer!);
            
            // Setup volume control - below music controls, proper spacing
            volumeControl = gameObject.AddComponent<VolumeControl>();
            volumeControl.Setup(manager!, controlsContainer!);
            
            // Setup headphone control panel - below volume control with extra spacing
            headphoneControlPanel = gameObject.AddComponent<HeadphoneControlPanel>();
            headphoneControlPanel.Setup(manager!, controlsContainer!);
            
            // Setup playlist panel - uses the parent container for full screen management
            playlistPanel = gameObject.AddComponent<PlaylistPanel>();
            playlistPanel.Setup(manager!, parentContainer!, this);
            
            // Create playlist toggle button in the controls container so it shifts with other controls
            playlistPanel.CreateToggleButton(controlsContainer!);
        }

        public void OnPlaylistToggle(bool isOpen)
        {
            isPlaylistOpen = isOpen;
            
            // Animate the controls container position
            if (controlsContainer != null)
            {
                Vector2 targetPosition = isOpen ? controlsOpenPosition : controlsClosedPosition;
                controlsContainer.anchoredPosition = targetPosition;
            }
        }

        public void Update()
        {
            try
            {
                if (manager == null) return;
                
                // Update all UI components
                displayPanel?.UpdateDisplay();
                progressBar?.UpdateProgress();
                musicControlPanel?.UpdateButtonText();
                volumeControl?.UpdateVolume();
                headphoneControlPanel?.UpdateStatus();
                playlistPanel?.UpdatePlaylist();
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"BackSpeaker UI update failed: {ex.Message}", "UI");
            }
        }
    }
} 