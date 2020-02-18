using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;
/// <summary>
///     Greets (or insults) people when coming home :)
/// </summary>
public class TheApp : NetDaemonApp
{

    #region -- Config properties --
    public string? StringConfig { get; set; } = null;
    public int? IntConfig { get; set; } = null;
    public IEnumerable<string>? EnumerableConfig { get; set; } = null;

    #endregion

    public override Task InitializeAsync()
    {
        // Do nothing

        return Task.CompletedTask;
    }

}