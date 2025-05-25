using UnityEngine;
using UnityEngine.UI;
using System;
using BackSpeakerMod.Core;
using BackSpeakerMod.UI.Components;
using BackSpeakerMod.UI.Helpers;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.UI
{
    public class BackSpeakerScreen : MonoBehaviour
    {
        // IL2CPP compatibility - explicit field initialization
        private BackSpeakerManager manager = null;
        private DisplayPanel displayPanel = null;
        private MusicControlPanel controlPanel = null;
        private VolumeControl volumeControl = null;
        private ProgressBar progressBar = null;
        private PlaylistPanel playlistPanel = null;
        private float lastUIUpdate = 0f;

        // IL2CPP compatibility - explicit parameterless constructor
        public BackSpeakerScreen() : base() { }

        public void Setup(BackSpeakerManager manager, Image imgBackground)
        {
            try
            {
                LoggerUtil.Info("BackSpeakerScreen: Starting setup");
                
                if (!ValidateInputs(manager, imgBackground))
                    return;
                
                this.manager = manager;
                manager.OnTracksReloaded += UpdateDisplay;
                
                var (canvasTransform, backgroundRect) = LayoutManager.GetTransforms(imgBackground);
                LayoutManager.SetupLayoutConstraints(imgBackground);
                
                CreateUIComponents(imgBackground.transform, backgroundRect);
                UpdateDisplay();
                
                LoggerUtil.Info("BackSpeakerScreen: Setup completed successfully");
            }
            catch (Exception ex)
            {
                LoggerUtil.Error($"BackSpeakerScreen: Setup failed: {ex}");
            }
        }

        private bool ValidateInputs(BackSpeakerManager manager, Image imgBackground)
        {
            if (!LayoutManager.ValidateSetup(imgBackground))
                return false;
                
            if (manager == null)
            {
                LoggerUtil.Error("BackSpeakerScreen: manager is null!");
                return false;
            }
            
            return true;
        }

        private void CreateUIComponents(Transform parent, RectTransform backgroundRect)
        {
            LoggerUtil.Info("BackSpeakerScreen: Creating UI components");
            
            // Create all components using the factory
            ComponentFactory.TryCreateComponent<DisplayPanel>("DisplayPanel", parent, manager, backgroundRect, out displayPanel);
            ComponentFactory.TryCreateComponent<ProgressBar>("ProgressBar", parent, manager, backgroundRect, out progressBar);
            ComponentFactory.TryCreateComponent<MusicControlPanel>("MusicControlPanel", parent, manager, backgroundRect, out controlPanel);
            ComponentFactory.TryCreateComponent<VolumeControl>("VolumeControl", parent, manager, backgroundRect, out volumeControl);
            ComponentFactory.TryCreateComponent<PlaylistPanel>("PlaylistPanel", parent, manager, backgroundRect, out playlistPanel);
            
            LoggerUtil.Info("BackSpeakerScreen: All components created");
        }

        public void UpdateDisplay()
        {
            try
            {
                displayPanel?.UpdateDisplay();
                controlPanel?.UpdateButtonText();
                volumeControl?.UpdateVolume();
                progressBar?.UpdateProgress();
                playlistPanel?.UpdatePlaylist();
                LoggerUtil.Info("BackSpeakerScreen: Display updated");
            }
            catch (Exception ex)
            {
                LoggerUtil.Error($"BackSpeakerScreen: UpdateDisplay failed: {ex}");
            }
        }
        
        void Update()
        {
            if (manager != null)
            {
                manager.Update();
                
                // Update UI at 10fps to reduce overhead
                if (Time.time - lastUIUpdate > 0.1f)
                {
                    UpdateUIComponents();
                    lastUIUpdate = Time.time;
                }
            }
        }

        private void UpdateUIComponents()
        {
            displayPanel?.UpdateDisplay();
            progressBar?.UpdateProgress();
            volumeControl?.UpdateVolume();
            controlPanel?.UpdateButtonText();
        }
        
        void OnDestroy()
        {
            if (manager != null)
                manager.OnTracksReloaded -= UpdateDisplay;
        }
    }
} 