using UnityEngine;
using BackSpeakerMod.Core.System;
using BackSpeakerMod.Configuration;

namespace BackSpeakerMod.Core.Common.Helpers
{
    /// <summary>
    /// GameObject creation and setup utilities
    /// </summary>
    public static class GameObjectHelper
    {
        /// <summary>
        /// Create empty GameObject with name
        /// </summary>
        public static GameObject? CreateEmpty(string name)
        {
            var obj = new GameObject(name);
            LoggingSystem.Debug($"Created empty GameObject: {name}", "Helper");
            return obj;
        }

        /// <summary>
        /// Create GameObject with component
        /// </summary>
        public static T? CreateWithComponent<T>(string name) where T : Component
        {
            var obj = CreateEmpty(name);
            var component = obj?.AddComponent<T>();
            LoggingSystem.Debug($"Created GameObject with {typeof(T).Name}: {name}", "Helper");
            return component;
        }

        /// <summary>
        /// Clone GameObject with new name
        /// </summary>
        public static GameObject? Clone(GameObject original, string? newName = null)
        {
            if (original == null) return null;
            
            var clone = UnityEngine.Object.Instantiate(original);
            if (!string.IsNullOrEmpty(newName))
                clone.name = newName;
                
            LoggingSystem.Debug($"Cloned GameObject: {clone.name}", "Helper");
            return clone;
        }

        /// <summary>
        /// Set object active state with logging
        /// </summary>
        public static void SetActiveState(GameObject obj, bool active)
        {
            if (obj == null) return;
            
            obj.SetActive(active);
            LoggingSystem.Debug($"{obj.name} set to {(active ? "active" : "inactive")}", "Helper");
        }

        /// <summary>
        /// Safely destroy a GameObject with null checking
        /// </summary>
        public static void SafeDestroy(GameObject obj)
        {
            if (obj != null)
            {
                UnityEngine.Object.Destroy(obj);
            }
        }

        /// <summary>
        /// Find child by name recursively
        /// </summary>
        public static Transform? FindChildRecursive(Transform parent, string childName)
        {
            if (parent == null) return null;
            
            // Check direct children first
            var directChild = parent.Find(childName);
            if (directChild != null) return directChild;
            
            // Search recursively in all children
            for (int i = 0; i < parent.childCount; i++)
            {
                var result = FindChildRecursive(parent.GetChild(i), childName);
                if (result != null) return result;
            }
            
            return null;
        }

        /// <summary>
        /// Get or add component safely
        /// </summary>
        public static T? GetOrAddComponent<T>(GameObject obj) where T : Component
        {
            if (obj == null) return null;
            
            var component = obj.GetComponent<T>();
            if (component == null)
            {
                component = obj.AddComponent<T>();
            }
            return component;
        }

        /// <summary>
        /// Destroy all children of a transform
        /// </summary>
        public static void DestroyAllChildren(Transform parent)
        {
            if (parent == null) return;
            
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(parent.GetChild(i).gameObject);
            }
        }
    }
} 