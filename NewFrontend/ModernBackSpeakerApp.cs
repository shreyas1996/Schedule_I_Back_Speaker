using System;
using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.NewBackend.Utils;
using BackSpeakerMod.NewBackend;
using BackSpeakerMod.NewFrontend.UI.Screens;
using BackSpeakerMod.S1Wrapper;

namespace BackSpeakerMod.NewFrontend
{
    /// <summary>
    /// Modern BackSpeaker Phone App with redesigned portrait UI
    /// Integrates the new modern UI system with the existing backend
    /// </summary>
    public class ModernBackSpeakerApp : MonoBehaviour
    {
        #region Private Fields
        
        private BackSpeakerMainManager? mainManager;
        private MainPlayerScreen? mainPlayerScreen;
        
        // UI State
        private bool isInitialized = false;
        
        #endregion
        
        #region Public Properties
        
        public bool IsInitialized => isInitialized;
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize the modern BackSpeaker app
        /// </summary>
        public void Initialize()
        {
            try
            {
                NewLoggingSystem.Info("Initializing ModernBackSpeakerApp", "ModernApp");
                
                // Initialize backend manager
                InitializeBackend();
                
                // Create main UI
                CreateMainUI();
                
                isInitialized = true;
                NewLoggingSystem.Info("ModernBackSpeakerApp initialized successfully", "ModernApp");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to initialize ModernBackSpeakerApp: {ex}", "ModernApp");
            }
        }
        
        #endregion
        
        #region Backend Integration
        
        private void InitializeBackend()
        {
            // Create or get the main manager
            var managerObj = new GameObject("BackSpeakerMainManager");
            managerObj.transform.SetParent(this.transform, false);
            
            mainManager = BackSpeakerMainManager.Instance!;
            // Note: BackSpeakerMainManager will be initialized separately by the main mod
            
            NewLoggingSystem.Info("Backend manager reference created", "ModernApp");
        }
        
        #endregion
        
        #region UI Creation
        
        private void CreateMainUI()
        {
            // Create the main player screen
            var mainScreenObj = new GameObject("MainPlayerScreen");
            mainScreenObj.transform.SetParent(this.transform, false);
            
            // Set up full screen layout
            var mainScreenRect = mainScreenObj.AddComponent<RectTransform>();
            mainScreenRect.anchorMin = Vector2.zero;
            mainScreenRect.anchorMax = Vector2.one;
            mainScreenRect.offsetMin = Vector2.zero;
            mainScreenRect.offsetMax = Vector2.zero;
            
            // Add the main player screen component
            mainPlayerScreen = S1Factory.RegisterAndAddComponent<MainPlayerScreen>(mainScreenObj);
            mainPlayerScreen?.Initialize(mainManager!);
            
            NewLoggingSystem.Info("Main UI created successfully", "ModernApp");
        }
        
        #endregion
        
        #region Public Interface
        
        /// <summary>
        /// Create the modern BackSpeaker screen content
        /// Called by the phone app system
        /// </summary>
        public GameObject CreateModernScreen()
        {
            try
            {
                NewLoggingSystem.Info("Creating modern BackSpeaker screen", "ModernApp");
                
                // Main container
                var screenContainer = new GameObject("ModernBackSpeakerScreen");
                
                // Set up container rect transform
                var containerRect = screenContainer.AddComponent<RectTransform>();
                containerRect.anchorMin = Vector2.zero;
                containerRect.anchorMax = Vector2.one;
                containerRect.offsetMin = Vector2.zero;
                containerRect.offsetMax = Vector2.zero;
                
                // Add this component to the container
                var appComponent = screenContainer.AddComponent<ModernBackSpeakerApp>();
                appComponent.Initialize();
                
                NewLoggingSystem.Info("Modern screen created successfully", "ModernApp");
                return screenContainer;
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to create modern screen: {ex}", "ModernApp");
                return null!;
            }
        }
        
        /// <summary>
        /// Get the main player screen component
        /// </summary>
        public MainPlayerScreen? GetMainPlayerScreen()
        {
            return mainPlayerScreen;
        }
        
        /// <summary>
        /// Update method for handling app logic
        /// </summary>
        public void Update()
        {
            if (!isInitialized) return;
            
            // Handle any app-level updates here
            // The individual screen components handle their own updates
        }
        
        /// <summary>
        /// Shutdown the app cleanly
        /// </summary>
        public void Shutdown()
        {
            try
            {
                NewLoggingSystem.Info("Shutting down ModernBackSpeakerApp", "ModernApp");
                
                // Clean up UI components
                if (mainPlayerScreen != null)
                {
                    // The MonoBehaviour will be destroyed automatically
                    mainPlayerScreen = null!;
                }
                
                // Clean up backend references
                mainManager = null!;
                
                isInitialized = false;
                NewLoggingSystem.Info("ModernBackSpeakerApp shutdown complete", "ModernApp");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error during shutdown: {ex}", "ModernApp");
            }
        }
        
        #endregion
        
        #region Unity Lifecycle
        
        private void OnDestroy()
        {
            Shutdown();
        }
        
        #endregion
    }
} 