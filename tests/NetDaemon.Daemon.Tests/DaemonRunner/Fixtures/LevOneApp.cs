using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;
/// <summary>
///     Greets (or insults) people when coming home :)
/// </summary>
public class LevOneApp : NetDaemonApp
{

    #region -- Config properties --

    #endregion

    public override Task InitializeAsync()
    {
        // Do nothing

        return Task.CompletedTask;
    }

}