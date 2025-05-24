using UnityEngine;
using UnityEngine.UI;
using System;
using BackSpeakerMod.Core;
using BackSpeakerMod.UI.Components;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.UI
{
    public class BackSpeakerScreen : MonoBehaviour
    {
        private BackSpeakerManager manager;
        private DisplayPanel displayPanel;
        private MusicControlPanel controlPanel;
        private VolumeControl volumeControl;

        public void Setup(BackSpeakerManager manager, Image imgBackground)
        {
            try
            {
                LoggerUtil.Info("BackSpeakerScreen: Starting setup");
                
                if (imgBackground == null)
                {
                    LoggerUtil.Error("BackSpeakerScreen: imgBackground is null!");
                    return;
                }
                
                if (manager == null)
                {
                    LoggerUtil.Error("BackSpeakerScreen: manager is null!");
                    return;
                }
                
                this.manager = manager;
                
                // Subscribe to tracks reload event
                manager.OnTracksReloaded += UpdateDisplay;
                
                // Get the transforms we need - CRITICAL: Use container like Drones does
                Transform canvasTransform = imgBackground.GetComponentInParent<Canvas>().transform;
                RectTransform bgRectTransform = imgBackground.rectTransform;
                
                LoggerUtil.Info("BackSpeakerScreen: Setting up components");
                LoggerUtil.Info($"BackSpeakerScreen: Canvas transform: {canvasTransform?.name}, Background: {bgRectTransform?.name}");
                
                // IMPORTANT: All UI elements should be children of imgBackground, not canvas level
                // This prevents UI bleeding to other apps
                
                // Set up display panel (text elements go on background)
                var displayObj = new GameObject("DisplayPanel");
                displayObj.transform.SetParent(imgBackground.transform, false); // Parent to background, not canvas!
                displayPanel = displayObj.AddComponent<DisplayPanel>();
                displayPanel.Setup(manager, bgRectTransform);
                
                // Set up control panel (buttons go on canvas level for proper interaction)
                var controlObj = new GameObject("MusicControlPanel");
                controlObj.transform.SetParent(canvasTransform, false);
                controlPanel = controlObj.AddComponent<MusicControlPanel>();
                controlPanel.Setup(manager, canvasTransform);
                
                // Set up volume control (slider goes on background)
                var volumeObj = new GameObject("VolumeControl");
                volumeObj.transform.SetParent(imgBackground.transform, false); // Parent to background, not canvas!
                volumeControl = volumeObj.AddComponent<VolumeControl>();
                volumeControl.Setup(manager, bgRectTransform);
                
                // Initial display update
                UpdateDisplay();
                
                LoggerUtil.Info("BackSpeakerScreen: Setup completed successfully");
            }
            catch (Exception ex)
            {
                LoggerUtil.Error($"BackSpeakerScreen: Setup failed with exception: {ex}");
            }
        }

        public void UpdateDisplay()
        {
            try
            {
                displayPanel?.UpdateDisplay();
                controlPanel?.UpdateButtonText();
                volumeControl?.UpdateVolume();
                LoggerUtil.Info("BackSpeakerScreen: Display updated");
            }
            catch (Exception ex)
            {
                LoggerUtil.Error($"BackSpeakerScreen: UpdateDisplay failed: {ex}");
            }
        }
        
        void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (manager != null)
                manager.OnTracksReloaded -= UpdateDisplay;
        }
    }
} 