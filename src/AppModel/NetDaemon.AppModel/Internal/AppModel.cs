namespace NetDaemon.AppModel.Internal;

internal class AppModelImpl : IAppModel
{
    private readonly IServiceProvider _provider;

    public AppModelImpl(
        IServiceProvider provider
    )
    {
        _provider = provider;
    }

    internal IAppModelContext? CurrentContext { get; set; }

    public async ValueTask DisposeAsync()
    {
        if (CurrentContext is not null)
            await CurrentContext.DisposeAsync().ConfigureAwait(false);
    }

    public async Task<IAppModelContext> InitializeAsync(CancellationToken cancellationToken)
    {
        var appModelContext = _provider.GetRequiredService<IAppModelContext>();
        var initContext = (IAsyncInitializable)appModelContext;
        await initContext.InitializeAsync(CancellationToken.None);
        return appModelContext;
    }
}