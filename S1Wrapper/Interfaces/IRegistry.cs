using System.Collections.Generic;

namespace BackSpeakerMod.S1Wrapper.Interfaces
{
    /// <summary>
    /// Interface for Schedule One Registry system
    /// Provides access to game object registration and lookup
    /// </summary>
    public interface IRegistry
    {
        /// <summary>
        /// Register an object with the registry
        /// </summary>
        void Register<T>(string id, T obj) where T : class;

        /// <summary>
        /// Get a registered object by ID
        /// </summary>
        T? Get<T>(string id) where T : class;

        /// <summary>
        /// Check if an object is registered with the given ID
        /// </summary>
        bool IsRegistered(string id);

        /// <summary>
        /// Unregister an object
        /// </summary>
        void Unregister(string id);

        /// <summary>
        /// Get all registered IDs
        /// </summary>
        IEnumerable<string> GetAllIds();

        /// <summary>
        /// Clear all registrations
        /// </summary>
        void Clear();
    }
} 