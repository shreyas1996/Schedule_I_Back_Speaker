using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Features.Testing.Data;
using BackSpeakerMod.Core.Features.Testing.Helpers;
using System;

namespace BackSpeakerMod.Core.Features.Testing.Spheres
{
    /// <summary>
    /// Creates and manages glowing spheres for testing
    /// </summary>
    public class GlowingSphere
    {
        private readonly GlowingSphereConfig config;
        private TestObjectState state;

        /// <summary>
        /// Initialize glowing sphere creator
        /// </summary>
        public GlowingSphere(GlowingSphereConfig sphereConfig = null)
        {
            config = sphereConfig ?? new GlowingSphereConfig();
            state = new TestObjectState();
            LoggingSystem.Info("GlowingSphere creator initialized", "Testing");
        }

        /// <summary>
        /// Create a glowing sphere at specified position
        /// </summary>
        public GameObject CreateAt(Vector3 position, Quaternion rotation)
        {
            if (!FeatureFlags.Testing.Enabled || !FeatureFlags.Testing.GlowingSphere)
            {
                LoggingSystem.Warning("Glowing sphere feature is disabled", "Testing");
                return null;
            }

            try
            {
                // Destroy existing sphere
                DestroyExisting();

                // Create new sphere
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = config.Name;
                sphere.transform.position = position;
                sphere.transform.rotation = rotation;
                sphere.transform.localScale = config.Scale;

                // Configure materials and appearance
                SphereConfigurationHelper.ConfigureMaterial(sphere, config);
                SphereConfigurationHelper.ConfigureRotation(sphere, config);

                // Set layer
                sphere.layer = config.Layer;

                // Update state
                state.IsActive = true;
                state.GameObject = sphere;
                state.SpawnPosition = position;
                state.SpawnRotation = rotation;
                state.SpawnTime = Time.time;
                state.Config = config;

                LoggingSystem.Info($"Created glowing sphere at {position}", "Testing");
                
                if (FeatureFlags.Testing.LayerTesting)
                {
                    SphereConfigurationHelper.LogSphereDetails(sphere);
                }

                return sphere;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to create glowing sphere: {ex.Message}", "Testing");
                return null;
            }
        }

        /// <summary>
        /// Create sphere in front of camera for guaranteed visibility
        /// </summary>
        public GameObject CreateInFrontOfCamera(float distance = 3f)
        {
            var camera = Camera.main;
            if (camera == null)
            {
                LoggingSystem.Warning("No main camera found for sphere placement", "Testing");
                return null;
            }

            var position = camera.transform.position + camera.transform.forward * distance;
            var rotation = Quaternion.LookRotation(camera.transform.position - position);

            LoggingSystem.Debug($"Creating sphere in front of camera at distance {distance}m", "Testing");
            return CreateAt(position, rotation);
        }

        /// <summary>
        /// Create sphere near player
        /// </summary>
        public GameObject CreateNearPlayer(float distance = 2f, float height = 1.5f)
        {
            var player = Il2CppScheduleOne.PlayerScripts.Player.Local;
            if (player == null)
            {
                LoggingSystem.Warning("No local player found for sphere placement", "Testing");
                return null;
            }

            var playerPos = player.transform.position;
            var playerForward = player.transform.forward;
            
            var position = playerPos + playerForward * distance + Vector3.up * height;
            var rotation = Quaternion.LookRotation(playerPos + Vector3.up * height - position);

            LoggingSystem.Debug($"Creating sphere near player at {distance}m distance, {height}m height", "Testing");
            return CreateAt(position, rotation);
        }

        /// <summary>
        /// Destroy existing sphere if any
        /// </summary>
        public void DestroyExisting()
        {
            if (state.IsActive && state.GameObject != null)
            {
                try
                {
                    LoggingSystem.Info("Destroying existing glowing sphere", "Testing");
                    UnityEngine.Object.Destroy(state.GameObject);
                }
                catch (Exception ex)
                {
                    LoggingSystem.Error($"Error destroying sphere: {ex.Message}", "Testing");
                }
            }
            
            state.Reset();
        }



        /// <summary>
        /// Get current state
        /// </summary>
        public TestObjectState GetState() => state;

        /// <summary>
        /// Check if sphere is active
        /// </summary>
        public bool IsActive => state.IsActive;

        /// <summary>
        /// Get status string
        /// </summary>
        public string GetStatus() => state.GetStatusString();
    }
} 