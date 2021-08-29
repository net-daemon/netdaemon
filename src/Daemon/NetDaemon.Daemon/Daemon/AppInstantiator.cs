using System;
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
            return new ApplicationContext(applicationType, appId, ServiceProvider);
        }
    }
}