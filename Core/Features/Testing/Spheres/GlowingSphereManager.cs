using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Features.Testing.Data;
using BackSpeakerMod.Core.Features.Testing.Spheres;

namespace BackSpeakerMod.Core.Features.Testing.Spheres
{
    /// <summary>
    /// Manages glowing sphere creation and lifecycle
    /// </summary>
    public class GlowingSphereManager
    {
        private readonly GlowingSphere glowingSphere;

        /// <summary>
        /// Initialize glowing sphere manager
        /// </summary>
        public GlowingSphereManager()
        {
            glowingSphere = new GlowingSphere();
            LoggingSystem.Info("GlowingSphereManager initialized", "Testing");
        }

        /// <summary>
        /// Whether glowing sphere is currently active
        /// </summary>
        public bool IsActive => glowingSphere.IsActive;

        /// <summary>
        /// Get current glowing sphere state
        /// </summary>
        public TestObjectState GetState() => glowingSphere.GetState();

        /// <summary>
        /// Create a glowing sphere for testing
        /// </summary>
        public bool CreateGlowingSphere()
        {
            if (!FeatureFlags.Testing.GlowingSphere)
            {
                LoggingSystem.Warning("Glowing sphere feature not available", "Testing");
                return false;
            }

            LoggingSystem.Info("Creating glowing sphere", "Testing");

            // Try camera placement first (most visible)
            var sphere = glowingSphere.CreateInFrontOfCamera(3f);
            if (sphere != null)
            {
                return true;
            }

            // Fallback to player placement
            sphere = glowingSphere.CreateNearPlayer(2f, 1.5f);
            return sphere != null;
        }

        /// <summary>
        /// Create a glowing sphere at specific position
        /// </summary>
        public bool CreateGlowingSphereAt(Vector3 position, Quaternion rotation)
        {
            if (!FeatureFlags.Testing.GlowingSphere)
            {
                LoggingSystem.Warning("Glowing sphere feature not available", "Testing");
                return false;
            }

            LoggingSystem.Info($"Creating glowing sphere at {position}", "Testing");
            var sphere = glowingSphere.CreateAt(position, rotation);
            return sphere != null;
        }

        /// <summary>
        /// Destroy glowing sphere
        /// </summary>
        public void DestroyGlowingSphere()
        {
            LoggingSystem.Info("Destroying glowing sphere", "Testing");
            glowingSphere.DestroyExisting();
        }

        /// <summary>
        /// Toggle glowing sphere on/off
        /// </summary>
        public bool ToggleGlowingSphere()
        {
            if (glowingSphere.IsActive)
            {
                DestroyGlowingSphere();
                return false;
            }
            else
            {
                return CreateGlowingSphere();
            }
        }

        /// <summary>
        /// Get status string for glowing sphere
        /// </summary>
        public string GetStatus()
        {
            if (!FeatureFlags.Testing.GlowingSphere)
                return "Glowing sphere disabled";
                
            var state = glowingSphere.GetState();
            if (state.IsActive)
            {
                var elapsed = Time.time - state.SpawnTime;
                return $"Glowing sphere active ({elapsed:F1}s)";
            }
            
            return "Glowing sphere inactive";
        }

        /// <summary>
        /// Shutdown glowing sphere manager
        /// </summary>
        public void Shutdown()
        {
            LoggingSystem.Info("Shutting down GlowingSphereManager", "Testing");
            DestroyGlowingSphere();
        }

        /// <summary>
        /// Get direct access to glowing sphere (if needed)
        /// </summary>
        public GlowingSphere GetGlowingSphere() => glowingSphere;
    }
} 