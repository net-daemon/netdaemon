using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.NetDaemon.Common;
/// <summary>
///     Greets (or insults) people when coming home :)
/// </summary>
public class AppComplexConfig : NetDaemonApp
{
    public string? AString { get; set; }
    public int? AnInt { get; set; }
    public bool? ABool { get; set; }
    public IEnumerable<string>? AStringList { get; set; }
    public IEnumerable<Device>? Devices { get; set; }
    public override Task InitializeAsync()
    {
        // Do nothing

        return Task.CompletedTask;
    }
}

public class Device
{
    public string? name { get; set; }
    public IEnumerable<Command>? commands { get; set; }
}
public class Command
{
    public string? name { get; set; }
    public string? data { get; set; }
}