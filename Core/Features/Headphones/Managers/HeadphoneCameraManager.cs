using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using Il2CppScheduleOne.PlayerScripts;
using Il2CppScheduleOne.DevUtilities;
using System;

namespace BackSpeakerMod.Core.Features.Headphones.Managers
{
    /// <summary>
    /// Manages headphone visibility based on camera mode using layer-based culling
    /// </summary>
    public class HeadphoneCameraManager
    {
        // Unity layer numbers
        private const int LAYER_DEFAULT = 0;
        private const int LAYER_FIRST_PERSON_ONLY = 8;     // Player layer - invisible in first person
        private const int LAYER_THIRD_PERSON_ONLY = 31;   // Use layer 31 for third person only objects
        private const int LAYER_ALWAYS_VISIBLE = 5;       // UI layer - always visible

        private readonly HeadphoneConfig config;
        private GameObject? currentHeadphoneInstance;
        private PlayerCamera? playerCamera;
        
        // Camera mode tracking
        private PlayerCamera.ECameraMode lastCameraMode;
        private bool lastFreeCamState;
        private bool lastViewingAvatarState;
        private bool isFirstPersonMode;

        /// <summary>
        /// Initialize the camera manager
        /// </summary>
        public HeadphoneCameraManager(HeadphoneConfig? headphoneConfig = null)
        {
            config = headphoneConfig ?? new HeadphoneConfig();
            LoggingSystem.Info("HeadphoneCameraManager initialized", "Headphones");
        }

        /// <summary>
        /// Set the headphone instance to manage
        /// </summary>
        public void SetHeadphoneInstance(GameObject? headphoneInstance)
        {
            currentHeadphoneInstance = headphoneInstance;
            if (currentHeadphoneInstance != null)
            {
                LoggingSystem.Info($"HeadphoneCameraManager now managing: {currentHeadphoneInstance.name}", "Headphones");
                UpdateHeadphoneVisibility();
            }
        }

        /// <summary>
        /// Update camera state and headphone visibility - call this in Update loop
        /// </summary>
        public void Update()
        {
            if (currentHeadphoneInstance == null)
                return;

            // Get PlayerCamera instance
            if (playerCamera == null)
            {
                playerCamera = PlayerCamera.Instance;
                if (playerCamera == null)
                    return;
            }

            // Check for camera mode changes
            bool cameraStateChanged = CheckCameraStateChanges();
            
            if (cameraStateChanged)
            {
                UpdateHeadphoneVisibility();
            }
        }

        /// <summary>
        /// Check if camera state has changed
        /// </summary>
        private bool CheckCameraStateChanges()
        {
            if (playerCamera == null)
                return false;

            try
            {
                var currentCameraMode = playerCamera.CameraMode;
                var currentFreeCamState = playerCamera.FreeCamEnabled;
                var currentViewingAvatarState = playerCamera.ViewingAvatar;

                // Determine if we're in first person mode
                bool currentFirstPersonMode = DetermineFirstPersonMode(currentCameraMode, currentFreeCamState, currentViewingAvatarState);

                // Check if anything changed
                bool changed = 
                    lastCameraMode != currentCameraMode ||
                    lastFreeCamState != currentFreeCamState ||
                    lastViewingAvatarState != currentViewingAvatarState ||
                    isFirstPersonMode != currentFirstPersonMode;

                if (changed)
                {
                    LoggingSystem.Info($"Camera mode changed: Mode={currentCameraMode}, FreeCam={currentFreeCamState}, ViewingAvatar={currentViewingAvatarState}, FirstPerson={currentFirstPersonMode}", "Headphones");
                    
                    // Update tracked values
                    lastCameraMode = currentCameraMode;
                    lastFreeCamState = currentFreeCamState;
                    lastViewingAvatarState = currentViewingAvatarState;
                    isFirstPersonMode = currentFirstPersonMode;
                }

                return changed;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error checking camera state: {ex.Message}", "Headphones");
                return false;
            }
        }

        /// <summary>
        /// Determine if we're in first person mode based on camera state
        /// </summary>
        private bool DetermineFirstPersonMode(PlayerCamera.ECameraMode cameraMode, bool freeCamEnabled, bool viewingAvatar)
        {
            // If freecam is enabled or viewing avatar, we're in third person mode
            if (freeCamEnabled || viewingAvatar)
                return false;

            // Default camera mode with no overrides = first person
            if (cameraMode == PlayerCamera.ECameraMode.Default)
                return true;

            // Vehicle and Skateboard modes are typically third person
            return false;
        }

        /// <summary>
        /// Update headphone visibility based on current camera mode
        /// </summary>
        private void UpdateHeadphoneVisibility()
        {
            if (currentHeadphoneInstance == null)
                return;

            try
            {
                int targetLayer = DetermineTargetLayer();
                
                LoggingSystem.Info($"Setting headphone layer to {targetLayer} (FirstPerson: {isFirstPersonMode})", "Headphones");
                
                // Use DevUtilities.LayerUtility.SetLayerRecursively to set the layer
                LayerUtility.SetLayerRecursively(currentHeadphoneInstance, targetLayer);
                
                LoggingSystem.Info($"✓ Headphone visibility updated for camera mode", "Headphones");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to update headphone visibility: {ex.Message}", "Headphones");
            }
        }

        /// <summary>
        /// Determine the appropriate layer based on camera mode
        /// </summary>
        private int DetermineTargetLayer()
        {
            if (isFirstPersonMode)
            {
                // In first person mode, hide the headphones
                // Use a layer that the first person camera doesn't render
                return LAYER_THIRD_PERSON_ONLY;
            }
            else
            {
                // In third person/freecam/avatar view, show the headphones
                // Use default layer or a layer visible to these cameras
                return LAYER_DEFAULT;
            }
        }

        /// <summary>
        /// Force update headphone visibility (useful when attaching headphones)
        /// </summary>
        public void ForceUpdateVisibility()
        {
            if (currentHeadphoneInstance != null && playerCamera != null)
            {
                LoggingSystem.Info("Force updating headphone visibility", "Headphones");
                CheckCameraStateChanges();
                UpdateHeadphoneVisibility();
            }
        }

        /// <summary>
        /// Reset camera tracking (useful when headphones are detached)
        /// </summary>
        public void ResetTracking()
        {
            currentHeadphoneInstance = null;
            LoggingSystem.Info("HeadphoneCameraManager tracking reset", "Headphones");
        }

        /// <summary>
        /// Get current camera mode info for debugging
        /// </summary>
        public string GetCameraInfo()
        {
            if (playerCamera == null)
                return "No PlayerCamera found";

            try
            {
                return $"Mode: {lastCameraMode}, FreeCam: {lastFreeCamState}, ViewingAvatar: {lastViewingAvatarState}, FirstPerson: {isFirstPersonMode}";
            }
            catch (Exception ex)
            {
                return $"Error getting camera info: {ex.Message}";
            }
        }
    }
} 