using System;
using System.Collections;
using UnityEngine;
using MelonLoader;

using BackSpeakerMod.S1Wrapper;
using BackSpeakerMod.S1Wrapper.Interfaces;
using BackSpeakerMod.NewBackend.Utils;
using BackSpeakerMod.NewBackend.Testing;

[assembly: MelonInfo(typeof(BackSpeakerMod.NewBackend.BackSpeakerEntry), "Back Speaker Mod", "1.0.0", "Shreyas")]
[assembly: MelonGame("TVGS", "Schedule I")]
[assembly: MelonColor(0, 255, 0, 255)]

namespace BackSpeakerMod.NewBackend
{
    /// <summary>
    /// Main entry point for the new BackSpeaker backend system
    /// Uses S1Player wrapper for proper player detection and lifecycle management
    /// </summary>
    public class BackSpeakerEntry : MelonMod
    {
        private static BackSpeakerEntry? _instance;
        public static BackSpeakerEntry? Instance => _instance;
        
        private BackSpeakerMainManager? _mainManager;
        private bool _isInMainScene = false;
        private IPlayer? _currentPlayer;
        
        // Scene detection settings
        private readonly string[] _mainSceneNames = { "Main", "GameScene", "Level1", "MainMenu" };
        
        // Configuration
        public static bool RunTestsOnStartup = false; // Set to true to run tests
        public static bool RunQuickSmokeTest = true; // Set to false to disable smoke test
        
        [Obsolete]
        public override void OnInitializeMelon()
        {
            _instance = this;
            NewLoggingSystem.Info("BackSpeakerEntry initialized", "Entry");
            
            // Run quick smoke test if enabled
            if (RunQuickSmokeTest)
            {
                MelonCoroutines.Start(NewBackendTester.QuickSmokeTest());
            }
        }
        
        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {
            NewLoggingSystem.Info($"Scene loaded: {sceneName} (Index: {buildIndex})", "Entry");
            
            // Check if this is a main scene
            bool isMainScene = IsMainScene(sceneName);
            
            if (isMainScene && !_isInMainScene)
            {
                _isInMainScene = true;
                NewLoggingSystem.Info($"Entering main scene: {sceneName}", "Entry");
                MelonCoroutines.Start(InitializeInMainScene());
            }
            else if (!isMainScene && _isInMainScene)
            {
                _isInMainScene = false;
                NewLoggingSystem.Info($"Leaving main scene, shutting down", "Entry");
                ShutdownSystem();
            }
        }

        public override void OnUpdate()
        {
            if(_mainManager != null) {
                _mainManager.Update();
            }
        }
        
        private bool IsMainScene(string sceneName)
        {
            foreach (string mainSceneName in _mainSceneNames)
            {
                if (sceneName.Contains(mainSceneName))
                {
                    return true;
                }
            }
            return false;
        }
        
        private IEnumerator InitializeInMainScene()
        {
            NewLoggingSystem.Info("Initializing BackSpeaker system in main scene", "Entry");
            
            // Wait for player detection using S1Player wrapper
            yield return MelonCoroutines.Start(DetectPlayer());
            
            if (_currentPlayer == null)
            {
                NewLoggingSystem.Warning("No player detected, initialization aborted", "Entry");
                yield break;
            }
            
            // Initialize the main manager with the detected player
            Exception initError = null;
            
            try
            {
                _mainManager = BackSpeakerMainManager.Instance;
                _mainManager.Initialize(_currentPlayer);
                
                NewLoggingSystem.Info("✓ BackSpeaker system initialized successfully", "Entry");
            }
            catch (Exception ex)
            {
                initError = ex;
            }
            
            if (initError != null)
            {
                NewLoggingSystem.Error($"Failed to initialize BackSpeaker system: {initError}", "Entry");
                yield break;
            }
            
            // Run comprehensive tests if enabled
            if (RunTestsOnStartup)
            {
                NewLoggingSystem.Info("Running comprehensive tests...", "Entry");
                yield return MelonCoroutines.Start(NewBackendTester.RunTests());
            }
        }
        
        private IEnumerator DetectPlayer()
        {
            NewLoggingSystem.Info("Starting player detection...", "Entry");
            
            int attempts = 0;
            const int maxAttempts = 30; // 30 seconds max
            
            while (attempts < maxAttempts)
            {
                // Use S1Player wrapper for detection
                S1Player.DetectPlayer();
                yield return new WaitForSeconds(1f);
                
                var player = S1Player.GetPlayer();
                if (player != null)
                {
                    _currentPlayer = player;
                    NewLoggingSystem.Info($"✓ Player detected: {player.Name}", "Entry");
                    yield break;
                }
                
                attempts++;
                NewLoggingSystem.Debug($"Player detection attempt {attempts}/{maxAttempts}", "Entry");
            }
            
            NewLoggingSystem.Warning($"Player not found after {maxAttempts} attempts", "Entry");
        }
        
        private void ShutdownSystem()
        {
            try
            {
                NewLoggingSystem.Info("Shutting down BackSpeaker system", "Entry");
                
                // Shutdown main manager
                _mainManager?.Shutdown();
                _mainManager = null;
                
                // Clear player reference
                S1Player.ClearPlayer();
                _currentPlayer = null;
                
                NewLoggingSystem.Info("✓ BackSpeaker system shutdown complete", "Entry");
            }
            catch (Exception ex)
            {
                NewLoggingSystem.Error($"Error during system shutdown: {ex}", "Entry");
            }
        }
        
        public bool IsSystemActive()
        {
            return _isInMainScene && _mainManager != null && _mainManager.IsInitialized;
        }
        
        public BackSpeakerMainManager? GetMainManager()
        {
            return _mainManager;
        }
        
        public IPlayer? GetCurrentPlayer()
        {
            return _currentPlayer;
        }
    }
} 