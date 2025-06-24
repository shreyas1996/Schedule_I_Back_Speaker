#if !IL2CPP
using System;
using UnityEngine;
using BackSpeakerMod.S1Wrapper.Interfaces;

namespace BackSpeakerMod.S1Wrapper.Mono
{
    /// <summary>
    /// Mono wrapper for Schedule I AppsCanvas - accurately mirrors AppsCanvas.cs
    /// </summary>
    public class MonoAppsCanvas : IAppsCanvas
    {
        private readonly ScheduleOne.UI.Phone.AppsCanvas _appsCanvas;

        public MonoAppsCanvas(ScheduleOne.UI.Phone.AppsCanvas appsCanvas)
        {
            _appsCanvas = appsCanvas ?? throw new ArgumentNullException(nameof(appsCanvas));
        }

        // Core Properties (from AppsCanvas.cs) - exact matches
        public bool isOpen => _appsCanvas.isOpen;

        // Core Methods (from AppsCanvas.cs) - exact matches
        public void SetIsOpen(bool o) => _appsCanvas.SetIsOpen(o);

        // Unity Component Access
        public Transform Transform => _appsCanvas.transform;
        public GameObject GameObject => _appsCanvas.gameObject;
        public Canvas canvas => _appsCanvas.canvas;

        // Internal access to wrapped object
        public ScheduleOne.UI.Phone.AppsCanvas InternalAppsCanvas => _appsCanvas;
    }
}
#endif