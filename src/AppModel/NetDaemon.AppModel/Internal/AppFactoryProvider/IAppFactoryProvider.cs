using NetDaemon.AppModel.Internal.AppFactory;

namespace NetDaemon.AppModel.Internal.AppFactoryProvider;

internal interface IAppFactoryProvider
{
    IReadOnlyCollection<IAppFactory> GetAppFactories();
}