using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;
/// <summary>
///     Greets (or insults) people when coming home :)
/// </summary>
public class AssmeblyDaemonApp : NetDaemonApp
{

    #region -- Test config --

    public string? StringConfig { get; set; } = null;
    public int? IntConfig { get; set; } = null;
    public IEnumerable<string>? EnumerableConfig { get; set; } = null;

    #endregion

    #region -- Test secrets --

    public string? TestSecretString { get; set; }
    public int? TestSecretInt { get; set; }

    public string? TestNormalString { get; set; }
    public int? TestNormalInt { get; set; }

    #endregion

    public override Task InitializeAsync()
    {
        // Do nothing

        return Task.CompletedTask;
    }

}