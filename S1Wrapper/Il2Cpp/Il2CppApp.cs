#if IL2CPP
using System;
using System.Collections.Generic;
using System.Reflection;
using BackSpeakerMod.S1Wrapper.Interfaces;
using BackSpeakerMod.NewBackend.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BackSpeakerMod.S1Wrapper.Il2Cpp
{
    /// <summary>
    /// IL2CPP wrapper for Schedule I App<ProductManagerApp> - accurately mirrors App<T> structure
    /// </summary>
    public class Il2CppApp : IApp
    {
        private readonly Il2CppScheduleOne.UI.App<Il2CppScheduleOne.UI.Phone.ProductManagerApp.ProductManagerApp> _app;

        public Il2CppApp(Il2CppScheduleOne.UI.App<Il2CppScheduleOne.UI.Phone.ProductManagerApp.ProductManagerApp> app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
        }

        // Core Properties (from App<T>.cs) - exact matches
        public bool isOpen => _app.isOpen;
        public string AppName => _app.AppName;
        public string IconLabel => _app.IconLabel;
        public Sprite AppIcon => _app.AppIcon;
        public EOrientation Orientation => (EOrientation)_app.Orientation;
        public bool AvailableInTutorial => _app.AvailableInTutorial;

        // Protected/Internal Properties - using reflection for access
        public RectTransform appContainer => _app.appContainer;
        public RectTransform notificationContainer => GetPrivateField<RectTransform>("notificationContainer");
        public Text notificationText => GetPrivateField<Text>("notificationText");
        public Button appIconButton => GetPrivateField<Button>("appIconButton");

        // Core Methods (from App<T>.cs) - exact matches
        public void SetOpen(bool open) => _app.SetOpen(open);
        public void SetNotificationCount(int amount) => _app.SetNotificationCount(amount);
        public void Exit(ExitAction exit)
        {
            // Convert our ExitAction to the game's ExitAction
            var gameExitAction = new Il2CppScheduleOne.DevUtilities.ExitAction();
            gameExitAction.Used = exit.Used;
            _app.Exit(gameExitAction);
            exit.Used = gameExitAction.Used;
        }

        // Unity Component Access
        public Transform Transform => _app.transform;
        public GameObject GameObject => _app.gameObject;

        // Static App Management - not implemented for individual apps
        public List<IApp> Apps => new List<IApp>(); // Would need static access
        public IApp GetApp(int index) => null; // Would need static access

        // Helper Properties
        public bool IsHorizontal => Orientation == EOrientation.Horizontal;
        public bool IsVertical => Orientation == EOrientation.Vertical;
        public float LookOffsetMultiplier => 1.0f; // Default value

        // Internal access to wrapped object
        public Il2CppScheduleOne.UI.App<Il2CppScheduleOne.UI.Phone.ProductManagerApp.ProductManagerApp> InternalApp => _app;

        /// <summary>
        /// Helper method to access private/protected fields via reflection
        /// </summary>
        private T GetPrivateField<T>(string fieldName) where T : class
        {
            try
            {
                var field = _app.GetType().GetField(fieldName, 
                    System.Reflection.BindingFlags.NonPublic | 
                    System.Reflection.BindingFlags.Instance);
                return field?.GetValue(_app) as T;
            }
            catch
            {
                return null;
            }
        }
    }
}
#endif   