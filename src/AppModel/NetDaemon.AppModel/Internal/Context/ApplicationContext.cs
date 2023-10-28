using NetDaemon.AppModel.Internal.AppFactories;

namespace NetDaemon.AppModel.Internal;

internal sealed class ApplicationContext
{
    private readonly CancellationTokenSource _cancelTokenSource = new();
    private readonly IServiceScope? _serviceScope;
    private bool _isDisposed;

    public ApplicationContext(IServiceProvider serviceProvider, IAppFactory appFactory)
    {
        // Create a new ServiceScope for all objects we create for this app
        // this makes sure they will all be disposed along with the app
        _serviceScope = serviceProvider.CreateScope();
        var scopedProvider = _serviceScope.ServiceProvider;

        // This ApplicationContext needs to be resolvable from this scoped provider
        // The class ApplicationScope which is registered as scoped makes this possible
        var appScope = scopedProvider.GetService<ApplicationScope>();
        if (appScope != null) appScope.ApplicationContext = this;

        // Now create the actual app from the new scope
        Instance = appFactory.Create(scopedProvider);
    }

    public object Instance { get; }

    public async Task InitializeAsync()
    {
        if (Instance is IAsyncInitializable initAsyncApp)
            await initAsyncApp.InitializeAsync(_cancelTokenSource.Token).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        // prevent multiple Disposes because the Service Scope will also dispose this
        if (_isDisposed) return;

        _isDisposed = true;

        if (_cancelTokenSource.IsCancellationRequested == false)
            await _cancelTokenSource.CancelAsync();

        switch (Instance)
        {
            case IAsyncDisposable asyncDisposable:
                await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                break;
            case IDisposable disposable:
                disposable.Dispose();
                break;
        }

        if (_serviceScope is IAsyncDisposable serviceScopeAsyncDisposable)
            await serviceScopeAsyncDisposable.DisposeAsync().ConfigureAwait(false);

        _cancelTokenSource.Dispose();
    }
}
