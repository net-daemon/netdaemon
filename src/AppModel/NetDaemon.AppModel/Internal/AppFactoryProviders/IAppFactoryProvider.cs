using NetDaemon.AppModel.Internal.AppFactories;

namespace NetDaemon.AppModel.Internal.AppFactoryProviders;

internal interface IAppFactoryProvider
{
    IReadOnlyCollection<IAppFactory> GetAppFactories();
}