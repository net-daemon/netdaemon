using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;
using NetDaemon.Daemon;

namespace NetDaemon.DevelopmentApps.apps.DebugApp
{
    /// <summary> Test application for interface based app
    /// </summary>
    [NetDaemonApp]
    public class InterfaceApp : IAsyncInitializable
    {
        private INetDaemon _host;

        public InterfaceApp(INetDaemonHost host)
        {
            _host = host;
            _host.CallService("notify", "persistent_notification", new { message = "Hello", title = "Yay it works via DI! via Constructor" }, true);;
        }

        public Task InitializeAsync()
        {
            _host.CallService("notify", "persistent_notification", new { message = "Hello", title = "Yay it works via DI!" }, true);;
            return Task.CompletedTask;
        }
    }
}
