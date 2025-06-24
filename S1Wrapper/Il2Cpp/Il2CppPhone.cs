using System;
using UnityEngine;
using UnityEngine.EventSystems;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Il2Cpp
{
#if IL2CPP
    /// <summary>
    /// IL2CPP wrapper for Schedule I Phone - accurately mirrors Phone.cs
    /// </summary>
    public class Il2CppPhone : IPhone
    {
        private readonly Il2CppScheduleOne.UI.Phone.Phone _phone;

        public Il2CppPhone(Il2CppScheduleOne.UI.Phone.Phone phone)
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
            get => Il2CppScheduleOne.UI.Phone.Phone.ActiveApp;
            set => Il2CppScheduleOne.UI.Phone.Phone.ActiveApp = value;
        }

        // Core Methods (from Phone.cs) - exact matches
        public void SetOpenable(bool o) => _phone.SetOpenable(o);
        public void SetIsOpen(bool o) => _phone.SetIsOpen(o);
        public void SetIsHorizontal(bool h) => _phone.SetIsHorizontal(h);
        public void SetLookOffsetMultiplier(float multiplier) => _phone.SetLookOffsetMultiplier(multiplier);
        public void RequestCloseApp() => _phone.RequestCloseApp();
        public bool MouseRaycast(out RaycastResult result)
        {
            // IL2CPP may need special handling for out parameters
            return _phone.MouseRaycast(out result);
        }

        // Events (from Phone.cs) - IL2CPP conversion - currently using simplified approach
        // TODO: Proper IL2CPP Action conversion will be implemented later once core functionality is working
        public Action onPhoneOpened 
        { 
            get => new Action(() => _phone.onPhoneOpened?.Invoke());
            set => _phone.onPhoneOpened = null; // Simplified for now
        }
        
        public Action onPhoneClosed 
        { 
            get => new Action(() => _phone.onPhoneClosed?.Invoke());
            set => _phone.onPhoneClosed = null; // Simplified for now  
        }
        
        public Action closeApps 
        { 
            get => new Action(() => _phone.closeApps?.Invoke());
            set => _phone.closeApps = null; // Simplified for now
        }

        // Unity Component Access
        public Transform Transform => _phone.transform;
        public GameObject GameObject => _phone.gameObject;

        // Helper Properties for easier access
        public bool IsInPortraitMode => !isHorizontal;
        public bool IsInLandscapeMode => isHorizontal;

        // Internal access to wrapped object
        public Il2CppScheduleOne.UI.Phone.Phone InternalPhone => _phone;
    }
#endif
}
