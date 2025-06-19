namespace BackSpeakerMod.S1Wrapper.Interfaces
{
    /// <summary>
    /// Interface for Schedule One Console system
    /// Provides access to game console functionality
    /// </summary>
    public interface IConsole
    {
        /// <summary>
        /// Execute a console command
        /// </summary>
        void ExecuteCommand(string command);

        /// <summary>
        /// Check if console is available/active
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Enable or disable console functionality
        /// </summary>
        void SetEnabled(bool enabled);
    }
} 