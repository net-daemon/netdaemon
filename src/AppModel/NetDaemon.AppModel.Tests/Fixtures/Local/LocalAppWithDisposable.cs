namespace LocalApps;

[NetDaemonApp]
public class MyAppLocalAppWithAsyncDispose : IAsyncDisposable, IDisposable
{
    public bool AsyncDisposeIsCalled { get; private set; }
    public bool DisposeIsCalled { get; private set; }

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

[NetDaemonApp]
public class MyAppLocalAppWithDispose : IDisposable
{
    public bool DisposeIsCalled { get; private set; }

    public void Dispose()
    {
        DisposeIsCalled = true;
        GC.SuppressFinalize(this);
    }
}
