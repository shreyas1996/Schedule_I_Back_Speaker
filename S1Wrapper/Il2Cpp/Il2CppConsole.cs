#if IL2CPP
using System;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Il2Cpp
{
    public class Il2CppConsole : IConsole
    {
        private readonly object _console;

        public Il2CppConsole(object console)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
        }

        public void Log(string message)
        {
            try
            {
                var method = _console.GetType().GetMethod("Log");
                method?.Invoke(_console, new object[] { message });
            }
            catch (Exception)
            {
                // Handle reflection failures silently
            }
        }

        public void LogError(string message)
        {
            try
            {
                var method = _console.GetType().GetMethod("LogError");
                method?.Invoke(_console, new object[] { message });
            }
            catch (Exception)
            {
                // Handle reflection failures silently
            }
        }

        public void LogWarning(string message)
        {
            try
            {
                var method = _console.GetType().GetMethod("LogWarning");
                method?.Invoke(_console, new object[] { message });
            }
            catch (Exception)
            {
                // Handle reflection failures silently
            }
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
                    // Use reflection to find available command execution methods
                    var processMethod = _console.GetType().GetMethod("ProcessCommand");
                    if (processMethod != null)
                    {
                        processMethod.Invoke(_console, new object[] { command });
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

        public bool IsAvailable => _console != null && _console.GetType().GetProperty("gameObject")?.GetValue(_console) is UnityEngine.GameObject gameObject && gameObject.activeInHierarchy;

        public void SetEnabled(bool enabled)
        {
            try
            {
                if (_console != null)
                {
                    _console.GetType().GetProperty("gameObject")?.SetValue(_console, _console.GetType().GetProperty("gameObject")?.GetValue(_console) is UnityEngine.GameObject gameObject ? gameObject : null);
                }
            }
            catch (Exception)
            {
                // Handle cases where console control isn't available
            }
        }
    }
}
#endif 