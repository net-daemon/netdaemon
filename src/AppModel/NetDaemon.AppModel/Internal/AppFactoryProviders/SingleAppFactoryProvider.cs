using NetDaemon.AppModel.Internal.AppFactories;

namespace NetDaemon.AppModel.Internal.AppFactoryProviders;

internal sealed class SingleAppFactoryProvider : IAppFactoryProvider
{
    private readonly IAppFactory _factory;

    private SingleAppFactoryProvider(IAppFactory factory)
    {
        _factory = factory;
    }

    public static IAppFactoryProvider Create(Delegate handler, string id, bool focus = false)
    {
        return new SingleAppFactoryProvider(new FuncAppFactory(handler, id, focus));
    }

    public static IAppFactoryProvider Create(Type type, string? id = default, bool? focus = default)
    {
        return new SingleAppFactoryProvider(new ClassAppFactory(type, id, focus));
    }

    public IReadOnlyCollection<IAppFactory> GetAppFactories() => [_factory];
}
