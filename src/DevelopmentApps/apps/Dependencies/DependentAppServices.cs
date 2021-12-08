using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;

namespace NetDaemon.DevelopmentApps.apps.Dependencies
{
    [NetDaemonServicesProvider]
    public class DependentAppServices
    {
        private readonly ILogger<DependentAppServices> _logger;

        public DependentAppServices(ILogger<DependentAppServices> logger)
        {
            _logger = logger;
        }
        
        public void ConfigureServices(IServiceCollection services)
        {
            _logger.LogInformation("Configure service DependentAppServices");
            services.AddSingleton<TestServiceDi>();
        }
    }
}