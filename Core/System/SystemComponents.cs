using System;
using BackSpeakerMod.Core.Features.Player.Attachment;
using BackSpeakerMod.Core.Features.Headphones.Managers;
using BackSpeakerMod.Core.Features.Placement.Managers;
using BackSpeakerMod.Core.Features.Testing.Managers;
using BackSpeakerMod.Core.Features.Audio.Managers;

namespace BackSpeakerMod.Core.System
{
    /// <summary>
    /// Container for all system components
    /// </summary>
    public class SystemComponents
    {
        /// <summary>
        /// Legacy player attachment module
        /// </summary>
        public PlayerAttachment PlayerAttachment { get; set; }

        /// <summary>
        /// Headphone management system
        /// </summary>
        public HeadphoneManager HeadphoneManager { get; set; }

        /// <summary>
        /// Placement management system
        /// </summary>
        public PlacementManager PlacementManager { get; set; }

        /// <summary>
        /// Testing management system
        /// </summary>
        public TestingManager TestingManager { get; set; }

        /// <summary>
        /// Audio management system
        /// </summary>
        public AudioManager AudioManager { get; set; }

        /// <summary>
        /// Event to notify UI when tracks are reloaded
        /// </summary>
        public Action OnTracksReloaded { get; set; }

        /// <summary>
        /// Create system components with dependencies
        /// </summary>
        public static SystemComponents Create()
        {
            LoggingSystem.Info("Creating system components", "System");

            var components = new SystemComponents();

            try
            {
                // Create feature managers
                components.HeadphoneManager = new HeadphoneManager();
                components.PlacementManager = new PlacementManager();
                components.TestingManager = new TestingManager();
                components.AudioManager = new AudioManager();

                // Create legacy modules
                components.PlayerAttachment = new PlayerAttachment();

                LoggingSystem.Info("System components created successfully", "System");
                return components;
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Failed to create system components: {ex.Message}", "System");
                throw;
            }
        }

        /// <summary>
        /// Shutdown all components
        /// </summary>
        public void Shutdown()
        {
            LoggingSystem.Info("Shutting down system components", "System");

            try
            {
                // Shutdown feature managers
                HeadphoneManager?.Shutdown();
                PlacementManager?.Shutdown();
                TestingManager?.Shutdown();
                AudioManager?.Shutdown();

                LoggingSystem.Info("System components shutdown completed", "System");
            }
            catch (Exception ex)
            {
                LoggingSystem.Error($"Exception during component shutdown: {ex.Message}", "System");
            }
        }
    }
} 