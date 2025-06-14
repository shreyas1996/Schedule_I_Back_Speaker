using BackSpeakerMod.Core.Common.Managers;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using System;
using UnityEngine;
using Il2CppScheduleOne.Audio;

namespace BackSpeakerMod.Core.Features.Audio.Managers
{
    /// <summary>
    /// Manages game audio volume adjustments when BackSpeaker is playing
    /// </summary>
    public class GameAudioManager
    {
        // Volume reduction factors (how much to reduce each audio type)
        private const float MUSIC_REDUCTION_FACTOR = 0.15f;    // Reduce to 15% (85% reduction)
        private const float FX_REDUCTION_FACTOR = 0.40f;       // Reduce to 40% (60% reduction)
        private const float AMBIENT_REDUCTION_FACTOR = 0.30f;  // Reduce to 30% (70% reduction)
        private const float UI_REDUCTION_FACTOR = 0.60f;       // Reduce to 60% (40% reduction)
        private const float VOICE_REDUCTION_FACTOR = 0.50f;    // Reduce to 50% (50% reduction)
        
        // Refresh interval for game audio manager references
        private const float REFRESH_INTERVAL = 5.0f;
        
        // State tracking
        private bool isGameAudioReduced = false;
        private bool hasStoredOriginalVolumes = false;
        
        // Original volume storage
        private float originalMusicVolume = 1.0f;
        private float originalFXVolume = 1.0f;
        private float originalAmbientVolume = 1.0f;
        private float originalUIVolume = 1.0f;
        private float originalVoiceVolume = 1.0f;
        
        // Game audio system references
        private Il2CppScheduleOne.Audio.AudioManager? gameAudioManager;
        private Il2CppScheduleOne.Audio.MusicPlayer? gameMusicPlayer;
        
        // Update tracking
        private float lastRefreshTime = 0f;
        
        /// <summary>
        /// Initialize the game audio manager
        /// </summary>
        public void Initialize()
        {
            try
            {
                RefreshGameAudioReferences();
                LoggingSystem.Debug("GameAudioManager initialized", "GameAudio");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to initialize GameAudioManager: {ex.Message}", "GameAudio");
            }
        }
        
        /// <summary>
        /// Update method to be called regularly to refresh game audio references
        /// </summary>
        public void Update()
        {
            try
            {
                float currentTime = Time.time;
                if (currentTime - lastRefreshTime >= REFRESH_INTERVAL)
                {
                    RefreshGameAudioReferences();
                    lastRefreshTime = currentTime;
                }
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error in GameAudioManager update: {ex.Message}", "GameAudio");
            }
        }
        
        /// <summary>
        /// Refresh references to game audio managers
        /// </summary>
        private void RefreshGameAudioReferences()
        {
            try
            {
                // Try to find the game's AudioManager using Unity's FindObjectOfType
                gameAudioManager = UnityEngine.Object.FindObjectOfType<Il2CppScheduleOne.Audio.AudioManager>();
                
                // Try to find the game's MusicPlayer using Unity's FindObjectOfType
                gameMusicPlayer = UnityEngine.Object.FindObjectOfType<Il2CppScheduleOne.Audio.MusicPlayer>();
                
                LoggingSystem.Debug($"Game audio managers - AudioManager: {(gameAudioManager != null ? "Found" : "Not Found")}, " +
                                  $"MusicPlayer: {(gameMusicPlayer != null ? "Found" : "Not Found")}", "GameAudio");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to refresh game audio references: {ex.Message}", "GameAudio");
                gameAudioManager = null;
                gameMusicPlayer = null;
            }
        }
        
        /// <summary>
        /// Reduce game audio volumes when BackSpeaker starts playing
        /// </summary>
        public void ReduceGameAudio()
        {
            try
            {
                if (isGameAudioReduced)
                {
                    LoggingSystem.Debug("Game audio already reduced, skipping", "GameAudio");
                    return;
                }
                
                if (gameAudioManager == null)
                {
                    RefreshGameAudioReferences();
                    if (gameAudioManager == null)
                    {
                        LoggingSystem.Warning("Cannot reduce game audio - AudioManager not available", "GameAudio");
                        return;
                    }
                }
                
                // Store original volumes if not already stored
                if (!hasStoredOriginalVolumes)
                {
                    try
                    {
                        // Store all volume types using the game's audio system
                        originalMusicVolume = gameAudioManager.GetVolume(EAudioType.Music, false); // Get unscaled volume
                        originalFXVolume = gameAudioManager.GetVolume(EAudioType.FX, false);
                        originalAmbientVolume = gameAudioManager.GetVolume(EAudioType.Ambient, false);
                        originalUIVolume = gameAudioManager.GetVolume(EAudioType.UI, false);
                        originalVoiceVolume = gameAudioManager.GetVolume(EAudioType.Voice, false);
                        
                        LoggingSystem.Debug($"Stored original volumes - " +
                                          $"Music: {originalMusicVolume:F2}, FX: {originalFXVolume:F2}, " +
                                          $"Ambient: {originalAmbientVolume:F2}, UI: {originalUIVolume:F2}, " +
                                          $"Voice: {originalVoiceVolume:F2}", "GameAudio");
                        
                        hasStoredOriginalVolumes = true;
                    }
                    catch (Exception ex)
                    {
                        LoggingSystem.Error($"Failed to store original volumes: {ex.Message}", "GameAudio");
                        return;
                    }
                }
                
                // Apply volume reductions
                try
                {
                    // Reduce individual audio types
                    gameAudioManager.SetVolume(EAudioType.Music, originalMusicVolume * MUSIC_REDUCTION_FACTOR);
                    gameAudioManager.SetVolume(EAudioType.FX, originalFXVolume * FX_REDUCTION_FACTOR);
                    gameAudioManager.SetVolume(EAudioType.Ambient, originalAmbientVolume * AMBIENT_REDUCTION_FACTOR);
                    gameAudioManager.SetVolume(EAudioType.UI, originalUIVolume * UI_REDUCTION_FACTOR);
                    gameAudioManager.SetVolume(EAudioType.Voice, originalVoiceVolume * VOICE_REDUCTION_FACTOR);
                    
                    LoggingSystem.Debug("Applied volume reduction to game audio using game's AudioManager", "GameAudio");
                }
                catch (Exception ex)
                {
                    LoggingSystem.Error($"Failed to apply volume reductions: {ex.Message}", "GameAudio");
                    return;
                }
                
                // Stop game music if available
                if (gameMusicPlayer != null)
                {
                    try
                    {
                        gameMusicPlayer.StopAndDisableTracks();
                        LoggingSystem.Debug("Stopped game music tracks", "GameAudio");
                    }
                    catch (Exception ex)
                    {
                        LoggingSystem.Warning($"Failed to stop game music: {ex.Message}", "GameAudio");
                    }
                }
                
                isGameAudioReduced = true;
                LoggingSystem.Info("Game audio reduced for BackSpeaker playback", "GameAudio");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to reduce game audio: {ex.Message}", "GameAudio");
            }
        }
        
        /// <summary>
        /// Restore game audio volumes when BackSpeaker stops playing
        /// </summary>
        public void RestoreGameAudio()
        {
            try
            {
                if (!isGameAudioReduced)
                {
                    LoggingSystem.Debug("Game audio not reduced, skipping restore", "GameAudio");
                    return;
                }
                
                if (gameAudioManager == null)
                {
                    RefreshGameAudioReferences();
                    if (gameAudioManager == null)
                    {
                        LoggingSystem.Warning("Cannot restore game audio - AudioManager not available", "GameAudio");
                        return;
                    }
                }
                
                // Restore original volumes
                if (hasStoredOriginalVolumes)
                {
                    try
                    {
                        // Restore all volume types
                        gameAudioManager.SetVolume(EAudioType.Music, originalMusicVolume);
                        gameAudioManager.SetVolume(EAudioType.FX, originalFXVolume);
                        gameAudioManager.SetVolume(EAudioType.Ambient, originalAmbientVolume);
                        gameAudioManager.SetVolume(EAudioType.UI, originalUIVolume);
                        gameAudioManager.SetVolume(EAudioType.Voice, originalVoiceVolume);
                        
                        LoggingSystem.Debug("Restored original game audio volumes", "GameAudio");
                    }
                    catch (Exception ex)
                    {
                        LoggingSystem.Error($"Failed to restore volumes: {ex.Message}", "GameAudio");
                    }
                }
                
                isGameAudioReduced = false;
                LoggingSystem.Info("Game audio restored after BackSpeaker playback", "GameAudio");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to restore game audio: {ex.Message}", "GameAudio");
            }
        }
        
        /// <summary>
        /// Reset the game audio manager state
        /// </summary>
        public void Reset()
        {
            try
            {
                // Restore audio if it was reduced
                if (isGameAudioReduced)
                {
                    RestoreGameAudio();
                }
                
                // Reset state
                isGameAudioReduced = false;
                hasStoredOriginalVolumes = false;
                gameAudioManager = null;
                gameMusicPlayer = null;
                
                LoggingSystem.Debug("GameAudioManager reset", "GameAudio");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to reset GameAudioManager: {ex.Message}", "GameAudio");
            }
        }
        
        /// <summary>
        /// Get current game audio status for debugging
        /// </summary>
        public string GetStatus()
        {
            try
            {
                var status = $"GameAudio - Reduced: {isGameAudioReduced}, " +
                           $"HasOriginals: {hasStoredOriginalVolumes}, " +
                           $"AudioMgr: {(gameAudioManager != null ? "OK" : "NULL")}, " +
                           $"MusicPlayer: {(gameMusicPlayer != null ? "OK" : "NULL")}";
                
                if (hasStoredOriginalVolumes)
                {
                    status += $", Originals: M:{originalMusicVolume:F2} F:{originalFXVolume:F2} " +
                             $"A:{originalAmbientVolume:F2} U:{originalUIVolume:F2} V:{originalVoiceVolume:F2}";
                }
                
                return status;
            }
            catch (Exception ex)
            {
                return $"GameAudio - Error: {ex.Message}";
            }
        }
        
        /// <summary>
        /// Cleanup resources
        /// </summary>
        public void Shutdown()
        {
            try
            {
                Reset();
                LoggingSystem.Debug("GameAudioManager shutdown", "GameAudio");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error during GameAudioManager shutdown: {ex.Message}", "GameAudio");
            }
        }
    }
} 