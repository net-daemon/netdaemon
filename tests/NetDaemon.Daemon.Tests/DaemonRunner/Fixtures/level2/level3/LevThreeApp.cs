using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetDaemon.Common;
/// <summary>
///     Greets (or insults) people when coming home :)
/// </summary>
public class LevThreeApp : NetDaemonApp
{
    public override Task InitializeAsync()
    {
        // Do nothing

        return Task.CompletedTask;
    }
}