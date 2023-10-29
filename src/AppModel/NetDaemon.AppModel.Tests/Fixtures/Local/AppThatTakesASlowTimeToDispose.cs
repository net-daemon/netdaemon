
namespace LocalApps;


[NetDaemonApp]
public class SlowDisposableApp : IDisposable
{
    public SlowDisposableApp()
    {
    }

    public void Dispose()
    {
        Thread.Sleep(3500);
    }
}

