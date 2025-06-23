#if !IL2CPP
using System.Collections.Generic;
using BackSpeakerMod.S1Wrapper.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using BackSpeakerMod.NewBackend.Utils;
using System.Reflection;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    /// <summary>
    /// Mono wrapper for Schedule I App<T> components
    /// Provides access to ProductManagerApp and other app types
    /// </summary>
    public class MonoApp : IApp
    {
        private readonly object _app; // The actual App<T> component
        private readonly Type _appType;
        private readonly GameObject _gameObject;

        public MonoApp(object app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _appType = app.GetType();
            
            // Get the GameObject from the component
            if (_app is Component component)
            {
                _gameObject = component.gameObject;
            }
            else
            {
                throw new ArgumentException("App must be a Unity Component", nameof(app));
            }
        }

        // Basic app properties
        public string Name => GetProperty<string>("AppName") ?? "Unknown App";
        public bool IsRunning => GetProperty<bool>("isOpen");
        public Sprite? Icon => GetProperty<Sprite>("AppIcon");
        
        // App lifecycle
        public void Start() => CallMethod("SetIsOpen", true);
        public void Stop() => CallMethod("SetIsOpen", false);
        public void OnClick(RaycastResult raycastResult) => CallMethod("ShortcutClicked");
        
        // Generic data storage (not supported by Schedule I apps)
        public void SetData(string key, object value)
        {
            NewLoggingSystem.Warning($"SetData not supported by Schedule I apps: {key}", "MonoApp");
        }
        
        public T GetData<T>(string key)
        {
            NewLoggingSystem.Warning($"GetData not supported by Schedule I apps: {key}", "MonoApp");
            return default(T);
        }

        // App<T> specific properties
        public string AppName 
        { 
            get => GetProperty<string>("AppName") ?? "Unknown App";
            set => SetProperty("AppName", value);
        }
        public string IconLabel => GetProperty<string>("IconLabel") ?? "Unknown";
        public Sprite AppIcon => GetProperty<Sprite>("AppIcon");
        public bool isOpen => GetProperty<bool>("isOpen");

        // App<T> specific methods
        public void SetIsOpen(bool open) => CallMethod("SetIsOpen", open);
        public void SetOpen(bool open) => CallMethod("SetOpen", open);
        public void SetIsHorizontal(bool horizontal) => CallMethod("SetIsHorizontal", horizontal);
        public void SetLookOffsetMultiplier(float multiplier) => CallMethod("SetLookOffsetMultiplier", multiplier);
        
        // App<T> advanced properties
        public bool IsHorizontal => GetProperty<bool>("IsHorizontal");
        public float LookOffsetMultiplier => GetProperty<float>("LookOffsetMultiplier");
        public Transform AppCanvas => _gameObject.transform;
        public GameObject AppGameObject => _gameObject;
        
        // App<T> state management
        public bool WasOpenLastFrame => GetProperty<bool>("WasOpenLastFrame");
        public bool JustOpened => GetProperty<bool>("JustOpened");
        public bool JustClosed => GetProperty<bool>("JustClosed");
        
        // App<T> events and callbacks
        public void OnAppOpened() => CallMethod("OnAppOpened");
        public void OnAppClosed() => CallMethod("OnAppClosed");
        public void OnAppUpdate() => CallMethod("OnAppUpdate");
        
        // Phone integration
        public void ShortcutClicked() => CallMethod("ShortcutClicked");
        public void RegisterWithPhone() => CallMethod("RegisterWithPhone");
        public void UnregisterFromPhone() => CallMethod("UnregisterFromPhone");

        private T GetProperty<T>(string propertyName)
        {
            try
            {
                var property = _appType.GetProperty(propertyName);
                if (property != null)
                {
                    var value = property.GetValue(_app);
                    if (value is T typedValue)
                        return typedValue;
                }
            }
            catch (Exception)
            {
                // Silently handle reflection failures
            }
            return default(T);
        }

        private void SetProperty<T>(string propertyName, T value)
        {
            try
            {
                var property = _appType.GetProperty(propertyName);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(_app, value);
                }
            }
            catch (Exception)
            {
                // Silently handle reflection failures
            }
        }

        private void CallMethod(string methodName, params object[] parameters)
        {
            try
            {
                var method = _appType.GetMethod(methodName);
                if (method != null)
                {
                    method.Invoke(_app, parameters);
                }
            }
            catch (Exception)
            {
                // Silently handle reflection failures
            }
        }
    }
}
#endif 