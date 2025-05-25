using UnityEngine;
using BackSpeakerMod.Core;
using BackSpeakerMod.UI.Components;
using BackSpeakerMod.Utils;

namespace BackSpeakerMod.UI.Helpers
{
    public static class ComponentFactory
    {
        public static T CreateComponent<T>(string name, Transform parent, BackSpeakerManager manager, RectTransform backgroundRect) where T : MonoBehaviour
        {
            try
            {
                var gameObj = new GameObject(name);
                gameObj.transform.SetParent(parent, false);
                
                var component = gameObj.AddComponent<T>();
                
                // Setup based on component type
                if (component is DisplayPanel displayPanel)
                {
                    displayPanel.Setup(manager, backgroundRect);
                }
                else if (component is MusicControlPanel controlPanel)
                {
                    controlPanel.Setup(manager, parent);
                }
                else if (component is VolumeControl volumeControl)
                {
                    volumeControl.Setup(manager, backgroundRect);
                }
                else if (component is ProgressBar progressBar)
                {
                    progressBar.Setup(manager, backgroundRect);
                }
                // Note: PlaylistPanel now requires BackSpeakerScreen reference, 
                // so it's set up directly in BackSpeakerScreen rather than via factory
                
                LoggerUtil.Info($"ComponentFactory: Created {typeof(T).Name} successfully");
                return component;
            }
            catch (System.Exception ex)
            {
                LoggerUtil.Error($"ComponentFactory: Failed to create {typeof(T).Name}: {ex}");
                return null;
            }
        }
        
        public static bool TryCreateComponent<T>(string name, Transform parent, BackSpeakerManager manager, RectTransform backgroundRect, out T component) where T : MonoBehaviour
        {
            component = CreateComponent<T>(name, parent, manager, backgroundRect);
            return component != null;
        }
    }
} 