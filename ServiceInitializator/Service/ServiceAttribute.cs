using System;

namespace Assets.Core.Client.Service {
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ServiceAttribute : Attribute {
        public string PrefabPath = string.Empty;
        public Type[] Dependecies = new Type[0];
        public bool InScene;

        public ServiceAttribute() {
        }
    }
}
