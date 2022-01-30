using NetDaemon.AppModel.Internal.AppFactory;

namespace NetDaemon.AppModel.Internal.AppFactoryProvider;

internal class SingleAppFactoryProvider : IAppFactoryProvider
{
    private readonly Type _appType;

    public SingleAppFactoryProvider(Type appType)
    {
        _appType = appType;
    }

    public IReadOnlyCollection<IAppFactory> GetAppFactories()
    {
        return new[] { new TypeAppFactory(_appType) };
    }
}