using System;
using System.Collections.Generic;
using UnityEngine;

namespace BackSpeakerMod.Core.Modules
{
    /// <summary>
    /// Types of music sources available
    /// </summary>
    public enum MusicSourceType
    {
        Jukebox,    // In-game jukebox music (for testing)
        LocalFolder, // User's local music folder
        YouTube     // YouTube streaming/downloading
    }

    /// <summary>
    /// Interface for different music source providers
    /// </summary>
    public interface IMusicSourceProvider
    {
        MusicSourceType SourceType { get; }
        string DisplayName { get; }
        bool IsAvailable { get; }
        
        /// <summary>
        /// Load tracks from this source
        /// </summary>
        void LoadTracks(Action<List<AudioClip>, List<(string title, string artist)>> onComplete);
        
        /// <summary>
        /// Get source-specific configuration options
        /// </summary>
        Dictionary<string, object> GetConfiguration();
        
        /// <summary>
        /// Apply configuration changes
        /// </summary>
        void ApplyConfiguration(Dictionary<string, object> config);
        
        /// <summary>
        /// Clean up any resources
        /// </summary>
        void Cleanup();
    }

    /// <summary>
    /// Event args for music source changes
    /// </summary>
    public class MusicSourceChangedEventArgs : EventArgs
    {
        public MusicSourceType PreviousSource { get; }
        public MusicSourceType NewSource { get; }
        public bool IsLoadingComplete { get; }
        public string StatusMessage { get; }

        public MusicSourceChangedEventArgs(MusicSourceType previousSource, MusicSourceType newSource, 
            bool isLoadingComplete = false, string statusMessage = "")
        {
            PreviousSource = previousSource;
            NewSource = newSource;
            IsLoadingComplete = isLoadingComplete;
            StatusMessage = statusMessage;
        }
    }
} 