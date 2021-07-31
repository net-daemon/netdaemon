using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using NetDaemon.Common;
using NetDaemon.Daemon;

namespace NetDaemon.DevelopmentApps.apps.DebugApp
{
    /// <summary> Test application for interface based app
    /// </summary>
    [Focus]
    public class InterfaceApp : INetDaemonApp
    {
        public Task InitializeAsync() => Task.CompletedTask;
       
        public string? Id { get; set; }

        public string Description => "Sample bare app";

        public bool IsEnabled { get; set; }

        public AppRuntimeInfo RuntimeInfo { get; } = new();
    }
}
