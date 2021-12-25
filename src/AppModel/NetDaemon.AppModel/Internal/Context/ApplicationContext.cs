namespace NetDaemon.AppModel.Internal;

internal class ApplicationContext :
    IApplicationContext,
    IApplicationInstance
{
    public string Id { get; }

    public bool IsEnabled { get; } = false;

    public Type AppType { get; }

    public object Instance { get; }

    private readonly IServiceScope? _serviceScope;
    public ApplicationContext(
        string id,
        Type appType,
        IServiceProvider serviceProvider
    )
    {
        // Create a new ServiceScope for all objects we create for this app
        // this makes sure they will all be disposed along with the app
        _serviceScope = serviceProvider.CreateScope();
        serviceProvider = _serviceScope.ServiceProvider;

        // This ApplicationContext needs to be resolvable from this scoped provider
        // The class ApplicationScope which is registered as scoped makes this possible
        var appScope = serviceProvider.GetService<ApplicationScope>();
        if (appScope != null)
        {
            appScope.ApplicationContext = this;
        }
        Id = id;
        AppType = appType;

        // Now create the actual app from the new scope
        Instance = ActivatorUtilities.CreateInstance(serviceProvider, appType);
    }

    public async ValueTask DisposeAsync()
    {
        if (Instance is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }

        if (Instance is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (_serviceScope is IAsyncDisposable serviceScopeAsyncDisposable)
        {
            await serviceScopeAsyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        GC.SuppressFinalize(this);
    }
}
