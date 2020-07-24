using System.Threading.Tasks;
using NetDaemon.Common;
using NetDaemon.Common.Reactive;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Reactive.Linq;
using System.Runtime.Serialization;
// using Netdaemon.Generated.Extensions;

/// <summary>
///     Does some awesome stuff
/// </summary>
public class GlobalApp : NetDaemonApp
{
    // private ISchedulerResult _schedulerResult;
    private int numberOfRuns = 0;
    IDisposable task;
    //public string? SharedThing { get; set; }
    public override Task InitializeAsync()
    {

        // Event("TEST_EVENT").Call(async (ev, data) => { Log("EVENT!"); }).Execute();

        return Task.CompletedTask;
    }
}


