
namespace LocalApps;


[NetDaemonApp]
public sealed class SlowDisposableApp : IDisposable
{
    public SlowDisposableApp()
    {
    }

    public void Dispose()
    {
        Thread.Sleep(3500);
    }
}

