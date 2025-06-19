#if !IL2CPP
using System.Collections.Generic;
using BackSpeakerMod.S1Wrapper.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using BackSpeakerMod.NewBackend.Utils;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    public class MonoApp : IApp
    {
        private readonly object _app; // Use object to handle generic App<T>
        private readonly Type _appType;

        public MonoApp(object app)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _appType = app.GetType();
        }

        public string Name => GetProperty<string>("AppName") ?? "Unknown App";
        public bool IsRunning => GetProperty<bool>("isOpen");
        public Sprite? Icon => GetProperty<Sprite>("AppIcon");
        
        public void Start() => CallMethod("SetIsOpen", true);
        public void Stop() => CallMethod("SetIsOpen", false);
        public void OnClick(RaycastResult raycastResult) => CallMethod("ShortcutClicked");
        
        public void SetData(string key, object value)
        {
            // Apps don't have generic data storage, so this is a no-op for Mono apps
        }
        
        public T GetData<T>(string key)
        {
            // Apps don't have generic data storage, so return default
            return default(T);
        }

        public string AppName => GetProperty<string>("AppName") ?? "Unknown App";
        public string IconLabel => GetProperty<string>("IconLabel") ?? "Unknown";
        public Sprite AppIcon => GetProperty<Sprite>("AppIcon");
        public bool isOpen => GetProperty<bool>("isOpen");

        public void SetIsOpen(bool open) => CallMethod("SetIsOpen", open);
        public void SetOpen(bool open) => CallMethod("SetOpen", open);
        public void SetIsHorizontal(bool horizontal) => CallMethod("SetIsHorizontal", horizontal);
        public void SetLookOffsetMultiplier(float multiplier) => CallMethod("SetLookOffsetMultiplier", multiplier);
        public void RequestCloseApp() => CallMethod("RequestCloseApp");
        public void SetLookOffset(float lookOffset) => CallMethod("SetLookOffset", lookOffset);
        
        public bool MouseRaycast(out RaycastResult result)
        {
            result = default;
            try
            {
                var method = _appType.GetMethod("MouseRaycast");
                if (method != null)
                {
                    var parameters = new object[] { null };
                    var returnValue = method.Invoke(_app, parameters);
                    if (returnValue is bool success)
                    {
                        if (parameters[0] is RaycastResult raycastResult)
                        {
                            result = raycastResult;
                        }
                        return success;
                    }
                }
            }
            catch (Exception)
            {
                // Silently handle reflection failures
            }
            return false;
        }

        public void SetNotificationCount(int amount) => CallMethod("SetNotificationCount", amount);
        public void GenerateHomeScreenIcon() => CallMethod("GenerateHomeScreenIcon");
        public void ShortcutClicked() => CallMethod("ShortcutClicked");
        public void Exit(ExitAction action) => CallMethod("Exit", action);

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