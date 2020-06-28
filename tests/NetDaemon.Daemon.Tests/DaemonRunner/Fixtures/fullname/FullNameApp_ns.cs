using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetDaemon.Common;

namespace TheAppNameSpace2
{
    /// <summary>
    ///     Greets (or insults) people when coming home :)
    /// </summary>
    public class FullNameApp : NetDaemonApp
    {
        public override Task InitializeAsync()
        {
            // Do nothing

            return Task.CompletedTask;
        }
    }
}