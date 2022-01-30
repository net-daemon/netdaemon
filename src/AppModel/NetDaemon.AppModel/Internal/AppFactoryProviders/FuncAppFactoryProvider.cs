using NetDaemon.AppModel.Internal.AppFactories;

namespace NetDaemon.AppModel.Internal.AppFactoryProviders;

internal class FuncAppFactoryProvider : IAppFactoryProvider
{
    public IReadOnlyCollection<IAppFactory> GetAppFactories()
    {
        throw new NotImplementedException();
    }
}