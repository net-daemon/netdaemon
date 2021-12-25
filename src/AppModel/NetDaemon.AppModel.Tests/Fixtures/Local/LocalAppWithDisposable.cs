using NetDaemon.AppModel.Common;

namespace LocalApps;


[NetDaemonApp]
public class MyAppLocalAppWithDispose : IAsyncDisposable, IDisposable
{
    public bool AsyncDisposeIsCalled { get; set; }
    public bool DisposeIsCalled { get; set; }
    public MyAppLocalAppWithDispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        AsyncDisposeIsCalled = true;
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
        DisposeIsCalled = true;
        GC.SuppressFinalize(this);
    }
}