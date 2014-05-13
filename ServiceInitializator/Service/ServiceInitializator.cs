using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Assets.Common.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Assets.Core.Client.Service {
    /// <summary>
    /// read README.pdf some info
    /// </summary>
    public static class ServiceInitializator {
        private static bool Is(Type targeType, Type baseType) {
            while (targeType != null) {
                if (targeType == baseType) {
                    return true;
                }

                targeType = targeType.BaseType;
            }
            return false;
        }

        private static IService InitializeMono(Type targetType, ServiceAttribute attribute) {
            GameObject serviceInstance = null;

            if (attribute != null) { //find attribute. No attribute == new GameObject + AddComponent
                if (!string.IsNullOrEmpty(attribute.PrefabPath)) {
                    serviceInstance = attribute.PrefabPath.LoadAndInstantiate();
                } else if (attribute.InScene) {
                    var objectOfType = Object.FindObjectOfType(targetType);
                    if (objectOfType != null) {
                        return (IService)objectOfType;
                    }
                }
            }

            if (serviceInstance == null) {
                serviceInstance = new GameObject();
            }

            serviceInstance.name = "" + targetType;
            serviceInstance.transform.parent = ApplicationManager.Instance.Component.transform;
            serviceInstance.transform.position = Vector3.zero;

            var serviceComponent = serviceInstance.GetComponent(targetType) ??
                                   serviceInstance.AddComponent(targetType);

            return (IService)serviceComponent;
        }

        private static IService InitializeService(Type targetType, ServiceAttribute attribute) {
            if (Is(targetType.BaseType, typeof(MonoBehaviour))) {
                return InitializeMono(targetType, attribute);
            }

            return (IService)Activator.CreateInstance(targetType);
        }

        public static void InitializeServices(Type targetType) {
            var properties = targetType.GetProperties(BindingFlags.Static | BindingFlags.Public);

            var initializedTypes = new List<Type>();
            var typesToInitialize = new Dictionary<PropertyInfo, ServiceAttribute>();
            var typesInTarget = properties.Select(p => p.PropertyType);

            foreach (var propertyInfo in properties) {
                var propertyType = propertyInfo.PropertyType;
                if (typeof(IService).IsAssignableFrom(propertyType) == false) {
                    continue;
                }

                var attr = propertyType.GetCustomAttributes(typeof(ServiceAttribute), true).FirstOrDefault() as ServiceAttribute;
                if (attr != null && attr.Dependecies.Any(t => !initializedTypes.Contains(t))) {
                    if (attr.Dependecies.Any(t => !typesInTarget.Contains(t))) {
                        Debug.LogError(string.Format("[ServiceInitializator] There is no such ServiceType [{0}] in load stack", propertyType));
                    } else if (attr.Dependecies.Any(t => t == propertyType)) {
                        Debug.LogError(string.Format("[ServiceInitializator] ServiceType depends on his own class [{0}]", propertyType));
                    } else {
                        typesToInitialize.Add(propertyInfo, attr);
                        continue;
                    }
                }

                var service = InitializeService(propertyType, attr);

                propertyInfo.SetValue(targetType, service, null);
                try {
                    service.Initialize();
                } catch (Exception e) {
                    Debug.LogError(string.Format("[ServiceInitializator] Can't initialize service {0}\r\nException: {1}", propertyType, e.InnerException));
                }

                initializedTypes.Add(propertyType);
            }

            while (typesToInitialize.Count > 0) {
                var newDic = new Dictionary<PropertyInfo, ServiceAttribute>();
                foreach (var pair in typesToInitialize) {
                    if (pair.Value.Dependecies.Any(t => !initializedTypes.Contains(t))) {
                        newDic.Add(pair.Key, pair.Value);
                    } else {
                        var service = InitializeService(pair.Key.PropertyType, pair.Value);
                        pair.Key.SetValue(targetType, service, null);
                        service.Initialize();
                        initializedTypes.Add(pair.Key.PropertyType);
                    }
                }

                typesToInitialize = newDic;
            }
        }

        public static void DisposeServices(Type targetType) {
            var properties = targetType.GetProperties(BindingFlags.Static | BindingFlags.Public);

            foreach (var propertyInfo in properties) {
                var propertyType = propertyInfo.PropertyType;
                if (typeof(IService).IsAssignableFrom(propertyType) == false) {
                    continue;
                }

                var service = propertyInfo.GetValue(targetType, null) as IService;
                if (service != null) {
                    service.Dispose();
                }
            }
        }
    }
}
