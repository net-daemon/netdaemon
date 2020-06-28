using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetDaemon.Common;

/// <summary>
///     Greets (or insults) people when coming home :)
/// </summary>
public class SupressLogs : NetDaemonApp
{
    [DisableLog(SupressLogType.MissingExecute)]
    public override async Task InitializeAsync()
    {
        // All  warnings in this is supressed
        Entity("Test");
        await DoStuff();
    }

    public async Task DoStuff()
    {
        // This is not supressed
        Entity("Test");
    }
}