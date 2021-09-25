using System;

namespace NetDaemon.Common
{
    class MetaDataFromAppBase : IApplicationMetadata
    {
        private INetDaemonAppBase _appInstance;

        public MetaDataFromAppBase(INetDaemonAppBase appInstance)
        {
            _appInstance = appInstance;
        }

        public string? Id
        {
            get => _appInstance.Id;
            set => _appInstance.Id = value;
        }

        public string? Description => _appInstance.Description;

        public bool IsEnabled
        {
            get => _appInstance.IsEnabled;
            set => _appInstance.IsEnabled = value;
        }

        public AppRuntimeInfo RuntimeInfo => _appInstance.RuntimeInfo;

        public Type AppType => _appInstance.GetType();
    }
}