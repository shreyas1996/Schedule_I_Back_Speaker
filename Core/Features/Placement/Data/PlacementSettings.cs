using UnityEngine;
using System;

namespace BackSpeakerMod.Core.Features.Placement.Data
{
    /// <summary>
    /// Configuration data for placement system
    /// </summary>
    [Serializable]
    public class PlacementSettings
    {
        public float RayStartOffset = 0.5f;
        public LayerMask PlacementLayerMask = 29;
        public float WallOffset = 0.1f;
        public KeyCode PlaceKey = KeyCode.LeftControl;
        public Material PreviewMaterial;
        public Color PreviewColor = Color.green;
        public float PreviewAlpha = 0.7f;
    }

    /// <summary>
    /// Runtime state for placement system
    /// </summary>
    public class PlacementState
    {
        public bool IsActive { get; set; }
        public RaycastHit? CurrentHit { get; set; }
        public GameObject PreviewObject { get; set; }
        public UnityEngine.Renderer PreviewRenderer { get; set; }

        public void Reset()
        {
            IsActive = false;
            CurrentHit = null;
            PreviewObject = null;
            PreviewRenderer = null;
        }

        public string GetStatusString()
        {
            if (!IsActive)
                return "Placement mode inactive";
                
            if (!CurrentHit.HasValue)
                return "Placement mode active - no valid surface found";
                
            return $"Placement mode active - surface: {CurrentHit.Value.collider.name}";
        }
    }
} 