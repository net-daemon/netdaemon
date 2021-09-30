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
        //private readonly INetDaemon _netDaemon;
        private readonly INetDaemonAppBase _appInstance;

        public AppBaseApplicationContext(Type applicationType, string id, IServiceProvider serviceProvider)
            : this(CreateInstance(applicationType, serviceProvider), serviceProvider)
        {
            Id = id;
        }
        //     : base(applicationType, id, serviceProvider)
        // {
        //     // INetDaemonAppBase apps are always instantiated but will be disposed directly if they are disabled
        //     // changing this would be a breaking change because these app classes implement their own lifetime management
        //     
        //     var appInstance = (INetDaemonAppBase)ActivatorUtilities.GetServiceOrCreateInstance(ServiceProvider, applicationType);
        //     _appInstance = appInstance ?? throw new InvalidOperationException($"Faild to create instance {applicationType.Name} for of app id {id}");
        //
        //     appInstance.Id = id;
        //     if (appInstance is NetDaemonAppBase appBase)
        //     {
        //         appBase.ServiceProvider = ServiceProvider;
        //     }
        //     ApplicationInstance = appInstance;
        // }

        private static INetDaemonAppBase CreateInstance(Type applicationType, IServiceProvider serviceProvider)
        {
            return (INetDaemonAppBase)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, applicationType);
        }

        /// <summary>
        /// Constructor from an already instantiated app (used for unit testing)
        /// </summary>
        /// <param name="appInstance"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="netDaemon"></param>
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