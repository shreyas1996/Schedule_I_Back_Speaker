using System;
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
    /// Unified interface for Schedule I App<T> components
    /// Wraps both IL2CPP and Mono App implementations
    /// </summary>
    public interface IApp
    {
        // Basic app properties
        string Name { get; }
        bool IsRunning { get; }
        Sprite? Icon { get; }
        
        // App lifecycle
        void Start();
        void Stop();
        void OnClick(RaycastResult raycastResult);
        
        // Generic data storage
        void SetData(string key, object value);
        T GetData<T>(string key);

        // App<T> specific properties (from Schedule I assembly)
        string AppName { get; set; }
        string IconLabel { get; }
        Sprite AppIcon { get; }
        bool isOpen { get; }

        // App<T> specific methods
        void SetIsOpen(bool open);
        void SetOpen(bool open);
        void SetIsHorizontal(bool horizontal);
        void SetLookOffsetMultiplier(float multiplier);
        
        // App<T> advanced properties
        bool IsHorizontal { get; }
        float LookOffsetMultiplier { get; }
        Transform AppCanvas { get; }
        GameObject AppGameObject { get; }
        
        // App<T> state management
        bool WasOpenLastFrame { get; }
        bool JustOpened { get; }
        bool JustClosed { get; }
        
        // App<T> events and callbacks
        void OnAppOpened();
        void OnAppClosed();
        void OnAppUpdate();
        
        // Phone integration
        void ShortcutClicked();
        void RegisterWithPhone();
        void UnregisterFromPhone();
    }

    /// <summary>
    /// Interface for Phone system wrapper
    /// </summary>
    public interface IPhoneSystem
    {
        bool IsPhoneOpen { get; }
        bool IsPhoneAvailable { get; }
        Transform HomeScreen { get; }
        Transform AppsCanvas { get; }
        Transform AppIcons { get; }
        
        void OpenPhone();
        void ClosePhone();
        void OpenApp(IApp app);
        void CloseApp(IApp app);
        void RegisterApp(IApp app);
        void UnregisterApp(IApp app);
        
        IApp[] GetAllApps();
        IApp? GetApp(string appName);
        bool HasApp(string appName);
    }

    /// <summary>
    /// Interface for PlayerSingleton access
    /// </summary>
    public interface IPlayerSingleton<T> where T : class
    {
        T Instance { get; }
        bool HasInstance { get; }
    }

    /// <summary>
    /// Interface for App Canvas management
    /// </summary>
    public interface IAppCanvas
    {
        Transform Container { get; }
        Transform Topbar { get; }
        Transform Background { get; }
        Transform Content { get; }
        
        void SetTitle(string title);
        void SetBackgroundColor(Color color);
        void ClearContent();
        void AddContent(GameObject content);
        void SetHorizontalMode(bool horizontal);
    }
}