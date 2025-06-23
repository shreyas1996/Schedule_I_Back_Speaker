using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.S1Wrapper;
using BackSpeakerMod.S1Wrapper.Interfaces;
using BackSpeakerMod.NewBackend.Utils;
using BackSpeakerMod.UIWrapper;

namespace BackSpeakerMod.NewFrontend
{
    /// <summary>
    /// BackSpeaker Phone App - manages app creation and UI
    /// Creates one properly working BackSpeaker app
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
        private GameObject? _appIcon;
        private Button? _appButton;
        private Transform? _appContainer;
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
                
                // Register required components (none needed for built-in Unity components)
                // RegisterRequiredComponents();
                
                // Create one properly working BackSpeaker app
                CreateBackSpeakerApp();
                
                // Set up the app if creation was successful
                if (_backSpeakerApp != null)
                {
                    // SetupBackSpeakerApp();
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
        /// Register components required for IL2CPP
        /// </summary>
        private void RegisterRequiredComponents()
        {
            try
            {
                // Note: Built-in Unity UI components (Button, Text, Image, LayoutGroups, etc.) 
                // do NOT need to be registered - Unity handles them automatically
                
                // Only register custom components here if they exist
                // Currently no custom components need registration for the phone app
                
                NewLoggingSystem.Info("✓ Component registration check complete", "PhoneApp");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Failed to register components: {ex}", "PhoneApp");
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
                var (app, canvas) = S1Factory.CloneApp("BackSpeaker", OnHomeScreenBtnClick);
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
            // _backSpeakerCanvas = _appsCanvas?.transform.FindChild("BackSpeakerApp").gameObject;
            _appContainer = _backSpeakerCanvas?.transform.FindChild("Container");
        }
        
        /// <summary>
        /// Setup BackSpeaker app content - exactly like old code
        /// </summary>
        private void SetupBackSpeakerContent()
        {
            try
            {
                NewLoggingSystem.Info("Setting up BackSpeaker app content", "PhoneApp");

                // Update topbar title
                var topbar = _appContainer?.FindChild("Topbar");
                if (topbar != null)
                {
                    var title = topbar.FindChild("Title");
                    if (title != null)
                    {
                        var titleText = title.GetComponent<Text>();
                        if (titleText != null)
                        {
                            titleText.text = "BackSpeaker";
                            NewLoggingSystem.Info("✓ Topbar title updated to 'BackSpeaker'", "PhoneApp");
                        }
                        else
                        {
                            NewLoggingSystem.Warning("Title Text component not found", "PhoneApp");
                        }
                    }
                    else
                    {
                        NewLoggingSystem.Warning("Title GameObject not found in Topbar", "PhoneApp");
                    }
                }
                else
                {
                    NewLoggingSystem.Warning("Topbar GameObject not found", "PhoneApp");
                }

                // Remove Scroll View and Details 
                _appContainer?.FindChild("Scroll View").DetachChildren();
                UnityEngine.Object.Destroy(_appContainer?.FindChild("Scroll View"));
                UnityEngine.Object.Destroy(_appContainer?.FindChild("Details").gameObject);

                // Set up background
                GameObject gameObject3 = _appContainer?.FindChild("Background").gameObject;
                gameObject3.transform.SetAsFirstSibling();
                var imgBackground = gameObject3.GetComponent<Image>();
                imgBackground.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Dark background
                
                // Create BackSpeaker screen GameObject exactly like old code
                var backSpeakerScreenObj = new GameObject("BackSpeakerScreen");
                backSpeakerScreenObj.transform.SetParent(_appContainer, false);
                
                // Add BackSpeakerTestScreen component using S1Factory
                var backSpeakerScreen = S1Factory.RegisterAndAddComponent<BackSpeakerTestScreen>(backSpeakerScreenObj);
                if (backSpeakerScreen != null)
                {
                    // Setup the screen with manager reference
                    var manager = BackSpeakerMod.NewBackend.BackSpeakerMainManager.Instance;
                    if (manager != null)
                    {
                        backSpeakerScreen.CreateTestScreen();
                        _backSpeakerCanvas.active = true;
                        NewLoggingSystem.Info("✓ BackSpeaker screen component added and setup", "PhoneApp");
                    }
                    else
                    {
                        NewLoggingSystem.Warning("BackSpeakerMainManager instance not found", "PhoneApp");
                    }
                }
                else
                {
                    NewLoggingSystem.Error("Failed to add BackSpeakerTestScreen component", "PhoneApp");
                }
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

            // OLD CODE: Check if playlist is open (simplified for now - no playlist component yet)
            // var playlistComponent = backSpeakerScreen?.ContentArea?.PlaylistToggle;
            // if (playlistComponent != null && playlistComponent.IsPlaylistOpen())
            // {
            //     NewLoggingSystem.Info("Playlist is open, closing it instead of exiting app", "PhoneApp");
            //     playlistComponent.ClosePlaylistIfOpen();
            //     return;
            // }

            // OLD CODE: No playlist open, proceed with normal app opening
            NewLoggingSystem.Info("Opening BackSpeaker app normally", "PhoneApp");

            // OLD CODE: EXACT logic
            if (_homeScreen != null) _homeScreen.GetComponent<Canvas>().enabled = false;
            if (_appsCanvas != null) _appsCanvas.GetComponent<Canvas>().enabled = true;
            if (_backSpeakerCanvas != null) _backSpeakerCanvas.active = true;
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
                
                // Set app properties for BackSpeaker app
                _backSpeakerApp.SetData("AppName", "BackSpeaker");
                _backSpeakerApp.SetData("IconLabel", "BackSpeaker");
                
                NewLoggingSystem.Info("✓ BackSpeaker app setup complete", "PhoneApp");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Exception setting up BackSpeaker app: {ex}", "PhoneApp");
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
                        
                        _backSpeakerApp.Stop();
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