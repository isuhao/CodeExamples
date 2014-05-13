using System;
using Service;

namespace Assets {
    public class ApplicationManager : IDisposable {
        private readonly Type[] _instanceTypes = { typeof(_), };

        public ApplicationManagerComponent Component { get; private set; }
        public static ApplicationManager Instance { get; private set; }

        private ApplicationManager(ApplicationManagerComponent component) {
            Component = component;
        }

        /// <summary>
        /// Initialize services in static class _
        /// </summary>
        private void Initialize() {
            foreach (var type in _instanceTypes) {
                ServiceInitializator.InitializeServices(type);
            }
        }

        /// <summary>
        /// Dispose ApplicationManager
        /// </summary>
        public void Dispose() {
            if (Instance == null) {
                return;
            }

            foreach (var type in _instanceTypes) {
                ServiceInitializator.DisposeServices(type);
            }

            Instance = null;
        }

        /// <summary>
        /// Initalize Instance of ApplicationManager
        /// </summary>
        /// <param name="component">ApplicationManagerComponent to have way use MonoBehaviour functions</param>
        public static void Initialize(ApplicationManagerComponent component) {
            if (Instance != null) {
                return;
            }

            Instance = new ApplicationManager(component);
            Instance.Initialize();
        }
    }
}
