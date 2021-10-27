using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace NetDaemon.Common
{
    /// <summary>
    /// Application Context implementation for apps derived from INetDaemonAppBase
    /// </summary>
    class AppBaseApplicationContext : ApplicationContext
    {
        private readonly INetDaemonAppBase _appInstance;

        public AppBaseApplicationContext(Type applicationType, string id, IServiceProvider serviceProvider)
            : base(applicationType, id, serviceProvider)
        {
            _appInstance = (INetDaemonAppBase)ActivatorUtilities.GetServiceOrCreateInstance(ServiceProvider, applicationType);
            _appInstance.Id = id;
            if (_appInstance is NetDaemonAppBase appBase)
            {
                appBase.ServiceProvider = ServiceProvider;
            }
            ApplicationInstance = _appInstance;
            Id = id;
        }

        /// <summary>
        /// Constructor from an already instantiated app (used for unit testing)
        /// </summary>
        /// <param name="appInstance"></param>
        /// <param name="serviceProvider"></param>
        public AppBaseApplicationContext(INetDaemonAppBase appInstance, IServiceProvider serviceProvider)
            : base(appInstance.GetType(), appInstance.Id!, serviceProvider)
        {
            _appInstance = appInstance;

            if (appInstance is NetDaemonAppBase appBase)
            {
                appBase.ServiceProvider = ServiceProvider;
            }
            ApplicationInstance = appInstance;
        }

        public override void SetConfigProvider(Action configProvider)
        {
            configProvider();
        }

        public override async Task RestoreStateAsync()
        {
            await _appInstance.StartUpAsync(ServiceProvider.GetRequiredService<INetDaemon>()).ConfigureAwait(false);
            await _appInstance.RestoreAppStateAsync().ConfigureAwait(false);
            if (!_appInstance.IsEnabled)
            {
                await _appInstance.DisposeAsync().ConfigureAwait(false);
            }
        }

        public override void Start()
        {
            // Do nothing, already instantiated 
        }

        public override string? Description => _appInstance.Description;

        public override bool IsEnabled
        {
            get => _appInstance.IsEnabled;
            set => _appInstance.IsEnabled = value;
        }

        public override AppRuntimeInfo RuntimeInfo => _appInstance.RuntimeInfo;
    }
}