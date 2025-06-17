using UnityEngine;
using System.Collections;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;
using MelonLoader;
using System;
using BackSpeakerMod.Core.Features.Player;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.Core.Features.Player
{
    public class PlayerAttachment
    {
        // State tracking
        private IPlayer? currentPlayer = null;
        private GameObject? speakerObject = null;
        private AudioSource? audioSource = null;
        
        // Static reference to current player (set by Harmony patch)
        public static IPlayer? CurrentPlayerInstance = null;
        
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
            LoggingSystem.Debug("Detaching speaker", "PlayerAttachment");
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
        private void OnPlayerReady(IPlayer player)
        {
            // Wait for headphones before attaching speaker
        }

        /// <summary>
        /// Handle player lost event from PlayerManager
        /// </summary>
        private void OnPlayerLost(IPlayer player)
        {
            LoggingSystem.Debug("Player lost", "PlayerAttachment");
            CleanupSpeaker(onPlayerLost: true);
        }

        /// <summary>
        /// Clean up speaker when player is lost
        /// </summary>
        private void CleanupSpeaker(bool onPlayerLost = false)
        {
            LoggingSystem.Debug("Cleaning up speaker", "PlayerAttachment");
            try
            {
                // Stop any playing audio first
                if (audioSource != null && audioSource.isPlaying)
                {
                    LoggingSystem.Debug("Stopping audio", "PlayerAttachment");
                    audioSource.Stop();
                }
                
                if (speakerObject != null)
                {
                    LoggingSystem.Debug("Destroying speaker object", "PlayerAttachment");
                    GameObject.Destroy(speakerObject);
                    speakerObject = null;
                }
                
                audioSource = null;
                if (onPlayerLost)
                {
                    currentPlayer = null;
                }
                
                OnSpeakerDetached?.Invoke();
                LoggingSystem.Debug("Speaker detached", "PlayerAttachment");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to cleanup speaker: {ex.Message}", "Audio");
            }
        }

        private void AttachSpeakerToPlayer(IPlayer player)
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
                LoggingSystem.Error($"Failed to attach speaker to player: {ex.Message}", "Audio");
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

        public AudioSource? GetAudioSource() => audioSource;
        public IPlayer? GetCurrentPlayer() => currentPlayer;
    }
}
