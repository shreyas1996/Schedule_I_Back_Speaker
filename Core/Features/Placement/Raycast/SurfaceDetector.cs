using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Features.Placement.Data;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Common.Managers;

namespace BackSpeakerMod.Core.Features.Placement.Raycast
{
    /// <summary>
    /// Detects surfaces for object placement using raycast
    /// </summary>
    public class SurfaceDetector
    {
        private readonly PlacementSettings settings;

        /// <summary>
        /// Current raycast hit information
        /// </summary>
        public RaycastHit? CurrentHit { get; private set; }

        /// <summary>
        /// Initialize surface detector
        /// </summary>
        public SurfaceDetector(PlacementSettings placementSettings)
        {
            settings = placementSettings ?? new PlacementSettings();
        }

        /// <summary>
        /// Update surface detection (call from main update loop)
        /// </summary>
        public void Update()
        {
            PerformRaycast();
        }

        /// <summary>
        /// Perform raycast to detect placement surface
        /// </summary>
        private void PerformRaycast()
        {
            var camera = CameraManager.GetPlayerCamera();
            if (camera == null)
            {
                CurrentHit = null;
                return;
            }

            // Create ray from camera through mouse position
            var ray = camera.ScreenPointToRay(UnityEngine.Input.mousePosition);

            // Move ray origin forward to clear player
            ray.origin = camera.transform.position + camera.transform.forward * settings.RayStartOffset;

            if (FeatureFlags.Placement.EnableRaycastDebugging)
            {
                Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 0.1f);
            }

            // Perform raycast
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, settings.PlacementLayerMask))
            {
                CurrentHit = hit;

                if (FeatureFlags.Placement.EnableRaycastDebugging)
                {
                    LoggingSystem.Debug($"Raycast hit: {hit.collider.name} at {hit.point}", "Placement");
                }
            }
            else
            {
                CurrentHit = null;
            }
        }

        /// <summary>
        /// Calculate placement position and rotation from hit
        /// </summary>
        public void GetPlacementTransform(out Vector3 position, out Quaternion rotation)
        {
            if (CurrentHit.HasValue)
            {
                var hit = CurrentHit.Value;
                position = hit.point + hit.normal * settings.WallOffset;
                rotation = Quaternion.LookRotation(hit.normal, Vector3.up);
            }
            else
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// Check if valid surface is available for placement
        /// </summary>
        public bool HasValidSurface => CurrentHit.HasValue;
    }
} 