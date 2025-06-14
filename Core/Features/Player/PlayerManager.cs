using UnityEngine;
using UnityEngine.SceneManagement;
using Il2CppScheduleOne.PlayerScripts;
using BackSpeakerMod.Configuration;
using MelonLoader;
using System;
using System.Collections;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.Core.Features.Player
{
    /// <summary>
    /// Scene-based player detection and management
    /// Properly handles menu -> main scene -> player spawn flow
    /// </summary>
    public static class PlayerManager
    {
        /// <summary>
        /// Event fired when player is detected and ready
        /// </summary>
        public static event global::System.Action<Il2CppScheduleOne.PlayerScripts.Player>? OnPlayerReady;

        /// <summary>
        /// Event fired when player is lost (scene change, etc.)
        /// </summary>
        public static event global::System.Action<Il2CppScheduleOne.PlayerScripts.Player>? OnPlayerLost;

        /// <summary>
        /// Current detected player
        /// </summary>
        public static Il2CppScheduleOne.PlayerScripts.Player? CurrentPlayer { get; private set; }

        /// <summary>
        /// Whether we're currently in the main game scene
        /// </summary>
        public static bool IsInMainScene { get; private set; }

        /// <summary>
        /// Whether player detection is active
        /// </summary>
        public static bool IsActive { get; private set; }

        private static bool isWaitingForPlayer = false;

        /// <summary>
        /// Initialize the player detection system
        /// </summary>
        public static void Initialize()
        {
            LoggingSystem.Info("Initializing scene-based player detection", "PlayerManager");
            
            // Subscribe to scene events using proper Il2Cpp delegates
            SceneManager.sceneLoaded += new global::System.Action<Scene, LoadSceneMode>(OnSceneLoaded);
            SceneManager.sceneUnloaded += new global::System.Action<Scene>(OnSceneUnloaded);
            
            IsActive = true;
            CheckCurrentScene();
        }

        /// <summary>
        /// Shutdown the player detection system
        /// </summary>
        public static void Shutdown()
        {
            LoggingSystem.Info("Shutting down player detection", "PlayerManager");
            
            SceneManager.sceneLoaded -= new global::System.Action<Scene, LoadSceneMode>(OnSceneLoaded);
            SceneManager.sceneUnloaded -= new global::System.Action<Scene>(OnSceneUnloaded);
            
            IsActive = false;
            ClearCurrentPlayer();
        }

        /// <summary>
        /// Handle scene loading
        /// </summary>
        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            LoggingSystem.Info($"Scene loaded: {scene.name}", "PlayerManager");
            
            // Check if this is the main game scene
            IsInMainScene = IsMainGameScene(scene.name);
            
            if (IsInMainScene)
            {
                LoggingSystem.Info("Main scene detected, starting player detection", "PlayerManager");
                StartPlayerDetection();
            }
            else
            {
                LoggingSystem.Debug($"Non-main scene loaded: {scene.name}", "PlayerManager");
                ClearCurrentPlayer();
            }
        }

        /// <summary>
        /// Handle scene unloading
        /// </summary>
        private static void OnSceneUnloaded(Scene scene)
        {
            LoggingSystem.Debug($"Scene unloaded: {scene.name}", "PlayerManager");
            
            if (IsMainGameScene(scene.name))
            {
                LoggingSystem.Info("Main scene unloaded, clearing player", "PlayerManager");
                IsInMainScene = false;
                ClearCurrentPlayer();
            }
        }

        /// <summary>
        /// Check if the current scene is a main game scene
        /// </summary>
        private static bool IsMainGameScene(string sceneName)
        {
            // Add known main scene names here
            return sceneName.Contains("Main") || 
                   sceneName.Contains("Game") || 
                   sceneName.Contains("Level") ||
                   sceneName == "SampleScene"; // Common Unity default scene name
        }

        /// <summary>
        /// Start looking for player in the current scene
        /// </summary>
        private static void StartPlayerDetection()
        {
            if (isWaitingForPlayer) return;
            
            isWaitingForPlayer = true;
            MelonCoroutines.Start(DetectPlayerCoroutine());
        }

        /// <summary>
        /// Coroutine to detect player spawning
        /// </summary>
        private static IEnumerator DetectPlayerCoroutine()
        {
            LoggingSystem.Debug("Starting player detection coroutine", "PlayerManager");
            
            float timeout = 30f; // 30 second timeout
            float elapsed = 0f;
            
            while (IsActive && IsInMainScene && elapsed < timeout)
            {
                var player = Il2CppScheduleOne.PlayerScripts.Player.Local;
                if (player != null && IsPlayerValid(player))
                {
                    LoggingSystem.Info("Player detected and validated!", "PlayerManager");
                    SetCurrentPlayer(player);
                    isWaitingForPlayer = false;
                    yield break;
                }
                
                elapsed += 0.5f;
                yield return new WaitForSeconds(0.5f);
            }
            
            if (elapsed >= timeout)
            {
                LoggingSystem.Warning("Player detection timed out after 30 seconds", "PlayerManager");
            }
            
            isWaitingForPlayer = false;
        }

        /// <summary>
        /// Validate that the player is properly initialized
        /// </summary>
        private static bool IsPlayerValid(Il2CppScheduleOne.PlayerScripts.Player player)
        {
            try
            {
                // Basic validation checks
                return player != null && 
                       player.gameObject != null && 
                       player.gameObject.activeInHierarchy &&
                       player.Avatar != null;
            }
            catch (Exception ex)
            {
                LoggingSystem.Debug($"Player validation failed: {ex.Message}", "PlayerManager");
                return false;
            }
        }

        /// <summary>
        /// Set the current player and notify listeners
        /// </summary>
        private static void SetCurrentPlayer(Il2CppScheduleOne.PlayerScripts.Player player)
        {
            if (CurrentPlayer == player) return;
            
            CurrentPlayer = player;
            LoggingSystem.Info($"Player set: {player.name}", "PlayerManager");
            
            try
            {
                OnPlayerReady?.Invoke(player);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error in OnPlayerReady event: {ex.Message}", "PlayerManager");
            }
        }

        /// <summary>
        /// Clear current player and notify listeners
        /// </summary>
        private static void ClearCurrentPlayer()
        {
            if (CurrentPlayer == null) return;
            
            LoggingSystem.Info("Clearing current player", "PlayerManager");
            CurrentPlayer = null;
            isWaitingForPlayer = false;
            
            try
            {
                OnPlayerLost?.Invoke(CurrentPlayer!);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error in OnPlayerLost event: {ex.Message}", "PlayerManager");
            }
        }

        /// <summary>
        /// Check the current scene on initialization
        /// </summary>
        private static void CheckCurrentScene()
        {
            var activeScene = SceneManager.GetActiveScene();
            LoggingSystem.Debug($"Current scene: {activeScene.name}", "PlayerManager");
            
            IsInMainScene = IsMainGameScene(activeScene.name);
            
            if (IsInMainScene)
            {
                StartPlayerDetection();
            }
        }

        /// <summary>
        /// Force a manual player detection check (for debugging)
        /// </summary>
        public static void ForcePlayerDetection()
        {
            LoggingSystem.Info("Forcing player detection", "PlayerManager");
            
            if (IsInMainScene)
            {
                StartPlayerDetection();
            }
            else
            {
                LoggingSystem.Warning("Not in main scene, cannot detect player", "PlayerManager");
            }
        }
    }
} 