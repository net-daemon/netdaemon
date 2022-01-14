namespace LocalApps;

[NetDaemonApp]
public class MyAppLocalAppWithInitialize : IAsyncInitializable
{
    public bool InitializeAsyncCalled { get; private set; }


    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        InitializeAsyncCalled = true;
        return Task.CompletedTask;
    }
}