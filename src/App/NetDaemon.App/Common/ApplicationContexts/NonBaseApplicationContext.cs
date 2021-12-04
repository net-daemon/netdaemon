using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Daemon.Services;

namespace NetDaemon.Common
{
    internal sealed class NonBaseApplicationContext : ApplicationContext
    {
        private readonly IPersistenceService _persistenceService;

        public NonBaseApplicationContext(Type applicationType, string id, IServiceProvider serviceProvider) : base(applicationType, id, serviceProvider)
        {
            _persistenceService = ServiceProvider.GetRequiredService<IPersistenceService>();
        }
        
        public override async Task RestoreStateAsync()
        {
            await _persistenceService.RestoreAppStateAsync().ConfigureAwait(false);
        }

        public override void Start()
        {
            try
            {
                var appInstance = ActivatorUtilities.CreateInstance(ServiceProvider, ApplicationType);
                ApplicationInstance = appInstance;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException($"Faild to create instance {ApplicationType.Name} for of app id {Id}", e);
            }

            ApplyConfig();
       }
    }
}