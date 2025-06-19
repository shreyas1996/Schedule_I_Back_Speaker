#if !IL2CPP
using System;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoConsole : IConsole
    {
        private readonly ScheduleOne.Console _console;

        public MonoConsole(ScheduleOne.Console console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public void ExecuteCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                return;

            try
            {
                // Try to execute the command through the console
                if (_console != null)
                {
                    // Different ways to execute commands depending on the console API
                    // Try the most common method first
                    if (_console.ProcessCommand != null)
                    {
                        _console.ProcessCommand(command);
                    }
                    else
                    {
                        // Fallback methods if ProcessCommand doesn't exist
                        var executeMethod = _console.GetType().GetMethod("Execute");
                        if (executeMethod != null)
                        {
                            executeMethod.Invoke(_console, new object[] { command });
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Silently handle execution failures
                // Console commands may not always be available
            }
        }

        public bool IsAvailable => _console != null && _console.gameObject.activeInHierarchy;

        public void SetEnabled(bool enabled)
        {
            try
            {
                if (_console != null)
                {
                    _console.gameObject.SetActive(enabled);
                }
            }
            catch (Exception)
            {
                // Handle cases where console control isn't available
            }
        }

        public void Log(string message)
        {
            _console.Log(message);
        }

        public void LogError(string message)
        {
            _console.LogError(message);
        }

        public void LogWarning(string message)
        {
            _console.LogWarning(message);
        }
    }
}
#endif 