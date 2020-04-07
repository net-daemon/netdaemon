using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;

namespace DependNs
{
    public class DependOnGlobalApp : NetDaemonApp
    {
        public override Task InitializeAsync()
        {
            // Do nothing

            return Task.CompletedTask;
        }
    }

    public class DependOnGlobalAndOtherApp : NetDaemonApp
    {
        public override Task InitializeAsync()
        {
            // Do nothing

            return Task.CompletedTask;
        }
    }

    public class DependOnGlobalOtherApp : NetDaemonApp
    {
        public override Task InitializeAsync()
        {
            // Do nothing

            return Task.CompletedTask;
        }


    }

    public class GlobalApp : NetDaemonApp
    {
        public override Task InitializeAsync()
        {
            // Do nothing

            return Task.CompletedTask;
        }
    }
}