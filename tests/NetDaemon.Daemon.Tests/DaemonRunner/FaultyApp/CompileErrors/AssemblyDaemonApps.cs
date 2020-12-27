using System.Collections.Generic;
using System.Threading.Tasks;
using NetDaemon.Common;

/// <summary>
///     Greets (or insults) people when coming home :)
/// </summary>
[SuppressMessage("", "CS1002")]
[SuppressMessage("", "CS1038")]
public class AssmeblyFaultyCompileErrorDaemonApp : NetDaemonApp
{
    #region -- Test config --

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