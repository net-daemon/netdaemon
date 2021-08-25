using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;

namespace NetDaemon.Daemon
{
    internal class AppInstantiator : IAppInstantiator
    {
        private readonly IServiceProvider _serviceProvider;

        public AppInstantiator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ApplicationContext Instantiate(Type netDaemonAppType, string appId)
        {
            IServiceScope? serviceScope = null;
            try
            {
                serviceScope = _serviceProvider.CreateScope();
                // The AppContext will dispose the ServiceScope

                var appInstance = ActivatorUtilities.CreateInstance(serviceScope.ServiceProvider, netDaemonAppType);

                var appContext = new ApplicationContext(appInstance)
                {
                    Id = appId
                };
                appContext.TrackDisposable(serviceScope);
                return appContext;
            }
            catch
            {
                serviceScope?.Dispose();
                throw;
            }
        }
    }
}