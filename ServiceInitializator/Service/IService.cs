namespace Assets.Core.Client.Service {
    interface IService {
        bool IsInitialized { get; set; }

        void Initialize();
        void Dispose();
    }
}
