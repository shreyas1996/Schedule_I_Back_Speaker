using UnityEngine;
using MelonLoader;
using System;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Core.Features.Player.Attachment;
using BackSpeakerMod.Core.Features.Headphones.Managers;
using BackSpeakerMod.Core.Features.Placement.Managers;
using BackSpeakerMod.Core.Features.Testing.Managers;
using BackSpeakerMod.Core.Features.Audio.Managers;
using BackSpeakerMod.Core.Common.Managers;

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
        }

        /// <summary>
        /// Initialize all system components
        /// </summary>
        public bool Initialize()
        {
            if (isInitialized)
            {
                return true;
            }

            try
            {
                // Step 1: Initialize core managers first
                PlayerManager.Initialize();

                // Step 2: Initialize headphones first
                bool headphonesReady = components.HeadphoneManager?.Initialize() ?? false;
                if (!headphonesReady)
                {
                    return false;
                }

                // Step 3: Initialize player attachment (but don't auto-attach speaker yet)
                components.PlayerAttachment?.Initialize();

                // Step 4: Initialize other managers
                components.PlacementManager?.Initialize();
                components.TestingManager?.Initialize();

                // Step 5: Set up coordinated initialization
                if (components.HeadphoneManager != null && components.PlayerAttachment != null)
                {
                    components.HeadphoneManager.SetPlayerAttachment(components.PlayerAttachment);
                }

                // Step 6: Wire up events
                if (components.PlayerAttachment != null)
                {
                    components.PlayerAttachment.OnSpeakerAttached += HandleSpeakerAttached;
                    components.PlayerAttachment.OnSpeakerDetached += HandleSpeakerDetached;
                }

                if (components.AudioManager != null)
                {
                    components.AudioManager.OnTracksReloaded += () => components.OnTracksReloaded?.Invoke();
                }

                isInitialized = true;
                OnInitializationComplete?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// Handle speaker attachment event
        /// </summary>
        private void HandleSpeakerAttached(AudioSource audioSource)
        {
            components.AudioManager?.Initialize(audioSource);
            components.AudioManager?.LoadTracks();
            OnSpeakerAttached?.Invoke(audioSource);
        }

        /// <summary>
        /// Handle speaker detachment event
        /// </summary>
        private void HandleSpeakerDetached()
        {
            components.AudioManager?.Reset();
        }

        /// <summary>
        /// Shutdown the initializer
        /// </summary>
        public void Shutdown()
        {
            PlayerManager.Shutdown();

            if (components.PlayerAttachment != null)
            {
                components.PlayerAttachment.OnSpeakerAttached -= HandleSpeakerAttached;
                components.PlayerAttachment.OnSpeakerDetached -= HandleSpeakerDetached;
            }

            isInitialized = false;
        }
    }
} 