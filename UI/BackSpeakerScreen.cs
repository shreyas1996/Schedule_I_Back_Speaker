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
        private ProgressBar progressBar;
        private PlaylistPanel playlistPanel;

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
                
                // Modern music app layout - vertical sections with proper spacing
                
                // Set up display panel (song info - larger, centered)
                var displayObj = new GameObject("DisplayPanel");
                displayObj.transform.SetParent(imgBackground.transform, false);
                try
                {
                    displayPanel = displayObj.AddComponent<DisplayPanel>();
                    displayPanel.Setup(manager, bgRectTransform);
                    LoggerUtil.Info("DisplayPanel created successfully");
                }
                catch (System.Exception ex)
                {
                    LoggerUtil.Error($"Failed to create DisplayPanel: {ex}");
                    throw;
                }
                
                // Set up progress bar (above main controls)
                var progressObj = new GameObject("ProgressBar");
                progressObj.transform.SetParent(imgBackground.transform, false);
                try
                {
                    progressBar = progressObj.AddComponent<ProgressBar>();
                    progressBar.Setup(manager, bgRectTransform);
                    LoggerUtil.Info("ProgressBar created successfully");
                }
                catch (System.Exception ex)
                {
                    LoggerUtil.Error($"Failed to create ProgressBar: {ex}");
                    throw;
                }
                
                // Set up control panel (main playback controls)
                var controlObj = new GameObject("MusicControlPanel");
                controlObj.transform.SetParent(imgBackground.transform, false);
                try
                {
                    controlPanel = controlObj.AddComponent<MusicControlPanel>();
                    controlPanel.Setup(manager, imgBackground.transform);
                    LoggerUtil.Info("MusicControlPanel created successfully");
                }
                catch (System.Exception ex)
                {
                    LoggerUtil.Error($"Failed to create MusicControlPanel: {ex}");
                    throw;
                }
                
                // Set up volume control (part of secondary controls)
                var volumeObj = new GameObject("VolumeControl");
                volumeObj.transform.SetParent(imgBackground.transform, false);
                try
                {
                    volumeControl = volumeObj.AddComponent<VolumeControl>();
                    volumeControl.Setup(manager, bgRectTransform);
                    LoggerUtil.Info("VolumeControl created successfully");
                }
                catch (System.Exception ex)
                {
                    LoggerUtil.Error($"Failed to create VolumeControl: {ex}");
                    throw;
                }
                
                // Set up playlist panel (hidden by default, toggle to show)
                var playlistObj = new GameObject("PlaylistPanel");
                playlistObj.transform.SetParent(imgBackground.transform, false); // Keep within bounds
                try
                {
                    playlistPanel = playlistObj.AddComponent<PlaylistPanel>();
                    playlistPanel.Setup(manager, bgRectTransform);
                    LoggerUtil.Info("PlaylistPanel created successfully");
                }
                catch (System.Exception ex)
                {
                    LoggerUtil.Error($"Failed to create PlaylistPanel: {ex}");
                    throw;
                }
                
                // Note: Playlist integration temporarily removed for stability
                
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
                progressBar?.UpdateProgress();
                playlistPanel?.UpdatePlaylist();
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