#if !IL2CPP
using System;
using UnityEngine;
using UnityEngine.UI;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    /// <summary>
    /// Mono wrapper for Schedule I HomeScreen - accurately mirrors HomeScreen.cs
    /// </summary>
    public class MonoHomeScreen : IHomeScreen
    {
        private readonly ScheduleOne.UI.Phone.HomeScreen _homeScreen;

        public MonoHomeScreen(ScheduleOne.UI.Phone.HomeScreen homeScreen)
        {
            _homeScreen = homeScreen ?? throw new ArgumentNullException(nameof(homeScreen));
        }

        // Core Properties (from HomeScreen.cs) - exact matches
        public bool isOpen => _homeScreen.isOpen;

        // Core Methods (from HomeScreen.cs) - exact matches
        public void SetIsOpen(bool o) => _homeScreen.SetIsOpen(o);
        public void SetCanvasActive(bool a) => _homeScreen.SetCanvasActive(a);
        
        public Button GenerateAppIcon<T>(IApp prog) where T : class
        {
            // This is tricky - we need to convert our IApp back to the actual App<T>
            // For now, we'll return null and implement later when we have concrete app types
            throw new NotImplementedException("GenerateAppIcon needs concrete App<T> implementation");
        }

        // Unity Component Access
        public Transform Transform => _homeScreen.transform;
        public GameObject GameObject => _homeScreen.gameObject;
        public Canvas canvas => _homeScreen.canvas;

        // Internal access to wrapped object
        public ScheduleOne.UI.Phone.HomeScreen InternalHomeScreen => _homeScreen;
    }
}
#endif 