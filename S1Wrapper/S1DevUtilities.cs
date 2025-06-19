using System;

namespace BackSpeakerMod.S1Wrapper
{
    /// <summary>
    /// Wrapper for Schedule One DevUtilities
    /// Handles PlayerSingleton pattern and other development utilities
    /// </summary>
    public static class S1DevUtilities
    {
        /// <summary>
        /// Gets a PlayerSingleton instance with IL2CPP/Mono compatibility
        /// </summary>
        public static T? GetPlayerSingleton<T>() where T : class
        {
#if IL2CPP
            if (S1Environment.IsIl2Cpp)
            {
                try
                {
                    // Handle common PlayerSingleton types
                    if (typeof(T).Name.Contains("Phone"))
                    {
                        var phone = Il2CppScheduleOne.DevUtilities.PlayerSingleton<Il2CppScheduleOne.UI.Phone.Phone>.instance;
                        return phone as T;
                    }
                    
                    // Generic approach for other types
                    var singletonType = typeof(Il2CppScheduleOne.DevUtilities.PlayerSingleton<>).MakeGenericType(typeof(T));
                    var instanceProperty = singletonType.GetProperty("instance");
                    if (instanceProperty != null)
                    {
                        return instanceProperty.GetValue(null) as T;
                    }
                }
                catch (Exception)
                {
                    // Silently handle reflection failures
                }
            }
            return null;
#else
            try
            {
                // Handle common PlayerSingleton types
                if (typeof(T).Name.Contains("Phone"))
                {
                    var phone = ScheduleOne.DevUtilities.PlayerSingleton<ScheduleOne.UI.Phone.Phone>.instance;
                    return phone as T;
                }
                
                // Generic approach for other types
                var singletonType = typeof(ScheduleOne.DevUtilities.PlayerSingleton<>).MakeGenericType(typeof(T));
                var instanceProperty = singletonType.GetProperty("instance");
                if (instanceProperty != null)
                {
                    return instanceProperty.GetValue(null) as T;
                }
            }
            catch (Exception)
            {
                // Silently handle reflection failures
            }
            return null;
#endif
        }

        /// <summary>
        /// Set layer recursively (common utility function)
        /// </summary>
        public static void SetLayerRecursively(UnityEngine.GameObject obj, int layer)
        {
            if (obj == null) return;

#if IL2CPP
            if (S1Environment.IsIl2Cpp)
            {
                try
                {
                    var layerUtilityType = typeof(Il2CppScheduleOne.DevUtilities.LayerUtility);
                    var setLayerMethod = layerUtilityType.GetMethod("SetLayerRecursively");
                    if (setLayerMethod != null)
                    {
                        setLayerMethod.Invoke(null, new object[] { obj, layer });
                        return;
                    }
                }
                catch (Exception)
                {
                    // Fallback to manual implementation
                }
            }
#else
            try
            {
                var layerUtilityType = typeof(ScheduleOne.DevUtilities.LayerUtility);
                var setLayerMethod = layerUtilityType.GetMethod("SetLayerRecursively");
                if (setLayerMethod != null)
                {
                    setLayerMethod.Invoke(null, new object[] { obj, layer });
                    return;
                }
            }
            catch (Exception)
            {
                // Fallback to manual implementation
            }
#endif

            // Fallback manual implementation
            SetLayerRecursivelyFallback(obj, layer);
        }

        private static void SetLayerRecursivelyFallback(UnityEngine.GameObject obj, int layer)
        {
            obj.layer = layer;
            foreach (UnityEngine.Transform child in obj.transform)
            {
                if (child.gameObject != null)
                {
                    SetLayerRecursivelyFallback(child.gameObject, layer);
                }
            }
        }

        /// <summary>
        /// Check if a PlayerSingleton instance exists
        /// </summary>
        public static bool HasPlayerSingleton<T>() where T : class
        {
            return GetPlayerSingleton<T>() != null;
        }

        /// <summary>
        /// Wait for PlayerSingleton to become available (for coroutines)
        /// </summary>
        public static System.Collections.IEnumerator WaitForPlayerSingleton<T>(System.Action<T> onReady) where T : class
        {
            T? instance = null;
            float timeout = 10f; // 10 second timeout
            float elapsed = 0f;

            while (instance == null && elapsed < timeout)
            {
                instance = GetPlayerSingleton<T>();
                if (instance != null)
                {
                    onReady?.Invoke(instance);
                    yield break;
                }
                
                elapsed += UnityEngine.Time.deltaTime;
                yield return null;
            }

            // Timeout reached without finding instance
            if (instance == null)
            {
                // Could log a warning here if logging system is available
            }
        }
    }
} 