using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;

namespace NetDaemon.DevelopmentApps.apps.DebugApp
{
    /// <summary> Test application for interface based app
    /// </summary>
    [NetDaemonApp]
    public class InterfaceApp : IAsyncInitializable
    {
        private INetDaemon _host;
        private readonly ILogger _logger;

        public InterfaceApp(INetDaemon host, ILogger logger)
        {
            _host = host;
            _logger = logger;
            _host.CallService("notify", "persistent_notification", new { message = "Hello", title = "Yay it works via DI! via Constructor" }, true);;
        }

        public Task InitializeAsync()
        {
            _host.CallService("notify", "persistent_notification", new { message = "Hello", title = "Yay it works via DI!" }, true);;
            _logger.LogInformation("Logging via injected logger");
            return Task.CompletedTask;
        }
    }
}
