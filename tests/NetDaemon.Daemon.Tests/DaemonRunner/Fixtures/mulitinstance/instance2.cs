using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetDaemon.Common.Reactive;
/// <summary>
///     Greets (or insults) people when coming home :)
/// </summary>
public class InstanceTwo : NetDaemonRxApp
{
    public override Task InitializeAsync()
    {
        // Do nothing

        return Task.CompletedTask;
    }
}