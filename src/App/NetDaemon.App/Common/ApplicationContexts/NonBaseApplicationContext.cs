using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Daemon.Services;

namespace NetDaemon.Common
{
    sealed class NonBaseApplicationContext : ApplicationContext
    {
        private readonly ApplicationPersistenceService _persistenceService;

        public NonBaseApplicationContext(Type applicationType, string id, IServiceProvider serviceProvider, INetDaemon netDaemon) : base(applicationType, id, serviceProvider, netDaemon)
        {
            _persistenceService = new ApplicationPersistenceService(this, netDaemon);
        }

        public override async Task RestoreStateAsync()
        {
            await _persistenceService.RestoreAppStateAsync().ConfigureAwait(false);
        }

        public override void InstantiateApp()
        {
            ApplicationInstance = ActivatorUtilities.GetServiceOrCreateInstance(ServiceProvider, ApplicationType);
            ApplyConfig();
       }
    }
}