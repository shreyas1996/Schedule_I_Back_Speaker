using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MelonLoader;
using BackSpeakerMod.S1Wrapper;
using BackSpeakerMod.S1Wrapper.Interfaces;
using BackSpeakerMod.NewBackend.Utils;
using BackSpeakerMod.UIWrapper;

namespace BackSpeakerMod.NewFrontend
{
    /// <summary>
    /// BackSpeaker Phone App - manages app creation and UI
    /// Creates one properly working BackSpeaker app with portrait orientation
    /// </summary>
    public class BackSpeakerPhoneApp
    {
        private IApp? _backSpeakerApp;  // The single BackSpeaker app
        private bool _isInitialized = false;
        private GameObject? _testScreen;
        
        // App state tracking (moved from S1Factory)
        private GameObject? _homeScreen;
        private GameObject? _appsCanvas;
        private GameObject? _backSpeakerCanvas;
        private Transform? _appContainer;
        private bool _isPortrait = false;
        private float _lookOffsetMultiplier = 1.0f;
        
        // Rotation control - now handled in S1Factory during cloning
        
        /// <summary>
        /// Initialize the phone app (called from main manager after player detection)
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                NewLoggingSystem.Warning("Phone app already initialized", "PhoneApp");
                return;
            }

            try
            {
                NewLoggingSystem.Info("Initializing BackSpeaker Phone App", "PhoneApp");

                // Set portrait mode to true
                _isPortrait = true;
                
                // Create one properly working BackSpeaker app
                CreateBackSpeakerApp();
                
                // Set up the app if creation was successful
                if (_backSpeakerApp != null)
                {
                    NewLoggingSystem.Info("✓ BackSpeaker app created with portrait orientation (fixed in S1Factory)", "PhoneApp");
                    _isInitialized = true;
                    NewLoggingSystem.Info("✓ BackSpeaker Phone App initialized successfully", "PhoneApp");
                }
                else
                {
                    NewLoggingSystem.Error("Failed to create BackSpeaker app - initialization aborted", "PhoneApp");
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception during phone app initialization: {ex}", "PhoneApp");
            }
        }
        
        /// <summary>
        /// Create the BackSpeaker app exactly like old code
        /// </summary>
        private void CreateBackSpeakerApp()
        {
            try
            {
                NewLoggingSystem.Info("Creating BackSpeaker app exactly like old code...", "PhoneApp");
                
                // Clone app exactly like old code
                var backSpeakerSprite = BackSpeakerMod.NewBackend.Utils.ResourceLoader.LoadEmbeddedSprite("BackSpeakerMod.EmbeddedResources.back_speaker_logo.png");
                var (app, canvas) = S1Factory.CloneApp("BackSpeaker", OnHomeScreenBtnClick, backSpeakerSprite, _isPortrait);
                _backSpeakerApp = app;
                _backSpeakerCanvas = canvas;
                
                if (_backSpeakerApp != null && _backSpeakerCanvas != null)
                {
                    NewLoggingSystem.Info("✓ BackSpeaker app created successfully", "PhoneApp");
                    FindGameObjects();
                    if(_appContainer != null)
                    {
                        SetupBackSpeakerContent();
                    }
                    else
                    {
                        throw new Exception("BackSpeakerApp or AppContainer not found");
                    }
                }
                else
                {
                    NewLoggingSystem.Warning("Failed to create BackSpeaker app", "PhoneApp");
                }
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception creating BackSpeaker app: {ex}", "PhoneApp");
                _backSpeakerApp = null;
            }
        }

        private void FindGameObjects()
        {
            _homeScreen = GameObject.Find("HomeScreen");
            _appsCanvas = GameObject.Find("AppsCanvas");
            _appContainer = _backSpeakerCanvas?.transform.Find("Container");
        }
        
        /// <summary>
        /// Setup BackSpeaker app content - Simplified to just add screen content to pre-configured container
        /// </summary>
        private void SetupBackSpeakerContent()
        {
            try
            {
                NewLoggingSystem.Info("Adding BackSpeaker screen content to configured container", "PhoneApp");

                // Container is already configured by S1Factory (titlebar, background, portrait mode, etc.)
                // We just need to add our screen content
                if (_appContainer != null)
                {
                    // Create BackSpeaker screen GameObject
                    var backSpeakerScreenObj = new GameObject("BackSpeakerScreen");
                    
                    // Add BackSpeakerTestScreen component using S1Factory
                    var backSpeakerScreen = S1Factory.RegisterAndAddComponent<BackSpeakerTestScreen>(backSpeakerScreenObj);
                    if (backSpeakerScreen != null)
                    {
                        // Create the test screen content
                        var testScreenContent = backSpeakerScreen.CreateTestScreen();
                        
                        if (testScreenContent != null)
                        {
                            // Use S1Factory to properly add the screen to the container
                            S1Factory.AddScreenToContainer(_appContainer, testScreenContent);
                            _backSpeakerCanvas?.SetActive(true);
                            NewLoggingSystem.Info("✓ BackSpeaker screen content added successfully", "PhoneApp");
                        }
                        else
                        {
                            NewLoggingSystem.Error("Failed to create BackSpeaker screen content", "PhoneApp");
                        }
                    }
                    else
                    {
                        NewLoggingSystem.Error("Failed to add BackSpeakerTestScreen component", "PhoneApp");
                    }
                }
                else
                {
                    NewLoggingSystem.Error("App container is null, cannot add screen content", "PhoneApp");
                }

                NewLoggingSystem.Info("✓ BackSpeaker app content setup complete", "PhoneApp");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception setting up BackSpeaker content: {ex}", "PhoneApp");
            }
        }
        
        /// <summary>
        /// OnHomeScreenBtnClick - EXACT implementation from old BackSpeakerApp.cs
        /// </summary>
        private void OnHomeScreenBtnClick()
        {
            NewLoggingSystem.Info("Home button clicked", "PhoneApp");

            // OLD CODE: No playlist open, proceed with normal app opening
            NewLoggingSystem.Info("Opening BackSpeaker app normally", "PhoneApp");

            try
            {
                // Get phone reference for proper orientation handling
                var phone = S1Factory.GetPhone();
                if (phone == null)
                {
                    NewLoggingSystem.Warning("Phone not available", "PhoneApp");
                    return;
                }

                // Open the app properly using the app's SetOpen method
                if (_backSpeakerApp != null)
                {
                    NewLoggingSystem.Info("Opening BackSpeaker app via SetOpen(true)", "PhoneApp");
                    _backSpeakerApp.SetOpen(true);

                    // Set phone to portrait mode (not horizontal) - this is the key fix!
                    NewLoggingSystem.Info("Setting phone to portrait mode", "PhoneApp");
                    if(_isPortrait) {
                        phone.SetIsHorizontal(false);
                        phone.SetLookOffsetMultiplier(_lookOffsetMultiplier);
                    } else {
                        phone.SetIsHorizontal(true);
                    }

                    if (_homeScreen != null) _homeScreen.GetComponent<Canvas>().enabled = false;
                    if (_appsCanvas != null) _appsCanvas.GetComponent<Canvas>().enabled = true;
                    if (_backSpeakerCanvas != null) _backSpeakerCanvas.SetActive(true);
                    
                    // Container is now properly configured during app creation - no runtime rotation needed
                }
                else
                {
                    NewLoggingSystem.Warning("BackSpeaker app is null", "PhoneApp");
                }
                
                NewLoggingSystem.Info("✓ BackSpeaker app opened in portrait mode", "PhoneApp");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error opening BackSpeaker app: {ex}", "PhoneApp");
            }
        }
        
        /// <summary>
        /// Set up the BackSpeaker app properties
        /// </summary>
        private void SetupBackSpeakerApp()
        {
            if (_backSpeakerApp == null) return;
            
            try
            {
                NewLoggingSystem.Info("Setting up BackSpeaker app properties...", "PhoneApp");
                
                // App properties are now set in S1Factory during cloning
                NewLoggingSystem.Info("✓ App properties set during cloning in S1Factory", "PhoneApp");
                
                NewLoggingSystem.Info("✓ BackSpeaker app setup complete", "PhoneApp");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception setting up BackSpeaker app: {ex}", "PhoneApp");
            }
        }
        
        /// <summary>
        /// Check if the app is currently in portrait mode
        /// </summary>
        public bool IsInPortraitMode()
        {
            var phone = S1Factory.GetPhone();
            return phone != null && phone.IsInPortraitMode;
        }
        
        /// <summary>
        /// Update method - no longer needed for rotation monitoring since it's fixed in S1Factory
        /// </summary>
        public void Update()
        {
            var phone = S1Factory.GetPhone();
            if (phone != null)
            {
                if(phone.IsOpen) {
                    if(_backSpeakerApp != null) {
                        if(_backSpeakerApp.isOpen) {
                            if(_isPortrait) {
                                phone.SetLookOffsetMultiplier(_lookOffsetMultiplier);
                            }
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Shutdown the phone app
        /// </summary>
        public void Shutdown()
        {
            try
            {
                NewLoggingSystem.Info("Shutting down BackSpeaker Phone App", "PhoneApp");
                
                // Destroy test screen
                if (_testScreen != null)
                {
                    UnityEngine.Object.Destroy(_testScreen);
                    _testScreen = null;
                }
                
                // Clean up the BackSpeaker app and its associated GameObject
                if (_backSpeakerApp != null)
                {
                    try
                    {
                        // Find and destroy the BackSpeaker app GameObject
                        var appCanvas = S1Factory.GetAppCanvas(_backSpeakerApp);
                        if (appCanvas != null && appCanvas.gameObject != null)
                        {
                            UnityEngine.Object.Destroy(appCanvas.gameObject);
                        }
                        
                        _backSpeakerApp.SetOpen(false);
                    }
                    catch (Exception ex)
                    {
                        NewLoggingSystem.Warning($"Exception cleaning up BackSpeaker app: {ex}", "PhoneApp");
                    }
                    _backSpeakerApp = null;
                }
                
                _isInitialized = false;
                NewLoggingSystem.Info("✓ Phone app shutdown complete", "PhoneApp");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception during phone app shutdown: {ex}", "PhoneApp");
            }
        }
        
        /// <summary>
        /// Check if the phone app is initialized
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// Get the BackSpeaker app instance
        /// </summary>
        public IApp? GetBackSpeakerApp() => _backSpeakerApp;
    }
} 