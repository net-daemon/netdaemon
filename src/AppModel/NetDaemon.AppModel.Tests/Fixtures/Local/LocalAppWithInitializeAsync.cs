namespace LocalApps;

[NetDaemonApp]
public class MyAppLocalAppWithInitializeAsync : IInitializableAsync
{
    public bool InitializeAsyncCalled { get; private set; }


    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        InitializeAsyncCalled = true;
        return Task.CompletedTask;
    }
}