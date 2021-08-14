using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;

namespace NetDaemon.Daemon
{
    public class AppInstantiator : IAppInstantiator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly INetDaemon _netDaemon;
        private readonly ILogger _logger;

        public AppInstantiator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            
            _netDaemon = _serviceProvider.GetRequiredService<INetDaemon>();
            _logger = _serviceProvider.GetRequiredService<ILogger>();
        }
        
        public ApplicationContext Instantiate(Type netDaemonAppType, string appId)
        {
            var appInstance = ActivatorUtilities.CreateInstance(_serviceProvider, netDaemonAppType);

            var appContext = new ApplicationContext(appInstance, _netDaemon, _logger)
            {
                Id = appId
            };
            
            return appContext;
        }
    }
}