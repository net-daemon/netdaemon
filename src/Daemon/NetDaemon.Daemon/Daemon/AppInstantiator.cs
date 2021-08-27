using System;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Common;

namespace NetDaemon.Daemon
{
    internal class AppInstantiator : IAppInstantiator
    {
        public IServiceProvider ServiceProvider { get; }

        public AppInstantiator(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public ApplicationContext Instantiate(Type applicationType, string appId)
        {
            IServiceScope? serviceScope = null;
            try
            {
                // The AppContext will dispose the ServiceScope

                var scope = ServiceProvider.CreateScope();
            
                var app = ActivatorUtilities.CreateInstance(scope.ServiceProvider, applicationType);
                return new ApplicationContext(app, scope){Id = appId};
            }
            catch
            {
                // only dispose in case of an Exception, The ApplictaionCOntext will take care of the normal disposal
                serviceScope?.Dispose();
                throw;
            }
        }
    }
}