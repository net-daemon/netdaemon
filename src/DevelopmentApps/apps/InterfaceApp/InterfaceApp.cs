using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;
using NetDaemon.Daemon;

namespace NetDaemon.DevelopmentApps.apps.DebugApp
{
    /// <summary> Test application for interface based app
    /// </summary>
    [Focus]
    [NetDaemonApp]
    public class InterfaceApp : IAsyncInitializable
    {
        private INetDaemonHost _host;
        private readonly ILogger _logger;

        public InterfaceApp(INetDaemonHost host, ILogger logger)
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
