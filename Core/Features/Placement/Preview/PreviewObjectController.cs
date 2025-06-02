using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Features.Placement.Data;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Common.Helpers;

namespace BackSpeakerMod.Core.Features.Placement.Preview
{
    /// <summary>
    /// Manages preview object creation and display
    /// </summary>
    public class PreviewObjectController
    {
        private readonly PlacementSettings settings;
        private GameObject previewObject;
        private Renderer previewRenderer;

        /// <summary>
        /// Whether preview object exists
        /// </summary>
        public bool HasPreview => previewObject != null;

        /// <summary>
        /// Preview object instance
        /// </summary>
        public GameObject PreviewObject => previewObject;

        /// <summary>
        /// Initialize preview controller
        /// </summary>
        public PreviewObjectController(PlacementSettings placementSettings)
        {
            settings = placementSettings ?? new PlacementSettings();
        }

        /// <summary>
        /// Create preview object from prefab
        /// </summary>
        public bool CreatePreview(GameObject prefab)
        {
            if (prefab == null)
            {
                LoggingSystem.Error("Cannot create preview - prefab is null", "Placement");
                return false;
            }

            try
            {
                DestroyPreview();

                previewObject = UnityEngine.Object.Instantiate(prefab);
                previewObject.name = $"Preview_{prefab.name}";

                SetupPreviewMaterial();
                HidePreview();

                LoggingSystem.Debug($"Created preview object: {previewObject.name}", "Placement");
                return true;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Error($"Failed to create preview object: {ex.Message}", "Placement");
                return false;
            }
        }

        /// <summary>
        /// Update preview object position and rotation
        /// </summary>
        public void UpdateTransform(Vector3 position, Quaternion rotation)
        {
            if (previewObject == null) return;

            previewObject.transform.position = position;
            previewObject.transform.rotation = rotation;
        }

        /// <summary>
        /// Show preview object
        /// </summary>
        public void ShowPreview()
        {
            if (previewObject != null && !FeatureFlags.Placement.ShowPreview)
            {
                previewObject.SetActive(false);
                return;
            }

            if (previewObject != null && !previewObject.activeInHierarchy)
            {
                previewObject.SetActive(true);
            }
        }

        /// <summary>
        /// Hide preview object
        /// </summary>
        public void HidePreview()
        {
            if (previewObject != null && previewObject.activeInHierarchy)
            {
                previewObject.SetActive(false);
            }
        }

        /// <summary>
        /// Destroy preview object
        /// </summary>
        public void DestroyPreview()
        {
            if (previewObject != null)
            {
                UnityEngine.Object.Destroy(previewObject);
                previewObject = null;
                previewRenderer = null;
                LoggingSystem.Debug("Preview object destroyed", "Placement");
            }
        }

        /// <summary>
        /// Setup preview material for transparency
        /// </summary>
        private void SetupPreviewMaterial()
        {
            if (previewObject == null) return;

            previewRenderer = previewObject.GetComponent<Renderer>();
            if (previewRenderer == null) return;

            // Create transparent preview material
            var previewColor = settings.PreviewColor;
            previewColor.a = settings.PreviewAlpha;
            
            var previewMaterial = MaterialHelper.CreateTransparentMaterial(previewColor, settings.PreviewAlpha);
            previewRenderer.material = previewMaterial;

            LoggingSystem.Debug("Preview material configured", "Placement");
        }

        /// <summary>
        /// Update settings
        /// </summary>
        public void UpdateSettings(PlacementSettings newSettings)
        {
            if (newSettings != null && previewRenderer != null)
            {
                SetupPreviewMaterial();
                LoggingSystem.Debug("Preview settings updated", "Placement");
            }
        }
    }
} 