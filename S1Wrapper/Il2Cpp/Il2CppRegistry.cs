using BackSpeakerMod.S1Wrapper.Interfaces;
using System.Collections.Generic;
using System;
using System.Linq;

namespace BackSpeakerMod.S1Wrapper.Il2Cpp
{
#if IL2CPP
    public class Il2CppRegistry : IRegistry
    {
        public void Register<T>(string id, T obj) where T : class
        {
            if (string.IsNullOrEmpty(id) || obj == null)
                return;

            try
            {
                // Try to use the Schedule One Registry if available
                // This might be a static method call
                var registryType = typeof(Il2CppScheduleOne.Registry);
                var registerMethod = registryType.GetMethod("Register", new[] { typeof(string), typeof(object) });
                
                if (registerMethod != null)
                {
                    registerMethod.Invoke(null, new object[] { id, obj });
                }
                else
                {
                    // Fallback: try different method signatures
                    var methods = registryType.GetMethods()
                        .Where(m => m.Name == "Register" || m.Name == "Add")
                        .Where(m => m.GetParameters().Length == 2);
                    
                    foreach (var method in methods)
                    {
                        try
                        {
                            method.Invoke(null, new object[] { id, obj });
                            break;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Registry operations may not always be available
            }
        }

        public T? Get<T>(string id) where T : class
        {
            if (string.IsNullOrEmpty(id))
                return null;

            try
            {
                var registryType = typeof(Il2CppScheduleOne.Registry);
                var getMethod = registryType.GetMethod("Get", new[] { typeof(string) });
                
                if (getMethod != null)
                {
                    var result = getMethod.Invoke(null, new object[] { id });
                    return result as T;
                }
                else
                {
                    // Try alternative method names
                    var methods = registryType.GetMethods()
                        .Where(m => m.Name == "Get" || m.Name == "Find" || m.Name == "Lookup")
                        .Where(m => m.GetParameters().Length == 1);
                    
                    foreach (var method in methods)
                    {
                        try
                        {
                            var result = method.Invoke(null, new object[] { id });
                            if (result is T typedResult)
                                return typedResult;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Registry operations may not always be available
            }

            return null;
        }

        public bool IsRegistered(string id)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            try
            {
                var registryType = typeof(Il2CppScheduleOne.Registry);
                var containsMethod = registryType.GetMethod("Contains", new[] { typeof(string) });
                
                if (containsMethod != null)
                {
                    var result = containsMethod.Invoke(null, new object[] { id });
                    return result is bool boolResult && boolResult;
                }
                else
                {
                    // Fallback: try to get the object and see if it exists
                    return Get<object>(id) != null;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Unregister(string id)
        {
            if (string.IsNullOrEmpty(id))
                return;

            try
            {
                var registryType = typeof(Il2CppScheduleOne.Registry);
                var unregisterMethod = registryType.GetMethod("Unregister", new[] { typeof(string) });
                
                if (unregisterMethod != null)
                {
                    unregisterMethod.Invoke(null, new object[] { id });
                }
                else
                {
                    // Try alternative method names
                    var methods = registryType.GetMethods()
                        .Where(m => m.Name == "Remove" || m.Name == "Delete" || m.Name == "Unregister")
                        .Where(m => m.GetParameters().Length == 1);
                    
                    foreach (var method in methods)
                    {
                        try
                        {
                            method.Invoke(null, new object[] { id });
                            break;
                        }
                        catch
                        {
                            continue;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Registry operations may not always be available
            }
        }

        public IEnumerable<string> GetAllIds()
        {
            try
            {
                var registryType = typeof(Il2CppScheduleOne.Registry);
                var getAllMethod = registryType.GetMethod("GetAllIds") ?? 
                                  registryType.GetMethod("GetKeys") ?? 
                                  registryType.GetMethod("Keys");
                
                if (getAllMethod != null)
                {
                    var result = getAllMethod.Invoke(null, new object[0]);
                    if (result is IEnumerable<string> stringEnum)
                        return stringEnum;
                }
            }
            catch (Exception)
            {
                // Registry operations may not always be available
            }

            return new string[0];
        }

        public void Clear()
        {
            try
            {
                var registryType = typeof(Il2CppScheduleOne.Registry);
                var clearMethod = registryType.GetMethod("Clear");
                
                if (clearMethod != null)
                {
                    clearMethod.Invoke(null, new object[0]);
                }
            }
            catch (Exception)
            {
                // Registry operations may not always be available
            }
        }
    }
#endif
} 