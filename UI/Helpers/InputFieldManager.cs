using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;  // Added for BaseEventData
using Il2CppScheduleOne;
using Il2CppScheduleOne.UI.Phone;
using Il2CppScheduleOne.DevUtilities;
using BackSpeakerMod.Core.System;

namespace BackSpeakerMod.UI.Helpers
{
    /// <summary>
    /// Manages input field focus state and handles keybind/phone state management
    /// </summary>
    public static class InputFieldManager
    {
        private static bool isInputFieldFocused = false;
        private static Phone? cachedPhoneInstance = null;
        private static EventTrigger? currentEventTrigger = null;
        private static bool wasPhoneOpen = false;

        public static void SetupInputField(InputField inputField)
        {
            if (inputField == null) return;

            var eventTrigger = inputField.gameObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = inputField.gameObject.AddComponent<EventTrigger>();
            }

            currentEventTrigger = eventTrigger;

            // Clear any existing triggers
            eventTrigger.triggers.Clear();

            // Add select listener
            var selectEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Select
            };
            selectEntry.callback.AddListener((UnityAction<BaseEventData>)delegate(BaseEventData eventData) { OnInputFieldSelected(inputField); });
            eventTrigger.triggers.Add(selectEntry);

            // Add end edit listener
            inputField.onEndEdit.AddListener((UnityAction<string>)delegate(string value) { OnInputFieldDeselected(value); });

            // Add deselect listener
            var deselectEntry = new EventTrigger.Entry
            {
                eventID = EventTriggerType.Deselect
            };
            deselectEntry.callback.AddListener((UnityAction<BaseEventData>)delegate(BaseEventData eventData) { OnInputFieldDeselected(inputField.text); });
            eventTrigger.triggers.Add(deselectEntry);

            LoggingSystem.Info($"Input field setup complete for {inputField.gameObject.name}", "UI");
        }

        private static void OnInputFieldSelected(InputField inputField)
        {
            if (!isInputFieldFocused)
            {
                LoggingSystem.Debug($"Input field focused: {inputField.name}", "UI");
                isInputFieldFocused = true;

                // Store phone state
                if (cachedPhoneInstance == null)
                {
                    cachedPhoneInstance = PlayerSingleton<Phone>.instance;
                    LoggingSystem.Debug("Caching phone instance", "UI");
                }

                if (cachedPhoneInstance != null)
                {
                    wasPhoneOpen = cachedPhoneInstance.IsOpen;
                    LoggingSystem.Debug($"Current phone state - Open: {wasPhoneOpen}", "UI");
                }

                // Update game state
                GameInput.IsTyping = true;
                DisablePhoneKeybinds();
            }
        }

        private static void OnInputFieldDeselected(string value)
        {
            if (isInputFieldFocused)
            {
                LoggingSystem.Debug($"Input field deselected - Final value: {value}", "UI");
                isInputFieldFocused = false;
                GameInput.IsTyping = false;

                // Only re-enable phone if it was previously open
                if (wasPhoneOpen)
                {
                    if (cachedPhoneInstance != null) 
                    {
                        LoggingSystem.Debug($"Restoring phone state - IsOpen: {cachedPhoneInstance.IsOpen}", "UI");
                    }
                    EnablePhoneKeybinds();
                }
            }
        }

        private static void DisablePhoneKeybinds()
        {
            try
            {
                if (cachedPhoneInstance != null)
                {
                    LoggingSystem.Debug($"Disabling phone keybinds - Current state: {cachedPhoneInstance.enabled}", "UI");
                    // Disable the phone script while typing to prevent keybind interference
                    cachedPhoneInstance.enabled = false;
                    LoggingSystem.Debug("Phone keybinds disabled successfully", "UI");
                }
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to disable phone keybinds: {ex.Message}\n{ex.StackTrace}", "UI");
            }
        }

        private static void EnablePhoneKeybinds()
        {
            try
            {
                if (cachedPhoneInstance != null)
                {
                    // Only re-enable if phone should actually be enabled
                    if (wasPhoneOpen && cachedPhoneInstance.IsOpen)
                    {
                        LoggingSystem.Debug("Re-enabling phone keybinds", "UI");
                        cachedPhoneInstance.enabled = true;
                        LoggingSystem.Debug("Phone keybinds restored successfully", "UI");
                    }
                    else
                    {
                        LoggingSystem.Debug($"Skipping phone keybind restore - WasOpen: {wasPhoneOpen}, IsOpen: {cachedPhoneInstance.IsOpen}", "UI");
                    }
                }
            }
            catch (System.Exception ex)
            {
                LoggingSystem.Error($"Failed to restore phone keybinds: {ex.Message}\n{ex.StackTrace}", "UI");
            }
        }

        public static void Reset()
        {
            LoggingSystem.Debug($"Resetting input field manager - CurrentState: {isInputFieldFocused}", "UI");
            
            if (isInputFieldFocused)
            {
                LoggingSystem.Debug("Cleaning up focused input field state", "UI");
                isInputFieldFocused = false;
                GameInput.IsTyping = false;
                
                // Only attempt to restore phone state if it was open
                if (wasPhoneOpen && cachedPhoneInstance != null)
                {
                    LoggingSystem.Debug($"Attempting to restore phone state - PhoneOpen: {cachedPhoneInstance.IsOpen}", "UI");
                    EnablePhoneKeybinds();
                }
            }

            // Reset event trigger
            if (currentEventTrigger != null)
            {
                LoggingSystem.Debug("Cleaning up event triggers", "UI");
                currentEventTrigger.triggers.Clear();
                currentEventTrigger = null;
            }

            // Reset state
            wasPhoneOpen = false;
            cachedPhoneInstance = null;
            LoggingSystem.Debug("Input field manager reset complete", "UI");
        }
    }
}
