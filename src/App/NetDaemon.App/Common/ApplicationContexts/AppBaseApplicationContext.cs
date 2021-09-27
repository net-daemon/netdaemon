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
        private readonly INetDaemon _netDaemon;
        private readonly INetDaemonAppBase _appInstance;
        public AppBaseApplicationContext(Type applicationType, string id, IServiceProvider serviceProvider, INetDaemon netDaemon) 
            : base(applicationType, id, serviceProvider, netDaemon)
        {
            _netDaemon = netDaemon;
            // INetDaemonAppBase apps are always instantiated but will be disposed directly if they are disabled
            // this is needed because these app classes implement their own Startup and RestoreAppState
            var app = (INetDaemonAppBase)ActivatorUtilities.GetServiceOrCreateInstance(ServiceProvider, applicationType);
            _appInstance = app ?? throw new InvalidOperationException($"Faild to create instance {applicationType.Name} for of app id {id}");

            app.Id = id;
            ApplicationInstance = app;
        }

        /// <summary>
        /// Constructor from an already instantiated app (used for unit testing)
        /// </summary>
        /// <param name="appInstance"></param>
        /// <param name="id"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="netDaemon"></param>
        public AppBaseApplicationContext(INetDaemonAppBase appInstance, string id, IServiceProvider serviceProvider, INetDaemon netDaemon)
            : base(appInstance.GetType(), id, serviceProvider, netDaemon)
        {
            _appInstance = appInstance;
            ApplicationInstance = appInstance;
            _netDaemon = netDaemon;
        }

        public override void SetConfigProvider(Action configProvider)
        {
            configProvider();
        }

        public override async Task RestoreStateAsync()
        {
            await _appInstance.StartUpAsync(_netDaemon).ConfigureAwait(false);
            await _appInstance.RestoreAppStateAsync().ConfigureAwait(false);
            if (!_appInstance.IsEnabled)
            {
                await _appInstance.DisposeAsync().ConfigureAwait(false);
            }
        }

        public override void InstantiateApp()
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