namespace NetDaemon.AppModel.Internal;

/// <summary>
/// This class serves as a factory for creating and initializing new ApplicationContexts
/// </summary>
internal class AppModelImpl : IAppModel
{
    private readonly IServiceProvider _provider;

    public AppModelImpl(IServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task<IAppModelContext> LoadNewApplicationContext(CancellationToken cancellationToken)
    {
        // Create a new AppModelContext
        var appModelContext = _provider.GetRequiredService<IAppModelContext>();
        await appModelContext.InitializeAsync(cancellationToken);
        return appModelContext;
    }
}