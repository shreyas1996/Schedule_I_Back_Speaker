using UnityEngine;
using System.Collections;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using MelonLoader;
using System;
using PlayerManager = BackSpeakerMod.Core.Common.Managers.PlayerManager;

namespace BackSpeakerMod.Core.Features.Player.Attachment
{
    public class PlayerAttachment
    {
        // State tracking
        private Il2CppScheduleOne.PlayerScripts.Player? currentPlayer = null;
        private GameObject? speakerObject = null;
        private AudioSource? audioSource = null;
        private string lastStatus = "";
        
        // Static reference to current player (set by Harmony patch)
        public static Il2CppScheduleOne.PlayerScripts.Player? CurrentPlayerInstance = null;
        
        /// <summary>
        /// Event fired when speaker is attached to player
        /// </summary>
        public event Action<AudioSource>? OnSpeakerAttached;
        public event Action? OnSpeakerDetached;

        public void Initialize()
        {
            LoggingSystem.Info("Initializing PlayerAttachment with auto-attach enabled", "Audio");
            
            // Subscribe to new player manager events instead of direct player events
            PlayerManager.OnPlayerReady += OnPlayerReady;
            PlayerManager.OnPlayerLost += OnPlayerLost;
            
            // Try immediate attachment if player is already ready
            if (PlayerManager.CurrentPlayer != null)
            {
                LoggingSystem.Info("Player already available, attaching immediately", "Audio");
                AttachSpeakerToPlayer(PlayerManager.CurrentPlayer);
            }
        }

        public void TriggerManualAttachment()
        {
            LoggingSystem.Info("Manual attachment triggered", "Audio");
            
            var player = PlayerManager.CurrentPlayer;
            if (player != null)
            {
                AttachSpeakerToPlayer(player);
            }
            else
            {
                LoggingSystem.Warning("No current player available for manual attachment", "Audio");
            }
        }

        public string GetAttachmentStatus()
        {
            string status;
            if (currentPlayer == null)
            {
                status = "⚠️ Waiting for player...";
            }
            else if (audioSource == null)
            {
                status = "⚠️ Player found, creating speaker...";
            }
            else
            {
                status = "✅ Ready - Speaker attached!";
            }
            
            if (status != lastStatus)
            {
                LoggingSystem.Debug($"Status changed to: {status}", "Audio");
                lastStatus = status;
            }
            
            return status;
        }

        public bool IsAudioReady() => audioSource != null && currentPlayer != null;

        /// <summary>
        /// Handle player ready event from PlayerManager
        /// </summary>
        private void OnPlayerReady(Il2CppScheduleOne.PlayerScripts.Player player)
        {
            LoggingSystem.Info($"Player ready event received: {player.name}", "Audio");
            AttachSpeakerToPlayer(player);
        }

        /// <summary>
        /// Handle player lost event from PlayerManager
        /// </summary>
        private void OnPlayerLost()
        {
            LoggingSystem.Info("Player lost event received, cleaning up speaker", "Audio");
            CleanupSpeaker();
        }

        /// <summary>
        /// Clean up speaker when player is lost
        /// </summary>
        private void CleanupSpeaker()
        {
            try
            {
                if (speakerObject != null)
                {
                    GameObject.Destroy(speakerObject);
                    speakerObject = null;
                }
                
                audioSource = null;
                currentPlayer = null;
                
                OnSpeakerDetached?.Invoke();
                LoggingSystem.Info("Speaker cleanup completed", "Audio");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Error during speaker cleanup: {ex.Message}", "Audio");
            }
        }

        private void AttachSpeakerToPlayer(Il2CppScheduleOne.PlayerScripts.Player player)
        {
            try
            {
                if (currentPlayer == player && audioSource != null)
                {
                    LoggingSystem.Debug("Speaker already attached to this player", "Audio");
                    return;
                }
                
                currentPlayer = player;
                LoggingSystem.Info($"Attaching speaker to player '{player.name}' at {player.transform.position}", "Audio");
                
                // Clean up existing speaker
                if (speakerObject != null)
                {
                    GameObject.Destroy(speakerObject);
                }
                
                // Create speaker object as child of player
                speakerObject = new GameObject("BackSpeaker");
                speakerObject.transform.SetParent(player.transform);
                speakerObject.transform.localPosition = Vector3.zero;
                
                // Add and configure AudioSource
                audioSource = speakerObject.AddComponent<AudioSource>();
                audioSource.volume = 0.5f;
                audioSource.loop = false;
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f; // 2D audio for music
                
                LoggingSystem.Info($"✅ Speaker successfully attached! AudioSource volume: {audioSource.volume}", "Audio");
                
                OnSpeakerAttached?.Invoke(audioSource);
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to attach speaker: {ex}", "Audio");
            }
        }

        public AudioSource GetAudioSource() => audioSource;
        public Il2CppScheduleOne.PlayerScripts.Player GetCurrentPlayer() => currentPlayer;
    }
} 