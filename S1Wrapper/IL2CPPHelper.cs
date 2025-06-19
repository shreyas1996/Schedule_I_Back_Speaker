#if IL2CPP
using System;
using UnityEngine;

namespace BackSpeakerMod.S1Wrapper
{
    /// <summary>
    /// Comprehensive IL2CPP Helper utilities
    /// Handles type registration, interop, and IL2CPP-specific operations
    /// </summary>
    public static class IL2CPPHelper
    {
        private static bool _initialized = false;

        /// <summary>
        /// Initialize IL2CPP helper system
        /// </summary>
        public static void Initialize()
        {
            if (_initialized) return;

            if (S1Environment.IsIl2Cpp)
            {
                try
                {
                    // Perform any necessary IL2CPP initialization
                    _initialized = true;
                }
                catch (Exception)
                {
                    _initialized = false;
                }
            }
        }

        public static Il2CppAssetBundle? LoadFromFile(string path)
        {
            if (S1Environment.IsIl2Cpp)
            {
                return Il2CppAssetBundleManager.LoadFromFile(path);
            }
            return null;
        }

        public static Il2CppAssetBundle? LoadFromMemory(byte[] data)
        {
            if (S1Environment.IsIl2Cpp)
            {
                return Il2CppAssetBundleManager.LoadFromMemory(data);
            }
            return null;
        }

        /// <summary>
        /// Register a type in IL2CPP runtime
        /// </summary>
        public static void RegisterIl2CppType<T>() where T : UnityEngine.Object
        {
            if (S1Environment.IsIl2Cpp)
            {
                try
                {
                    Il2CppInterop.Runtime.Injection.ClassInjector.RegisterTypeInIl2Cpp<T>();
                }
                catch (Exception)
                {
                    // Silently handle registration failures
                    // Type might already be registered
                }
            }
        }

        /// <summary>
        /// Register multiple types at once
        /// </summary>
        public static void RegisterIl2CppTypes(params Type[] types)
        {
            if (S1Environment.IsIl2Cpp)
            {
                foreach (var type in types)
                {
                    try
                    {
                        if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                        {
                            var method = typeof(Il2CppInterop.Runtime.Injection.ClassInjector)
                                .GetMethod("RegisterTypeInIl2Cpp")?.MakeGenericMethod(type);
                            method?.Invoke(null, new object[0]);
                        }
                    }
                    catch (Exception)
                    {
                        // Continue with other types
                    }
                }
            }
        }

        /// <summary>
        /// Create IL2CPP-compatible GameObject with component
        /// </summary>
        public static GameObject CreateIl2CppGameObject<T>(string name) where T : Component
        {
            var gameObject = new GameObject(name);
            
            if (S1Environment.IsIl2Cpp)
            {
                try
                {
                    // Ensure type is registered before adding component
                    RegisterIl2CppType<T>();
                    var component = gameObject.AddComponent<T>();
                    return gameObject;
                }
                catch (Exception)
                {
                    // Fallback to regular GameObject
                    UnityEngine.Object.Destroy(gameObject);
                    return new GameObject(name);
                }
            }
            return gameObject;
        }

        /// <summary>
        /// Safe IL2CPP component addition
        /// </summary>
        public static T? AddIl2CppComponent<T>(GameObject gameObject) where T : Component
        {
            if (S1Environment.IsIl2Cpp)
            {
                try
                {
                    RegisterIl2CppType<T>();
                    return gameObject.AddComponent<T>();
                }
                catch (Exception)
                {
                    return null;
                }
            }
            try
            {
                return gameObject.AddComponent<T>();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Convert IL2CPP object to regular object (if needed)
        /// </summary>
        public static T? ConvertFromIl2Cpp<T>(object il2cppObject) where T : class
        {
            if (S1Environment.IsIl2Cpp && il2cppObject != null)
            {
                try
                {
                    // Handle IL2CPP to managed object conversion
                    if (il2cppObject is T directCast)
                        return directCast;

                    // Try IL2CPP specific conversions
                    var objectType = il2cppObject.GetType();
                    if (objectType.Name.StartsWith("Il2Cpp"))
                    {
                        // This might need specific handling based on the type
                        return il2cppObject as T;
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return il2cppObject as T;
        }

        /// <summary>
        /// Safe method invocation on IL2CPP objects
        /// </summary>
        public static object? InvokeIl2CppMethod(object target, string methodName, params object[] parameters)
        {
            if (target == null) return null;

            try
            {
                var method = target.GetType().GetMethod(methodName);
                if (method != null)
                {
                    return method.Invoke(target, parameters);
                }
            }
            catch (Exception)
            {
                // Silently handle method invocation failures
            }
            return null;
        }

        /// <summary>
        /// Safe property access on IL2CPP objects
        /// </summary>
        public static T? GetIl2CppProperty<T>(object target, string propertyName)
        {
            if (target == null) return default(T);

            try
            {
                var property = target.GetType().GetProperty(propertyName);
                if (property != null)
                {
                    var value = property.GetValue(target);
                    if (value is T typedValue)
                        return typedValue;
                }
            }
            catch (Exception)
            {
                // Silently handle property access failures
            }
            return default(T);
        }

        /// <summary>
        /// Safe property setting on IL2CPP objects
        /// </summary>
        public static bool SetIl2CppProperty(object target, string propertyName, object value)
        {
            if (target == null) return false;

            try
            {
                var property = target.GetType().GetProperty(propertyName);
                if (property != null && property.CanWrite)
                {
                    property.SetValue(target, value);
                    return true;
                }
            }
            catch (Exception)
            {
                // Silently handle property setting failures
            }
            return false;
        }

        /// <summary>
        /// Check if an object is an IL2CPP object
        /// </summary>
        public static bool IsIl2CppObject(object obj)
        {
            if (obj == null) return false;
            return obj.GetType().Namespace?.Contains("Il2Cpp") == true || 
                   obj.GetType().Name.StartsWith("Il2Cpp");
        }

        /// <summary>
        /// Get IL2CPP type name from regular type name
        /// </summary>
        public static string GetIl2CppTypeName(string regularTypeName)
        {
            if (string.IsNullOrEmpty(regularTypeName)) return "";
            
            // Handle common type name conversions
            if (regularTypeName.StartsWith("ScheduleOne."))
            {
                return regularTypeName.Replace("ScheduleOne.", "Il2CppScheduleOne.");
            }
            
            return "Il2Cpp" + regularTypeName;
        }

        /// <summary>
        /// Create wrapper for IL2CPP object
        /// </summary>
        public static object? CreateWrapperForIl2CppObject(object il2cppObject, Type wrapperType)
        {
            if (il2cppObject == null || wrapperType == null) return null;

            try
            {
                // Try to create wrapper instance
                var constructor = wrapperType.GetConstructor(new[] { il2cppObject.GetType() });
                if (constructor != null)
                {
                    return constructor.Invoke(new[] { il2cppObject });
                }

                // Try generic object constructor
                var genericConstructor = wrapperType.GetConstructor(new[] { typeof(object) });
                if (genericConstructor != null)
                {
                    return genericConstructor.Invoke(new[] { il2cppObject });
                }
            }
            catch (Exception)
            {
                // Silently handle wrapper creation failures
            }

            return null;
        }

        /// <summary>
        /// Batch register common Schedule One types
        /// </summary>
        public static void RegisterCommonScheduleOneTypes()
        {   
            if (!S1Environment.IsIl2Cpp) return;

            try
            {
                // Register common Unity types that might be needed
                var commonTypes = new[]
                {
                    typeof(MonoBehaviour),
                    typeof(Component),
                    typeof(Transform),
                    typeof(GameObject),
                    typeof(Canvas),
                    typeof(UnityEngine.UI.Button),
                    typeof(UnityEngine.UI.Text),
                    typeof(UnityEngine.UI.Image)
                };

                RegisterIl2CppTypes(commonTypes);
            }
            catch (Exception)
            {
                // Silently handle batch registration failures
            }
        }

        /// <summary>
        /// Debug information about IL2CPP state
        /// </summary>
        public static string GetIl2CppDebugInfo()
        {
            var info = $"IL2CPP Helper Status:\n";
            info += $"Environment: {(S1Environment.IsIl2Cpp ? "IL2CPP" : "Mono")}\n";
            info += $"Initialized: {_initialized}\n";

            info += $"IL2CPP Runtime Available: {S1Environment.IsIl2Cpp}\n";
            if (S1Environment.IsIl2Cpp)
            {
                try
                {
                    var injectorType = typeof(Il2CppInterop.Runtime.Injection.ClassInjector);
                    info += $"ClassInjector Available: {injectorType != null}\n";
                }
                catch (Exception ex)
                {
                    info += $"ClassInjector Error: {ex.Message}\n";
                }
            }

            return info;
        }
    }
}
#endif
