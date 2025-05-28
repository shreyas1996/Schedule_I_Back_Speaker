using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using BackSpeakerMod.Core.Common.Managers;

namespace BackSpeakerMod.Core.Common.Managers
{
    /// <summary>
    /// Camera detection and access management
    /// </summary>
    public static class CameraManager
    {
        /// <summary>
        /// Cached main camera reference
        /// </summary>
        private static Camera _mainCamera;

        /// <summary>
        /// Cached player camera reference
        /// </summary>
        private static Camera _playerCamera;

        /// <summary>
        /// Time when cache was last updated
        /// </summary>
        private static float _lastCacheUpdate;

        /// <summary>
        /// Cache duration in seconds
        /// </summary>
        private const float CacheDuration = 1f;

        /// <summary>
        /// Get the main camera (falls back to Camera.main)
        /// </summary>
        public static Camera GetMainCamera()
        {
            if (ShouldRefreshCache())
            {
                RefreshCameraCache();
            }

            return _mainCamera ?? Camera.main;
        }

        /// <summary>
        /// Get the player camera (tries multiple methods)
        /// </summary>
        public static Camera GetPlayerCamera()
        {
            if (ShouldRefreshCache())
            {
                RefreshCameraCache();
            }

            return _playerCamera ?? GetMainCamera();
        }

        /// <summary>
        /// Find camera by name (common camera names)
        /// </summary>
        public static Camera FindCameraByName(string name)
        {
            var cameraObj = GameObject.Find(name);
            if (cameraObj != null)
            {
                var camera = cameraObj.GetComponent<Camera>();
                if (camera != null)
                {
                    LoggingSystem.Debug($"Found camera by name: {name}", "CameraHelper");
                    return camera;
                }
            }
            return null;
        }

        /// <summary>
        /// Try to find the player's first-person camera
        /// </summary>
        public static Camera FindPlayerCamera()
        {
            LoggingSystem.Debug("Searching for player camera...", "CameraHelper");

            // Method 1: Check if player is available and has camera
            var player = PlayerManager.CurrentPlayer;
            if (player != null)
            {
                var playerCamera = FindCameraInPlayer(player);
                if (playerCamera != null)
                {
                    LoggingSystem.Debug("Found camera attached to player", "CameraHelper");
                    return playerCamera;
                }
            }

            // Method 2: Try common camera names
            string[] commonNames = {
                "Main Camera",
                "PlayerCamera",
                "FirstPersonCamera", 
                "FPSCamera",
                "Player_Camera",
                "PlayerCam",
                "Camera"
            };

            foreach (var name in commonNames)
            {
                var camera = FindCameraByName(name);
                if (camera != null)
                {
                    LoggingSystem.Debug($"Found camera by common name: {name}", "CameraHelper");
                    return camera;
                }
            }

            // Method 3: Find any active camera in scene
            var allCameras = UnityEngine.Object.FindObjectsOfType<Camera>();
            foreach (var camera in allCameras)
            {
                if (camera.gameObject.activeInHierarchy && camera.enabled)
                {
                    LoggingSystem.Debug($"Found active camera: {camera.name}", "CameraHelper");
                    return camera;
                }
            }

            LoggingSystem.Warning("No suitable camera found", "CameraHelper");
            return null;
        }

        /// <summary>
        /// Find camera component in player object hierarchy
        /// </summary>
        private static Camera FindCameraInPlayer(Il2CppScheduleOne.PlayerScripts.Player player)
        {
            try
            {
                // Check player object itself
                var camera = player.GetComponent<Camera>();
                if (camera != null) return camera;

                // Check children recursively
                camera = player.GetComponentInChildren<Camera>();
                if (camera != null) return camera;

                // Check player avatar if available
                if (player.Avatar != null)
                {
                    camera = player.Avatar.GetComponent<Camera>();
                    if (camera != null) return camera;

                    camera = player.Avatar.GetComponentInChildren<Camera>();
                    if (camera != null) return camera;
                }

                LoggingSystem.Debug("No camera found in player hierarchy", "CameraHelper");
                return null;
            }
            catch (global::System.Exception ex)
            {
                LoggingSystem.Debug($"Error searching for camera in player: {ex.Message}", "CameraHelper");
                return null;
            }
        }

        /// <summary>
        /// Check if camera cache should be refreshed
        /// </summary>
        private static bool ShouldRefreshCache()
        {
            return Time.time - _lastCacheUpdate > CacheDuration;
        }

        /// <summary>
        /// Refresh the camera cache
        /// </summary>
        private static void RefreshCameraCache()
        {
            LoggingSystem.Debug("Refreshing camera cache", "CameraHelper");

            _playerCamera = FindPlayerCamera();
            _mainCamera = Camera.main;
            _lastCacheUpdate = Time.time;
        }

        /// <summary>
        /// Force refresh camera cache (useful when player changes)
        /// </summary>
        public static void ForceRefresh()
        {
            LoggingSystem.Debug("Force refreshing camera cache", "CameraHelper");
            _lastCacheUpdate = 0f; // Force refresh on next access
        }

        /// <summary>
        /// Clear the camera cache
        /// </summary>
        public static void ClearCache()
        {
            LoggingSystem.Debug("Clearing camera cache", "CameraHelper");
            _mainCamera = null;
            _playerCamera = null;
            _lastCacheUpdate = 0f;
        }

        /// <summary>
        /// Check if we have a valid camera for raycast operations
        /// </summary>
        public static bool HasValidCamera()
        {
            var camera = GetPlayerCamera();
            return camera != null && camera.gameObject.activeInHierarchy;
        }

        /// <summary>
        /// Get camera info for debugging
        /// </summary>
        public static string GetCameraInfo()
        {
            var playerCam = GetPlayerCamera();
            var mainCam = GetMainCamera();

            return $"PlayerCamera: {(playerCam != null ? playerCam.name : "null")}, " +
                   $"MainCamera: {(mainCam != null ? mainCam.name : "null")}";
        }
    }
} 