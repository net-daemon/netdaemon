using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;
using NetDaemon.HassModel.Common;

namespace NetDaemon.DevelopmentApps.apps.Dependencies
{
    [NetDaemonApp]
    [Focus]
    public class DependentChildApp : IInitializable
    {
        private readonly ILogger<DependentApp> _logger;
        private readonly TestServiceDi _testServiceDi;

        public DependentChildApp(IHaContext ha, ILogger<DependentApp> logger, TestServiceDi testServiceDi)
        {
            _logger = logger;
            _testServiceDi = testServiceDi;
        }

        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<TestServiceDi>();
        }
        
        public void Initialize()
        {
            _logger.LogInformation("Initialize DependentChildApp");
            _testServiceDi.Test = "Child";
        }
    }

    public class TestServiceDi
    {
        public string Test { get; set; }
    }
}