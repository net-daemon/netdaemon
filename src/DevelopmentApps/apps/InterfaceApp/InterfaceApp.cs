using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;
using NetDaemon.Daemon;
using NetDaemon.Model3.Common;

namespace NetDaemon.DevelopmentApps.apps.DebugApp
{
    /// <summary> Test application for interface based app
    /// </summary>
    [NetDaemonApp]
    public class InterfaceApp : IAsyncInitializable
    {
        private readonly IHaContext _ha;

        public InterfaceApp(IHaContext ha)
        {
            _ha = ha;
        }

        public Task InitializeAsync()
        {
            _ha.CallService("notify", "persistent_notification", data: new { message = "Hello", title = "Yay it works via DI!" });;
            return Task.CompletedTask;
        }
    }
}
