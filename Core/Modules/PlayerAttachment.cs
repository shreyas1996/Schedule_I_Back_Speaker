using UnityEngine;
using System.Collections;
using Il2CppScheduleOne.PlayerScripts;
using BackSpeakerMod.Utils;
using MelonLoader;

namespace BackSpeakerMod.Core.Modules
{
    public class PlayerAttachment
    {
        private GameObject speakerObject = null;
        private AudioSource audioSource = null;
        private Player currentPlayer = null;
        private string lastStatus = "";
        
        // Static reference to current player (set by Harmony patch)
        public static Player CurrentPlayerInstance = null;
        
        public System.Action<AudioSource> OnSpeakerAttached;
        public System.Action OnSpeakerDetached;

        public void Initialize()
        {
            LoggerUtil.Info("PlayerAttachment: Initializing with auto-attach enabled");
            // Subscribe to player spawn event for auto-attachment
            Player.onLocalPlayerSpawned += (Il2CppSystem.Action)(OnLocalPlayerSpawned);
            
            // Try immediate attachment if player is already spawned
            TryInitialAttachment();
        }

        public void TriggerManualAttachment()
        {
            LoggerUtil.Info("PlayerAttachment: Manual attachment triggered");
            TryInitialAttachment();
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
                LoggerUtil.Info($"PlayerAttachment: Status changed to: {status}");
                lastStatus = status;
            }
            
            return status;
        }

        public bool IsAudioReady() => audioSource != null && currentPlayer != null;

        private void TryInitialAttachment()
        {
            var player = GetPlayerInstance();
            if (player != null)
            {
                LoggerUtil.Info("PlayerAttachment: Player found, attempting immediate attachment");
                AttachSpeakerToPlayer(player);
            }
            else
            {
                LoggerUtil.Info("PlayerAttachment: Player not found, starting poll coroutine");
                MelonCoroutines.Start(PollForPlayer());
            }
        }

        private Player GetPlayerInstance()
        {
            // Try static reference first (set by Harmony patch)
            if (CurrentPlayerInstance != null)
                return CurrentPlayerInstance;
                
            // Fallback: Find player in scene
            try
            {
                var player = GameObject.FindObjectOfType<Player>();
                if (player != null)
                {
                    CurrentPlayerInstance = player; // Cache for next time
                    return player;
                }
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Warn($"PlayerAttachment: Error finding player: {ex.Message}");
            }
            
            return null;
        }

        private System.Collections.IEnumerator PollForPlayer()
        {
            LoggerUtil.Info("PlayerAttachment: Polling for player...");
            int attempts = 0;
            while (GetPlayerInstance() == null && attempts < 100)
            {
                attempts++;
                if (attempts % 10 == 0)
                    LoggerUtil.Info($"PlayerAttachment: Still waiting for player... (attempt {attempts}/100)");
                yield return new WaitForSeconds(0.5f);
            }
            
            var player = GetPlayerInstance();
            if (player != null)
            {
                LoggerUtil.Info("PlayerAttachment: Player found via polling!");
                AttachSpeakerToPlayer(player);
            }
            else
            {
                LoggerUtil.Warn("PlayerAttachment: Failed to find player after polling. Manual trigger may be needed.");
            }
        }

        private void OnLocalPlayerSpawned()
        {
            LoggerUtil.Info("PlayerAttachment: Local player spawned event triggered!");
            var player = GetPlayerInstance();
            if (player != null)
            {
                AttachSpeakerToPlayer(player);
            }
            else
            {
                LoggerUtil.Warn("PlayerAttachment: Player spawned event but player not found, retrying...");
                MelonCoroutines.Start(RetryAttachment());
            }
        }

        private void AttachSpeakerToPlayer(Player player)
        {
            try
            {
                if (currentPlayer == player && audioSource != null)
                {
                    LoggerUtil.Info("PlayerAttachment: Speaker already attached to this player");
                    return;
                }
                
                currentPlayer = player;
                LoggerUtil.Info($"PlayerAttachment: Attaching speaker to player '{player.name}' at {player.transform.position}");
                
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
                
                LoggerUtil.Info($"PlayerAttachment: ✅ Speaker successfully attached! AudioSource volume: {audioSource.volume}");
                
                OnSpeakerAttached?.Invoke(audioSource);
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"PlayerAttachment: Failed to attach speaker: {ex}");
            }
        }

        private System.Collections.IEnumerator RetryAttachment()
        {
            yield return new WaitForSeconds(1f);
            var player = GetPlayerInstance();
            if (player != null)
                AttachSpeakerToPlayer(player);
        }

        public AudioSource GetAudioSource() => audioSource;
        public Player GetCurrentPlayer() => currentPlayer;
    }
} 