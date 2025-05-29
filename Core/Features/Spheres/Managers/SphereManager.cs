using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Features.Spheres.Data;
using BackSpeakerMod.Core.Features.Spheres.Attachment;
using BackSpeakerMod.Core.Features.Placement.Managers;

namespace BackSpeakerMod.Core.Features.Spheres.Managers
{
    /// <summary>
    /// High-level manager for sphere attachment and placement functionality
    /// </summary>
    public class SphereManager
    {
        private readonly SphereConfig config;
        private readonly SphereAttachment attachment;
        private readonly PlacementManager placementManager;
        private bool isInitialized = false;

        /// <summary>
        /// Initialize sphere manager
        /// </summary>
        public SphereManager(SphereConfig sphereConfig = null, PlacementManager placementMgr = null)
        {
            config = sphereConfig ?? new SphereConfig();
            attachment = new SphereAttachment(config);
            placementManager = placementMgr;
            
            LoggingSystem.Info("SphereManager initialized", "SphereManager");
        }

        /// <summary>
        /// Initialize the sphere system
        /// </summary>
        public bool Initialize()
        {
            if (!FeatureFlags.Spheres.Enabled)
            {
                LoggingSystem.Info("Sphere feature is disabled", "SphereManager");
                return false;
            }

            LoggingSystem.Info("Initializing sphere system", "SphereManager");
            
            // Subscribe to events
            SubscribeToEvents();
            
            // Auto-attach if configured
            if (config.AutoAttachOnSpawn)
            {
                LoggingSystem.Debug("Auto-attach enabled - will attach sphere on player spawn", "SphereManager");
            }
            
            isInitialized = true;
            LoggingSystem.Info("Sphere system initialized successfully", "SphereManager");
            return true;
        }

        /// <summary>
        /// Current sphere state
        /// </summary>
        public SphereState State => attachment.State;

        /// <summary>
        /// Check if sphere is currently attached
        /// </summary>
        public bool IsAttached => attachment.IsAttached;

        /// <summary>
        /// Attach sphere to player's head
        /// </summary>
        public bool AttachSphereToPlayer()
        {
            if (!isInitialized)
            {
                LoggingSystem.Warning("Sphere system not initialized", "SphereManager");
                return false;
            }

            LoggingSystem.Info("Attaching sphere to player", "SphereManager");
            return attachment.AttachToPlayer();
        }

        /// <summary>
        /// Detach current sphere
        /// </summary>
        public bool DetachSphere()
        {
            if (!isInitialized)
            {
                LoggingSystem.Warning("Sphere system not initialized", "SphereManager");
                return false;
            }

            LoggingSystem.Info("Detaching sphere", "SphereManager");
            return attachment.Detach();
        }

        /// <summary>
        /// Toggle sphere attachment to player
        /// </summary>
        public bool ToggleSphereAttachment()
        {
            if (!isInitialized)
            {
                LoggingSystem.Warning("Sphere system not initialized", "SphereManager");
                return false;
            }

            LoggingSystem.Info("Toggling sphere attachment", "SphereManager");
            return attachment.TogglePlayerAttachment();
        }

        /// <summary>
        /// Start placement mode for spheres
        /// </summary>
        public bool StartSphereePlacement()
        {
            if (!isInitialized)
            {
                LoggingSystem.Warning("Sphere system not initialized", "SphereManager");
                return false;
            }

            if (placementManager == null)
            {
                LoggingSystem.Warning("Placement manager not available", "SphereManager");
                return false;
            }

            LoggingSystem.Info("Starting sphere placement mode", "SphereManager");
            
            // Custom placement logic for spheres could go here
            // For now, use the general placement system
            return true; // Would integrate with placement system
        }

        /// <summary>
        /// Place sphere at specific position
        /// </summary>
        public bool PlaceSphereAt(Vector3 position, Vector3 normal)
        {
            if (!isInitialized)
            {
                LoggingSystem.Warning("Sphere system not initialized", "SphereManager");
                return false;
            }

            LoggingSystem.Info($"Placing sphere at {position}", "SphereManager");
            return attachment.AttachToSurface(position, normal);
        }

        /// <summary>
        /// Handle player spawn event for auto-attach
        /// </summary>
        public void OnPlayerSpawned()
        {
            if (!isInitialized || !config.AutoAttachOnSpawn)
                return;

            LoggingSystem.Info("Player spawned - auto-attaching sphere", "SphereManager");
            AttachSphereToPlayer();
        }

        /// <summary>
        /// Handle player despawn event
        /// </summary>
        public void OnPlayerDespawned()
        {
            if (!isInitialized)
                return;

            LoggingSystem.Info("Player despawned - detaching sphere", "SphereManager");
            DetachSphere();
        }

        /// <summary>
        /// Get sphere status description
        /// </summary>
        public string GetStatus()
        {
            if (!isInitialized)
                return "Sphere system not initialized";

            if (!FeatureFlags.Spheres.Enabled)
                return "Sphere feature disabled";

            return attachment.GetStatusDescription();
        }

        /// <summary>
        /// Get detailed sphere information
        /// </summary>
        public string GetDetailedStatus()
        {
            var status = GetStatus();
            if (attachment.IsAttached)
            {
                var state = attachment.State;
                var elapsed = Time.time - state.AttachTime;
                status += $" ({elapsed:F1}s)";
                
                if (config.ShowDebugInfo)
                {
                    status += $"\nPosition: {state.AttachPosition}";
                    status += $"\nRotation: {state.AttachRotation.eulerAngles}";
                    status += $"\nAttached to: {state.GetAttachmentType()}";
                }
            }
            return status;
        }

        /// <summary>
        /// Update sphere configuration
        /// </summary>
        public void UpdateConfiguration(SphereConfig newConfig)
        {
            if (newConfig == null) return;
            
            LoggingSystem.Info("Updating sphere configuration", "SphereManager");
            attachment.UpdateConfiguration(newConfig);
        }

        /// <summary>
        /// Check if sphere is valid
        /// </summary>
        public bool IsSphereValid() => attachment.IsSphereValid();

        /// <summary>
        /// Subscribe to relevant events
        /// </summary>
        private void SubscribeToEvents()
        {
            SphereEvents.OnSphereAttached += OnSphereAttached;
            SphereEvents.OnSphereDetached += OnSphereDetached;
            SphereEvents.OnSphereAttachmentFailed += OnSphereAttachmentFailed;
            SphereEvents.OnSphereStateChanged += OnSphereStateChanged;
        }

        /// <summary>
        /// Unsubscribe from events
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            SphereEvents.OnSphereAttached -= OnSphereAttached;
            SphereEvents.OnSphereDetached -= OnSphereDetached;
            SphereEvents.OnSphereAttachmentFailed -= OnSphereAttachmentFailed;
            SphereEvents.OnSphereStateChanged -= OnSphereStateChanged;
        }

        /// <summary>
        /// Handle sphere attached event
        /// </summary>
        private void OnSphereAttached(SphereState state)
        {
            LoggingSystem.Info($"Sphere attached to {state.GetAttachmentType()}", "SphereManager");
        }

        /// <summary>
        /// Handle sphere detached event
        /// </summary>
        private void OnSphereDetached(SphereState state)
        {
            LoggingSystem.Info($"Sphere detached from {state.GetAttachmentType()}", "SphereManager");
        }

        /// <summary>
        /// Handle sphere attachment failed event
        /// </summary>
        private void OnSphereAttachmentFailed(string error)
        {
            LoggingSystem.Warning($"Sphere attachment failed: {error}", "SphereManager");
        }

        /// <summary>
        /// Handle sphere state changed event
        /// </summary>
        private void OnSphereStateChanged(SphereState state)
        {
            if (config.ShowDebugInfo)
            {
                LoggingSystem.Debug($"Sphere state changed: {state.Status}", "SphereManager");
            }
        }

        /// <summary>
        /// Shutdown sphere manager
        /// </summary>
        public void Shutdown()
        {
            LoggingSystem.Info("Shutting down sphere manager", "SphereManager");
            
            UnsubscribeFromEvents();
            DetachSphere();
            isInitialized = false;
            
            LoggingSystem.Info("Sphere manager shutdown complete", "SphereManager");
        }
    }
} 