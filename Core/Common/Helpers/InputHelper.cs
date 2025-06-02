using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;

namespace BackSpeakerMod.Core.Common.Helpers
{
    /// <summary>
    /// Input detection utilities
    /// </summary>
    public static class InputHelper
    {
        /// <summary>
        /// Check if key was pressed this frame
        /// </summary>
        public static bool IsKeyPressed(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }

        /// <summary>
        /// Check if key is currently held
        /// </summary>
        public static bool IsKeyHeld(KeyCode key)
        {
            return Input.GetKey(key);
        }

        /// <summary>
        /// Check if mouse button was clicked this frame
        /// </summary>
        public static bool IsMouseClicked(int button = 0)
        {
            return Input.GetMouseButtonDown(button);
        }

        /// <summary>
        /// Get mouse position in screen coordinates
        /// </summary>
        public static Vector3 GetMousePosition()
        {
            return Input.mousePosition;
        }

        /// <summary>
        /// Check if escape key was pressed
        /// </summary>
        public static bool IsEscapePressed()
        {
            return IsKeyPressed(KeyCode.Escape);
        }

        /// <summary>
        /// Check if any modifier key is held
        /// </summary>
        public static bool IsModifierHeld()
        {
            return Input.GetKey(KeyCode.LeftControl) || 
                   Input.GetKey(KeyCode.RightControl) ||
                   Input.GetKey(KeyCode.LeftShift) || 
                   Input.GetKey(KeyCode.RightShift) ||
                   Input.GetKey(KeyCode.LeftAlt) || 
                   Input.GetKey(KeyCode.RightAlt);
        }
    }
} 