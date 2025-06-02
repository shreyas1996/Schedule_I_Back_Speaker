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
            PlayerManager.OnPlayerReady += OnPlayerReady;
            PlayerManager.OnPlayerLost += OnPlayerLost;
        }

        public void TriggerManualAttachment()
        {
            var player = PlayerManager.CurrentPlayer;
            if (player != null)
            {
                AttachSpeakerToPlayer(player);
            }
        }

        /// <summary>
        /// Manually detach the speaker from the player
        /// </summary>
        public void DetachSpeaker()
        {
            CleanupSpeaker();
        }

        public string GetAttachmentStatus()
        {
            if (currentPlayer == null)
            {
                return "⚠️ Waiting for player...";
            }
            else if (audioSource == null)
            {
                return "⚠️ Player found, creating speaker...";
            }
            else
            {
                return "✅ Ready - Speaker attached!";
            }
        }

        public bool IsAudioReady() => audioSource != null && currentPlayer != null;

        /// <summary>
        /// Handle player ready event from PlayerManager
        /// </summary>
        private void OnPlayerReady(Il2CppScheduleOne.PlayerScripts.Player player)
        {
            // Wait for headphones before attaching speaker
        }

        /// <summary>
        /// Handle player lost event from PlayerManager
        /// </summary>
        private void OnPlayerLost()
        {
            CleanupSpeaker();
        }

        /// <summary>
        /// Clean up speaker when player is lost
        /// </summary>
        private void CleanupSpeaker()
        {
            try
            {
                // Stop any playing audio first
                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Stop();
                }
                
                if (speakerObject != null)
                {
                    GameObject.Destroy(speakerObject);
                    speakerObject = null;
                }
                
                audioSource = null;
                currentPlayer = null;
                
                OnSpeakerDetached?.Invoke();
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to cleanup speaker: {ex.Message}", "Audio");
            }
        }

        private void AttachSpeakerToPlayer(Il2CppScheduleOne.PlayerScripts.Player player)
        {
            try
            {
                if (currentPlayer == player && audioSource != null)
                {
                    return;
                }
                
                currentPlayer = player;
                
                if (speakerObject != null)
                {
                    GameObject.Destroy(speakerObject);
                }
                
                speakerObject = new GameObject("BackSpeaker");
                speakerObject.transform.SetParent(player.transform);
                speakerObject.transform.localPosition = Vector3.zero;
                
                audioSource = speakerObject.AddComponent<AudioSource>();
                audioSource.volume = 0.5f;
                audioSource.loop = false;
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
                
                OnSpeakerAttached?.Invoke(audioSource);
            }
            catch (Exception ex)
            {
                // Silent attachment failure
            }
        }

        /// <summary>
        /// Attach speaker when headphones are confirmed (called by HeadphoneManager)
        /// </summary>
        public void AttachSpeakerWithHeadphones()
        {
            var player = PlayerManager.CurrentPlayer;
            if (player != null)
            {
                AttachSpeakerToPlayer(player);
            }
        }

        public AudioSource GetAudioSource() => audioSource;
        public Il2CppScheduleOne.PlayerScripts.Player GetCurrentPlayer() => currentPlayer;
    }
} 