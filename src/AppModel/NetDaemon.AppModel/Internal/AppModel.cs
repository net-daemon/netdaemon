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

    public IAppModelContext? CurrentAppModelContext { get; private set;  }

    public async Task<IAppModelContext> LoadNewApplicationContext(CancellationToken cancellationToken)
    {
        var appModelContext = ActivatorUtilities.CreateInstance<AppModelContext>(_provider);
        await appModelContext.InitializeAsync(cancellationToken);
        
        // Assign to CurrentAppModelContext so it can be resolved via DI  
        CurrentAppModelContext = appModelContext;
        return appModelContext;
    }
}