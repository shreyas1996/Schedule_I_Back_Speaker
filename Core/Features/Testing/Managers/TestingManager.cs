using BackSpeakerMod.Core.System;
// using BackSpeakerMod.Core.Features.Testing.Spheres; // Excluded from compilation
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Features.Testing.Cubes;
using BackSpeakerMod.Core.Features.Testing.Data;
using System.Collections.Generic;

namespace BackSpeakerMod.Core.Features.Testing.Managers
{
    /// <summary>
    /// High-level manager for testing functionality
    /// Coordinates sphere and cube managers
    /// NOTE: Sphere functionality disabled - focusing on headphones
    /// </summary>
    public class TestingManager
    {
        // private readonly GlowingSphereManager sphereManager; // Excluded from compilation
        private readonly TestCubeManager cubeManager;
        private bool isInitialized = false;

        /// <summary>
        /// Initialize testing manager
        /// </summary>
        public TestingManager()
        {
            LoggingSystem.Info("Initializing TestingManager (sphere functionality disabled)", "Testing");
            
            // sphereManager = new GlowingSphereManager(); // Excluded from compilation
            cubeManager = new TestCubeManager();
            LoggingSystem.Info("TestingManager initialized", "Testing");
        }

        /// <summary>
        /// Initialize the testing system
        /// </summary>
        public bool Initialize()
        {
            if (!FeatureFlags.Testing.Enabled)
            {
                LoggingSystem.Info("Testing feature is disabled", "Testing");
                return false;
            }

            LoggingSystem.Info("Initializing testing system", "Testing");
            isInitialized = true;
            LoggingSystem.Info("Testing system initialized successfully", "Testing");
            return true;
        }

        // Glowing Sphere API - stub implementations since spheres are excluded
        public bool CreateGlowingSphere() 
        {
            LoggingSystem.Warning("Sphere functionality disabled - focusing on headphones", "Testing");
            return false;
        }
        
        public bool CreateGlowingSphereAt(UnityEngine.Vector3 position, UnityEngine.Quaternion rotation) 
        {
            LoggingSystem.Warning("Sphere functionality disabled - focusing on headphones", "Testing");
            return false;
        }
        
        public void DestroyGlowingSphere() 
        {
            LoggingSystem.Warning("Sphere functionality disabled - focusing on headphones", "Testing");
        }
        
        public bool ToggleGlowingSphere() 
        {
            LoggingSystem.Warning("Sphere functionality disabled - focusing on headphones", "Testing");
            return false;
        }
        
        public bool IsGlowingSphereActive => false;
        
        public TestObjectState GetGlowingSphereState() => new TestObjectState { Status = TestObjectStatus.Inactive };

        // Test Cube API
        public bool CreateTestCube() => isInitialized && cubeManager.CreateTestCube();
        public void DestroyTestCube() => cubeManager.DestroyTestCube();
        public bool ToggleTestCube() => isInitialized && cubeManager.ToggleTestCube();
        public bool IsTestCubeActive => cubeManager.IsActive;
        public TestObjectState GetTestCubeState() => cubeManager.GetState();

        /// <summary>
        /// Destroy all test objects
        /// </summary>
        public void DestroyAllTestObjects()
        {
            LoggingSystem.Info("Destroying all test objects", "Testing");
            // sphereManager.DestroyGlowingSphere(); // Excluded from compilation
            cubeManager.DestroyTestCube();
        }

        /// <summary>
        /// Get system status
        /// </summary>
        public string GetStatus()
        {
            if (!FeatureFlags.Testing.Enabled)
                return "Testing feature disabled";
                
            if (!isInitialized)
                return "Testing system not initialized";

            var statuses = new List<string>();
            
            // if (sphereManager.IsActive)
            //     statuses.Add(sphereManager.GetStatus());
                
            if (cubeManager.IsActive)
                statuses.Add(cubeManager.GetStatus());

            if (statuses.Count == 0)
                return "Testing system ready - no active objects";
            else
                return $"Testing system active: {string.Join(", ", statuses)}";
        }

        /// <summary>
        /// Shutdown testing manager
        /// </summary>
        public void Shutdown()
        {
            LoggingSystem.Info("Shutting down testing manager", "Testing");
            // sphereManager.Shutdown(); // Excluded from compilation
            cubeManager.Shutdown();
            isInitialized = false;
        }

        /// <summary>
        /// Get glowing sphere manager for direct access (if needed)
        /// </summary>
        // public GlowingSphereManager GetSphereManager() => sphereManager;

        /// <summary>
        /// Get test cube manager for direct access (if needed)
        /// </summary>
        public TestCubeManager GetCubeManager() => cubeManager;
    }
} 