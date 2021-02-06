using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetDaemon.Common.Reactive;

namespace DependNs
{
    public class DependOnGlobalApp : NetDaemonRxApp
    {
        public override Task InitializeAsync()
        {
            // Do nothing

            return Task.CompletedTask;
        }
    }

    public class DependOnGlobalAndOtherApp : NetDaemonRxApp
    {
        public override Task InitializeAsync()
        {
            // Do nothing

            return Task.CompletedTask;
        }
    }

    public class DependOnGlobalOtherApp : NetDaemonRxApp
    {
        public override Task InitializeAsync()
        {
            // Do nothing

            return Task.CompletedTask;
        }
    }

    public class GlobalApp : NetDaemonRxApp
    {
        public override Task InitializeAsync()
        {
            // Do nothing

            return Task.CompletedTask;
        }
    }
}