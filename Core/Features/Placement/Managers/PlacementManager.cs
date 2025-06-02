using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Features.Placement.Raycast;
using BackSpeakerMod.Core.Features.Placement.Data;
using BackSpeakerMod.Core.Common.Helpers;
using BackSpeakerMod.Core.Common.Managers;
using System;

namespace BackSpeakerMod.Core.Features.Placement.Managers
{
    /// <summary>
    /// Manager for object placement functionality
    /// </summary>
    public class PlacementManager
    {
        private readonly RaycastPlacement raycastPlacement;
        private bool isInitialized = false;

        /// <summary>
        /// Event fired when an object is placed
        /// </summary>
        public event Action<Vector3, Quaternion, GameObject> OnObjectPlaced;

        /// <summary>
        /// Initialize placement manager
        /// </summary>
        public PlacementManager()
        {
            var settings = new PlacementSettings();
            raycastPlacement = new RaycastPlacement(settings);
            
            // Subscribe to placement events
            raycastPlacement.OnObjectPlaced += OnRaycastObjectPlaced;
            raycastPlacement.OnPlacementCancelled += OnRaycastPlacementCancelled;

            LoggingSystem.Info("PlacementManager initialized", "Placement");
        }

        /// <summary>
        /// Initialize the placement system
        /// </summary>
        public bool Initialize()
        {
            if (!FeatureFlags.Placement.Enabled)
            {
                LoggingSystem.Info("Placement feature is disabled", "Placement");
                return false;
            }

            LoggingSystem.Info("Initializing placement system", "Placement");

            try
            {
                // Ensure camera is available
                if (!CameraManager.HasValidCamera())
                {
                    LoggingSystem.Warning("No valid camera found for placement system", "Placement");
                    return false;
                }

                isInitialized = true;
                LoggingSystem.Info("Placement system initialized successfully", "Placement");
                return true;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Exception during placement initialization: {ex.Message}", "Placement");
                return false;
            }
        }

        /// <summary>
        /// Start placement mode with a preview object
        /// </summary>
        public bool StartPlacement(GameObject previewPrefab, Func<Vector3, Quaternion, GameObject> placeCallback)
        {
            if (!isInitialized)
            {
                LoggingSystem.Warning("Placement system not initialized", "Placement");
                return false;
            }

            if (!CameraManager.HasValidCamera())
            {
                LoggingSystem.Warning("No valid camera for placement", "Placement");
                return false;
            }

            LoggingSystem.Info("Starting placement mode", "Placement");
            raycastPlacement.StartPlacement(previewPrefab, placeCallback);
            return true;
        }

        /// <summary>
        /// Stop placement mode
        /// </summary>
        public void StopPlacement()
        {
            LoggingSystem.Info("Stopping placement mode", "Placement");
            raycastPlacement.StopPlacement();
        }

        /// <summary>
        /// Toggle placement mode
        /// </summary>
        public bool TogglePlacement(GameObject previewPrefab, Func<Vector3, Quaternion, GameObject> placeCallback)
        {
            if (raycastPlacement.IsActive)
            {
                StopPlacement();
                return false;
            }
            else
            {
                return StartPlacement(previewPrefab, placeCallback);
            }
        }

        /// <summary>
        /// Update placement system (call from main update loop)
        /// </summary>
        public void Update()
        {
            if (!isInitialized || !FeatureFlags.Placement.Enabled)
                return;

            try
            {
                raycastPlacement.Update();
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Exception during placement update: {ex.Message}", "Placement");
            }
        }

        /// <summary>
        /// Check if placement mode is active
        /// </summary>
        public bool IsInPlacementMode => raycastPlacement.IsActive;

        /// <summary>
        /// Get current hit information
        /// </summary>
        public RaycastHit? GetCurrentHit() => raycastPlacement.CurrentHit;

        /// <summary>
        /// Update placement settings
        /// </summary>
        public void UpdateSettings(PlacementSettings newSettings)
        {
            LoggingSystem.Info("Updating placement settings", "Placement");
            raycastPlacement.UpdateSettings(newSettings);
        }

        /// <summary>
        /// Get system status
        /// </summary>
        public string GetStatus()
        {
            if (!FeatureFlags.Placement.Enabled)
                return "Placement feature disabled";
                
            if (!isInitialized)
                return "Placement system not initialized";
                
            if (!CameraManager.HasValidCamera())
                return "No valid camera for placement";
                
            if (raycastPlacement.IsActive)
                return "Placement mode active - move mouse to preview, press Ctrl to place";
                
            return "Placement system ready";
        }

        /// <summary>
        /// Handle raycast object placement
        /// </summary>
        private void OnRaycastObjectPlaced(Vector3 position, Quaternion rotation)
        {
            LoggingSystem.Info($"Object placed via raycast at {position}", "Placement");
            OnObjectPlaced?.Invoke(position, rotation, null); // GameObject will be provided by callback
        }

        /// <summary>
        /// Handle placement cancellation
        /// </summary>
        private void OnRaycastPlacementCancelled()
        {
            LoggingSystem.Info("Placement cancelled by user", "Placement");
        }

        /// <summary>
        /// Shutdown placement manager
        /// </summary>
        public void Shutdown()
        {
            LoggingSystem.Info("Shutting down placement manager", "Placement");
            
            StopPlacement();
            
            // Unsubscribe from events
            raycastPlacement.OnObjectPlaced -= OnRaycastObjectPlaced;
            raycastPlacement.OnPlacementCancelled -= OnRaycastPlacementCancelled;
            
            isInitialized = false;
        }

        /// <summary>
        /// Get raycast placement system for direct access (if needed)
        /// </summary>
        public RaycastPlacement GetRaycastPlacement() => raycastPlacement;
    }
} 