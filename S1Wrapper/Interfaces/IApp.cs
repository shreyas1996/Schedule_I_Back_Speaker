using UnityEngine;
using UnityEngine.EventSystems;

namespace BackSpeakerMod.S1Wrapper.Interfaces
{
    /// <summary>
    /// Exit action for app closing
    /// </summary>
    public enum ExitAction
    {
        Close = 0,
        Minimize = 1,
        Hide = 2
    }

    /// <summary>
    /// Interface for Schedule One App objects
    /// Provides access to phone app functionality
    /// </summary>
    public interface IApp
    {
        string Name { get; }
        bool IsRunning { get; }
        Sprite? Icon { get; }
        
        void Start();
        void Stop();
        void OnClick(RaycastResult raycastResult);
        void SetData(string key, object value);
        T GetData<T>(string key);
        
        // Legacy app properties and methods for compatibility
        string AppName { get; }
        string IconLabel { get; }
        Sprite AppIcon { get; }
        bool isOpen { get; }
        
        void SetIsOpen(bool open);
        void SetOpen(bool open);
        void SetIsHorizontal(bool horizontal);
        void SetLookOffsetMultiplier(float multiplier);
        void RequestCloseApp();
        void SetLookOffset(float lookOffset);
        bool MouseRaycast(out RaycastResult result);
        void SetNotificationCount(int amount);
        void GenerateHomeScreenIcon();
        void ShortcutClicked();
        void Exit(ExitAction exit);
    }
}