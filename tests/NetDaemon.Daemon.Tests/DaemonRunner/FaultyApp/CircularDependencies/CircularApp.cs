using JoySoftware.HomeAssistant.NetDaemon.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
///     Greets (or insults) people when coming home :)
/// </summary>
public class CircularApp : NetDaemonApp
{

    public override Task InitializeAsync()
    {
        // Do nothing

        return Task.CompletedTask;
    }

}