using UnityEngine;
using MelonLoader;
using System;
using System.Collections;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Features.Player.Attachment;
using BackSpeakerMod.Core.Features.Headphones.Managers;
using BackSpeakerMod.Core.Features.Placement.Managers;
using BackSpeakerMod.Core.Features.Testing.Managers;
using BackSpeakerMod.Core.Features.Audio.Managers;
using BackSpeakerMod.Core.Common.Managers;
using System.Threading.Tasks;

namespace BackSpeakerMod.Core.System
{
    /// <summary>
    /// Handles initialization of all system components
    /// </summary>
    public class SystemInitializer
    {
        private readonly SystemComponents components;
        private bool isInitialized = false;

        /// <summary>
        /// Event fired when initialization is complete
        /// </summary>
        public event Action OnInitializationComplete;

        /// <summary>
        /// Event fired when speaker is attached
        /// </summary>
        public event Action<AudioSource> OnSpeakerAttached;

        /// <summary>
        /// Whether system is initialized
        /// </summary>
        public bool IsInitialized => isInitialized;

        /// <summary>
        /// Initialize with system components
        /// </summary>
        public SystemInitializer(SystemComponents systemComponents)
        {
            components = systemComponents ?? throw new ArgumentNullException(nameof(systemComponents));
            LoggingSystem.Info("SystemInitializer created", "System");
        }

        /// <summary>
        /// Initialize all system components
        /// </summary>
        public async Task<bool> InitializeAsync()
        {
            if (isInitialized)
            {
                LoggingSystem.Warning("SystemInitializer already initialized", "System");
                return true;
            }

            try
            {
                LoggingSystem.Info("Starting system initialization", "System");

                // Initialize core managers first
                InitializeCoreManagers();

                // Initialize feature managers with async support
                await InitializeFeatureManagersAsync();

                // Initialize legacy modules
                InitializeLegacyModules();

                // Wire up events
                WireUpEvents();

                // Complete initialization
                CompleteInitialization();

                isInitialized = true;
                LoggingSystem.Info("System initialization completed successfully", "System");
                OnInitializationComplete?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"System initialization failed: {ex.Message}", "System");
                return false;
            }
        }

        /// <summary>
        /// Initialize all system components (fallback sync method)
        /// </summary>
        public bool Initialize()
        {
            if (isInitialized)
            {
                LoggingSystem.Warning("SystemInitializer already initialized", "System");
                return true;
            }

            try
            {
                LoggingSystem.Info("Starting system initialization", "System");

                // Initialize core managers first
                InitializeCoreManagers();

                // Initialize feature managers
                InitializeFeatureManagers();

                // Initialize legacy modules
                InitializeLegacyModules();

                // Wire up events
                WireUpEvents();

                // Complete initialization
                CompleteInitialization();

                isInitialized = true;
                LoggingSystem.Info("System initialization completed successfully", "System");
                OnInitializationComplete?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"System initialization failed: {ex.Message}", "System");
                return false;
            }
        }

        /// <summary>
        /// Initialize core managers (scene detection, player detection, etc.)
        /// </summary>
        private void InitializeCoreManagers()
        {
            LoggingSystem.Info("Initializing core managers", "System");

            // Initialize PlayerManager for scene and player detection
            PlayerManager.Initialize();

            LoggingSystem.Info("Core managers initialized successfully", "System");
        }

        /// <summary>
        /// Initialize feature managers with async support
        /// </summary>
        private async Task InitializeFeatureManagersAsync()
        {
            LoggingSystem.Info("Initializing feature managers with async support", "System");

            // Initialize headphones with async loading first
            if (components.HeadphoneManager != null)
            {
                await components.HeadphoneManager.InitializeAsync();
            }

            // Initialize sphere manager
            components.SphereManager?.Initialize();

            // Initialize other managers synchronously
            components.PlacementManager?.Initialize();
            components.TestingManager?.Initialize();

            LoggingSystem.Info("Feature managers initialized successfully", "System");
        }

        /// <summary>
        /// Initialize the granular feature managers (sync fallback)
        /// </summary>
        private void InitializeFeatureManagers()
        {
            LoggingSystem.Info("Initializing feature managers", "System");

            components.HeadphoneManager?.Initialize();
            components.SphereManager?.Initialize();
            components.PlacementManager?.Initialize();
            components.TestingManager?.Initialize();

            LoggingSystem.Info("Feature managers initialized successfully", "System");
        }

        /// <summary>
        /// Initialize legacy modules
        /// </summary>
        private void InitializeLegacyModules()
        {
            LoggingSystem.Info("Initializing legacy modules", "System");

            components.PlayerAttachment?.Initialize();

            LoggingSystem.Info("Legacy modules initialized successfully", "System");
        }

        /// <summary>
        /// Wire up all necessary events
        /// </summary>
        private void WireUpEvents()
        {
            LoggingSystem.Info("Wiring up system events", "System");

            // Legacy module events
            if (components.PlayerAttachment != null)
            {
                components.PlayerAttachment.OnSpeakerAttached += HandleSpeakerAttached;
            }

            // Audio manager events
            if (components.AudioManager != null)
            {
                components.AudioManager.OnTracksReloaded += () => components.OnTracksReloaded?.Invoke();
            }

            LoggingSystem.Info("System events wired up successfully", "System");
        }

        /// <summary>
        /// Complete the initialization process
        /// </summary>
        private void CompleteInitialization()
        {
            LoggingSystem.Info("Completing system initialization", "System");
            // Additional completion logic can be added here
        }

        /// <summary>
        /// Handle speaker attachment event
        /// </summary>
        private void HandleSpeakerAttached(AudioSource audioSource)
        {
            LoggingSystem.Info("Speaker attached, initializing audio manager", "System");
            
            components.AudioManager?.Initialize(audioSource);
            LoadTracksAfterAttachment();
            OnSpeakerAttached?.Invoke(audioSource);
        }

        /// <summary>
        /// Load tracks after speaker attachment
        /// </summary>
        private void LoadTracksAfterAttachment()
        {
            if (components.PlayerAttachment?.IsAudioReady() == true)
            {
                LoggingSystem.Info("Audio ready, loading tracks", "System");
                components.AudioManager?.LoadTracks();
            }
            else
            {
                LoggingSystem.Info("Audio not ready, will retry track loading", "System");
                MelonCoroutines.Start(RetryTrackLoading());
            }
        }

        /// <summary>
        /// Retry loading tracks with delay
        /// </summary>
        private IEnumerator RetryTrackLoading()
        {
            yield return new WaitForSeconds(2f);
            LoadTracksAfterAttachment();
        }

        /// <summary>
        /// Shutdown the initializer
        /// </summary>
        public void Shutdown()
        {
            LoggingSystem.Info("Shutting down SystemInitializer", "System");

            // Shutdown core managers
            PlayerManager.Shutdown();

            // Unwire events
            if (components.PlayerAttachment != null)
            {
                components.PlayerAttachment.OnSpeakerAttached -= HandleSpeakerAttached;
            }

            isInitialized = false;
        }
    }
} 