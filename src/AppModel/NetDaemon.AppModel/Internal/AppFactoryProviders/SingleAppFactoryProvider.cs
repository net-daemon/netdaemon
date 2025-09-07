using NetDaemon.AppModel.Internal.AppFactories;

namespace NetDaemon.AppModel.Internal.AppFactoryProviders;

internal sealed class SingleAppFactoryProvider : IAppFactoryProvider
{
    private readonly IAppFactory _factory;

    private SingleAppFactoryProvider(IAppFactory factory)
    {
        _factory = factory;
    }

    public IReadOnlyCollection<IAppFactory> GetAppFactories()
    {
        return new[] { _factory };
    }

    public static IAppFactoryProvider Create(Delegate handler,
        string? id = default, bool? focus = default)
    {
        var factory = FuncAppFactory.Create(handler, id, focus ?? false);
        return new SingleAppFactoryProvider(factory);
    }

    public static IAppFactoryProvider Create(Type type, string? id = default, bool? focus = default)
    {
        var factory = FuncAppFactory.Create(type, id, focus);
        return new SingleAppFactoryProvider(factory);
    }
}
