using System.Collections.Generic;
using System.Threading.Tasks;
using NetDaemon.Common.Reactive;

/// <summary>
///     Greets (or insults) people when coming home :)
/// </summary>
public class CircularApp : NetDaemonRxApp
{
    public override Task InitializeAsync()
    {
        // Do nothing

        return Task.CompletedTask;
    }
}