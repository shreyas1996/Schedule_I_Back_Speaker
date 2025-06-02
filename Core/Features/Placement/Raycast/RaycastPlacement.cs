using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Features.Placement.Data;
using BackSpeakerMod.Core.Features.Placement.Input;
using BackSpeakerMod.Core.Features.Placement.Preview;
using System;

namespace BackSpeakerMod.Core.Features.Placement.Raycast
{
    /// <summary>
    /// Coordinates raycast-based placement system
    /// </summary>
    public class RaycastPlacement
    {
        private readonly PlacementSettings settings;
        private readonly PlacementState state;
        private readonly PlacementInputHandler inputHandler;
        private readonly PreviewObjectController previewController;
        private readonly SurfaceDetector surfaceDetector;
        
        private Func<Vector3, Quaternion, GameObject> placeObjectCallback;

        /// <summary>
        /// Event fired when object is placed
        /// </summary>
        public event Action<Vector3, Quaternion> OnObjectPlaced;

        /// <summary>
        /// Event fired when placement is cancelled
        /// </summary>
        public event Action OnPlacementCancelled;

        /// <summary>
        /// Current hit information
        /// </summary>
        public RaycastHit? CurrentHit => surfaceDetector.CurrentHit;

        /// <summary>
        /// Whether placement mode is active
        /// </summary>
        public bool IsActive => state.IsActive;

        /// <summary>
        /// Initialize raycast placement system
        /// </summary>
        public RaycastPlacement(PlacementSettings placementSettings = null)
        {
            settings = placementSettings ?? new PlacementSettings();
            state = new PlacementState();
            inputHandler = new PlacementInputHandler(settings);
            previewController = new PreviewObjectController(settings);
            surfaceDetector = new SurfaceDetector(settings);

            // Wire up events
            inputHandler.OnPlaceRequested += HandlePlaceRequested;
            inputHandler.OnCancelRequested += HandleCancelRequested;

            LoggingSystem.Info("RaycastPlacement system initialized", "Placement");
        }

        /// <summary>
        /// Start placement mode with preview object
        /// </summary>
        public void StartPlacement(GameObject previewPrefab, Func<Vector3, Quaternion, GameObject> placeCallback)
        {
            if (!FeatureFlags.Placement.Enabled)
            {
                LoggingSystem.Warning("Placement system is disabled", "Placement");
                return;
            }

            if (state.IsActive)
            {
                LoggingSystem.Warning("Placement mode already active", "Placement");
                return;
            }

            placeObjectCallback = placeCallback;
            
            if (previewController.CreatePreview(previewPrefab))
            {
                state.IsActive = true;
                LoggingSystem.Info("Placement mode started", "Placement");
            }
        }

        /// <summary>
        /// Stop placement mode
        /// </summary>
        public void StopPlacement()
        {
            if (!state.IsActive) return;

            state.Reset();
            previewController.DestroyPreview();
            placeObjectCallback = null;

            LoggingSystem.Info("Placement mode stopped", "Placement");
        }

        /// <summary>
        /// Update placement system (call from main update loop)
        /// </summary>
        public void Update()
        {
            if (!state.IsActive || !FeatureFlags.Placement.Enabled) return;

            inputHandler.Update();
            surfaceDetector.Update();
            UpdatePreview();
        }

        /// <summary>
        /// Update preview based on surface detection
        /// </summary>
        private void UpdatePreview()
        {
            if (surfaceDetector.HasValidSurface)
            {
                surfaceDetector.GetPlacementTransform(out var position, out var rotation);
                previewController.UpdateTransform(position, rotation);
                previewController.ShowPreview();
            }
            else
            {
                previewController.HidePreview();
            }
        }

        /// <summary>
        /// Handle place request from input
        /// </summary>
        private void HandlePlaceRequested()
        {
            if (!surfaceDetector.HasValidSurface) return;

            surfaceDetector.GetPlacementTransform(out var position, out var rotation);
            
            try
            {
                var placedObject = placeObjectCallback?.Invoke(position, rotation);
                OnObjectPlaced?.Invoke(position, rotation);
                LoggingSystem.Info($"Object placed at {position}", "Placement");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to place object: {ex.Message}", "Placement");
            }
        }

        /// <summary>
        /// Handle cancel request from input
        /// </summary>
        private void HandleCancelRequested()
        {
            OnPlacementCancelled?.Invoke();
            StopPlacement();
        }

        /// <summary>
        /// Update settings
        /// </summary>
        public void UpdateSettings(PlacementSettings newSettings)
        {
            if (newSettings != null)
            {
                inputHandler.UpdateSettings(newSettings);
                previewController.UpdateSettings(newSettings);
                LoggingSystem.Debug("Placement settings updated", "Placement");
            }
        }
    }
} 