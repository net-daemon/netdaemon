using Microsoft.Extensions.Logging;
using NetDaemon.Common;
using NetDaemon.HassModel.Common;

namespace NetDaemon.DevelopmentApps.apps.Dependencies
{
    [NetDaemonApp]
    [Focus]
    public class DependentApp : IInitializable
    {
        private readonly ILogger<DependentApp> _logger;
        private readonly TestServiceDi _testServiceDi;

        public DependentApp(IHaContext ha, ILogger<DependentApp> logger, TestServiceDi testServiceDi, DependentChildApp childApp)
        {
            _logger = logger;
            _testServiceDi = testServiceDi;
        }
        
        public void Initialize()
        {
            _logger.LogInformation("Initialize DependentApp " + _testServiceDi.Test);
        }
    }
}