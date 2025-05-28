using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Features.Placement.Data;
using System;

namespace BackSpeakerMod.Core.Features.Placement.Input
{
    /// <summary>
    /// Handles input for placement operations
    /// </summary>
    public class PlacementInputHandler
    {
        private readonly PlacementSettings settings;
        
        /// <summary>
        /// Event fired when place key is pressed
        /// </summary>
        public event Action OnPlaceRequested;
        
        /// <summary>
        /// Event fired when escape key is pressed
        /// </summary>
        public event Action OnCancelRequested;

        /// <summary>
        /// Initialize input handler
        /// </summary>
        public PlacementInputHandler(PlacementSettings placementSettings)
        {
            settings = placementSettings ?? new PlacementSettings();
        }

        /// <summary>
        /// Update input handling (call from main update loop)
        /// </summary>
        public void Update()
        {
            HandleKeyboardInput();
        }

        /// <summary>
        /// Handle keyboard input for placement
        /// </summary>
        private void HandleKeyboardInput()
        {
            // Cancel with Escape
            if (UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                LoggingSystem.Info("Placement cancelled by user input", "Placement");
                OnCancelRequested?.Invoke();
                return;
            }

            // Place object with configured key
            if (UnityEngine.Input.GetKeyDown(settings.PlaceKey))
            {
                LoggingSystem.Debug($"Place key ({settings.PlaceKey}) pressed", "Placement");
                OnPlaceRequested?.Invoke();
            }
        }

        /// <summary>
        /// Update settings
        /// </summary>
        public void UpdateSettings(PlacementSettings newSettings)
        {
            if (newSettings != null)
            {
                LoggingSystem.Debug("Placement input settings updated", "Placement");
            }
        }
    }
} 