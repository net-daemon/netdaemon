using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetDaemon.Common.Reactive;
using Microsoft.Extensions.Logging;

namespace Injection
{
    public class DependantApp : NetDaemonRxApp
    {
        public DependantApp(Action<string> logger)
        {
            logger("Hello logger");
        }

        public override Task InitializeAsync()
        {
            // Do nothing

            return Task.CompletedTask;
        }
    }
}