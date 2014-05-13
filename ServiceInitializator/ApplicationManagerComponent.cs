using System;
using UnityEngine;

namespace Assets {
    /// <summary>
    /// ApplicationManagerComponent class that listening application Start and Finish and creates ApplicationManager instance or Dispose it
    /// </summary>
    public class ApplicationManagerComponent : MonoBehaviour {
        /// <summary>
        /// Create ApplicationManager instance
        /// </summary>
        protected void Awake() {
            ApplicationManager.Initialize(this);
        }

        /// <summary>
        /// Dispose ApplicationManager
        /// </summary>
        protected void OnDestroy() {
            ApplicationManager.Instance.Dispose();
        }
    }
}
