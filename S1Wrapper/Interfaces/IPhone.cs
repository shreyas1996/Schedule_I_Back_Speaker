using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BackSpeakerMod.S1Wrapper.Interfaces
{
    /// <summary>
    /// Interface for Schedule I Phone wrapper - matches Phone.cs structure exactly
    /// </summary>
    public interface IPhone
    {
        // Core Properties (from Phone.cs)
        bool IsOpen { get; }
        bool isHorizontal { get; }
        bool isOpenable { get; }
        bool FlashlightOn { get; }
        float ScaledLookOffset { get; }
        
        // Static Properties
        GameObject ActiveApp { get; set; }
        
        // Core Methods (from Phone.cs)
        void SetOpenable(bool o);
        void SetIsOpen(bool o);
        void SetIsHorizontal(bool h);
        void SetLookOffsetMultiplier(float multiplier);
        void RequestCloseApp();
        bool MouseRaycast(out RaycastResult result);
        
        // Events (from Phone.cs)
        Action onPhoneOpened { get; set; }
        Action onPhoneClosed { get; set; }
        Action closeApps { get; set; }
        
        // Unity Component Access
        Transform Transform { get; }
        GameObject GameObject { get; }
        
        // Helper Properties for easier access
        bool IsInPortraitMode { get; }
        bool IsInLandscapeMode { get; }
    }
}
