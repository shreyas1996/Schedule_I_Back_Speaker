using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BackSpeakerMod.S1Wrapper.Interfaces
{
    /// <summary>
    /// App orientation enum - matches App<T>.EOrientation exactly
    /// </summary>
    public enum EOrientation
    {
        Horizontal,
        Vertical
    }

    /// <summary>
    /// Exit action for app closing
    /// </summary>
    public class ExitAction
    {
        public bool Used { get; set; }
    }

    /// <summary>
    /// Interface for Schedule I App<T> wrapper - matches App<T> structure exactly
    /// </summary>
    public interface IApp
    {
        // Core Properties (from App<T>.cs)
        bool isOpen { get; }
        string AppName { get; }
        string IconLabel { get; }
        Sprite AppIcon { get; }
        EOrientation Orientation { get; }
        bool AvailableInTutorial { get; }
        
        // Protected/Internal Properties
        RectTransform appContainer { get; }
        RectTransform notificationContainer { get; }
        Text notificationText { get; }
        Button appIconButton { get; }
        
        // Core Methods (from App<T>.cs)  
        void SetOpen(bool open);
        void SetNotificationCount(int amount);
        void Exit(ExitAction exit);
        
        // Unity Component Access
        Transform Transform { get; }
        GameObject GameObject { get; }
        
        // Static App Management
        List<IApp> Apps { get; }
        IApp GetApp(int index);
        
        // Helper Properties
        bool IsHorizontal { get; }
        bool IsVertical { get; }
        float LookOffsetMultiplier { get; }
    }

    /// <summary>
    /// Interface for HomeScreen wrapper - matches HomeScreen.cs structure exactly
    /// </summary>
    public interface IHomeScreen
    {
        // Core Properties (from HomeScreen.cs)
        bool isOpen { get; }
        
        // Core Methods (from HomeScreen.cs)
        void SetIsOpen(bool o);
        void SetCanvasActive(bool a);
        Button GenerateAppIcon<T>(IApp prog) where T : class;
        
        // Unity Component Access
        Transform Transform { get; }
        GameObject GameObject { get; }
        Canvas canvas { get; }
    }

    /// <summary>
    /// Interface for AppsCanvas wrapper - matches AppsCanvas.cs structure exactly
    /// </summary>
    public interface IAppsCanvas
    {
        // Core Properties (from AppsCanvas.cs)
        bool isOpen { get; }
        
        // Core Methods (from AppsCanvas.cs)
        void SetIsOpen(bool o);
        
        // Unity Component Access
        Transform Transform { get; }
        GameObject GameObject { get; }
        Canvas canvas { get; }
    }


}