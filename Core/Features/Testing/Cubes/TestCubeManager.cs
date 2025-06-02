using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Features.Testing.Data;
using System;

namespace BackSpeakerMod.Core.Features.Testing.Cubes
{
    /// <summary>
    /// Manages test cube creation and lifecycle
    /// </summary>
    public class TestCubeManager
    {
        private TestObjectState testCubeState;

        /// <summary>
        /// Initialize test cube manager
        /// </summary>
        public TestCubeManager()
        {
            testCubeState = new TestObjectState();
            LoggingSystem.Info("TestCubeManager initialized", "Testing");
        }

        /// <summary>
        /// Whether test cube is currently active
        /// </summary>
        public bool IsActive => testCubeState.IsActive;

        /// <summary>
        /// Get current test cube state
        /// </summary>
        public TestObjectState GetState() => testCubeState;

        /// <summary>
        /// Create a test cube for debugging
        /// </summary>
        public bool CreateTestCube()
        {
            if (!FeatureFlags.Testing.TestCube)
            {
                LoggingSystem.Warning("Test cube feature not available", "Testing");
                return false;
            }

            try
            {
                LoggingSystem.Info("Creating test cube", "Testing");

                // Destroy existing cube
                DestroyTestCube();

                // Find player for positioning
                var player = Il2CppScheduleOne.PlayerScripts.Player.Local;
                if (player == null)
                {
                    LoggingSystem.Warning("No local player found for test cube placement", "Testing");
                    return false;
                }

                // Create test cube
                var testCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                testCube.name = "TestCube";

                // Position in front of player
                var playerPos = player.transform.position;
                var playerForward = player.transform.forward;
                var position = playerPos + playerForward * 3f + Vector3.up * 1.5f;
                
                testCube.transform.position = position;
                testCube.transform.rotation = Quaternion.identity;

                // Configure appearance
                var config = new TestCubeConfig();
                testCube.transform.localScale = config.Scale;

                var renderer = testCube.GetComponent<Renderer>();
                if (renderer != null)
                {
                    var material = new Material(Shader.Find("Standard"));
                    material.color = config.PrimaryColor;
                    material.SetFloat("_Metallic", 0f);
                    material.SetFloat("_Glossiness", 0.5f);
                    renderer.material = material;
                }

                testCube.layer = config.Layer;

                // Update state
                testCubeState.IsActive = true;
                testCubeState.GameObject = testCube;
                testCubeState.SpawnPosition = position;
                testCubeState.SpawnRotation = Quaternion.identity;
                testCubeState.SpawnTime = Time.time;
                testCubeState.Config = config;

                LoggingSystem.Info($"Test cube created at {position}", "Testing");
                return true;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to create test cube: {ex.Message}", "Testing");
                return false;
            }
        }

        /// <summary>
        /// Destroy test cube
        /// </summary>
        public void DestroyTestCube()
        {
            if (testCubeState.IsActive && testCubeState.GameObject != null)
            {
                try
                {
                    LoggingSystem.Info("Destroying test cube", "Testing");
                    UnityEngine.Object.Destroy(testCubeState.GameObject);
                }
                catch (Exception ex)
                {
                    LoggingSystem.Error($"Error destroying test cube: {ex.Message}", "Testing");
                }
            }
            
            testCubeState.Reset();
        }

        /// <summary>
        /// Toggle test cube on/off
        /// </summary>
        public bool ToggleTestCube()
        {
            if (testCubeState.IsActive)
            {
                DestroyTestCube();
                return false;
            }
            else
            {
                return CreateTestCube();
            }
        }

        /// <summary>
        /// Get status string for test cube
        /// </summary>
        public string GetStatus()
        {
            if (!FeatureFlags.Testing.TestCube)
                return "Test cube disabled";
                
            if (testCubeState.IsActive)
            {
                var elapsed = Time.time - testCubeState.SpawnTime;
                return $"Test cube active ({elapsed:F1}s)";
            }
            
            return "Test cube inactive";
        }

        /// <summary>
        /// Shutdown test cube manager
        /// </summary>
        public void Shutdown()
        {
            LoggingSystem.Info("Shutting down TestCubeManager", "Testing");
            DestroyTestCube();
        }
    }
} 