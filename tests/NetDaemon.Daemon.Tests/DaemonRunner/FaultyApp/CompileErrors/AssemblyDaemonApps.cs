using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using NetDaemon.Common;
using NetDaemon.Common.Reactive;

/// <summary>
///     Greets (or insults) people when coming home :)
/// </summary>
[SuppressMessage("", "CS1002")]
[SuppressMessage("", "CS1038")]
[SuppressMessage("", "CA1050")]
public class AssmeblyFaultyCompileErrorDaemonApp : NetDaemonRxApp
{
    public override Task InitializeAsync()
    {
        // Do nothing

        return Task.CompletedTask;
    }

    [HomeAssistantServiceCall]
    public Task HandleServiceCall(dynamic data)
    {
        int x = 0 //compile error should be found in test
        return Task.CompletedTask;
    }
}