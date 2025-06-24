#if !IL2CPP
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    /// <summary>
    /// Mono wrapper for Schedule I Phone - accurately mirrors Phone.cs
    /// </summary>
    public class MonoPhone : IPhone
    {
        private readonly ScheduleOne.UI.Phone.Phone _phone;

        public MonoPhone(ScheduleOne.UI.Phone.Phone phone)
        {
            _phone = phone ?? throw new ArgumentNullException(nameof(phone));
        }

        // Core Properties (from Phone.cs) - exact matches
        public bool IsOpen => _phone.IsOpen;
        public bool isHorizontal => _phone.isHorizontal;
        public bool isOpenable => _phone.isOpenable;
        public bool FlashlightOn => _phone.FlashlightOn;
        public float ScaledLookOffset => _phone.ScaledLookOffset;

        // Static Properties - exact matches
        public GameObject ActiveApp 
        { 
            get => ScheduleOne.UI.Phone.Phone.ActiveApp;
            set => ScheduleOne.UI.Phone.Phone.ActiveApp = value;
        }

        // Core Methods (from Phone.cs) - exact matches
        public void SetOpenable(bool o) => _phone.SetOpenable(o);
        public void SetIsOpen(bool o) => _phone.SetIsOpen(o);
        public void SetIsHorizontal(bool h) => _phone.SetIsHorizontal(h);
        public void SetLookOffsetMultiplier(float multiplier) => _phone.SetLookOffsetMultiplier(multiplier);
        public void RequestCloseApp() => _phone.RequestCloseApp();
        public bool MouseRaycast(out RaycastResult result) => _phone.MouseRaycast(out result);

        // Events (from Phone.cs) - exact matches
        public Action onPhoneOpened 
        { 
            get => _phone.onPhoneOpened;
            set => _phone.onPhoneOpened = value;
        }
        
        public Action onPhoneClosed 
        { 
            get => _phone.onPhoneClosed;
            set => _phone.onPhoneClosed = value;
        }
        
        public Action closeApps 
        { 
            get => _phone.closeApps;
            set => _phone.closeApps = value;
        }

        // Unity Component Access
        public Transform Transform => _phone.transform;
        public GameObject GameObject => _phone.gameObject;

        // Helper Properties for easier access
        public bool IsInPortraitMode => !isHorizontal;
        public bool IsInLandscapeMode => isHorizontal;

        // Internal access to wrapped object
        public ScheduleOne.UI.Phone.Phone InternalPhone => _phone;
    }
}
#endif
