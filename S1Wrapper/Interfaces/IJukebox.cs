using UnityEngine;
using System.Collections.Generic;

namespace BackSpeakerMod.S1Wrapper.Interfaces
{
    /// <summary>
    /// Interface for Schedule One Jukebox objects
    /// Provides access to music tracks and jukebox functionality
    /// </summary>
    public interface IJukebox
    {
        /// <summary>
        /// The Unity GameObject representing this jukebox
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// The transform of this jukebox
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// The name of this jukebox
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets all audio tracks available in this jukebox
        /// </summary>
        List<AudioClip> GetTracks();

        /// <summary>
        /// Gets the number of tracks in this jukebox
        /// </summary>
        int TrackCount { get; }

        /// <summary>
        /// Whether this jukebox has any tracks
        /// </summary>
        bool HasTracks { get; }

        /// <summary>
        /// Gets a specific track by index
        /// </summary>
        AudioClip? GetTrack(int index);

        /// <summary>
        /// Checks if the jukebox is active and usable
        /// </summary>
        bool IsActive { get; }
    }
} 