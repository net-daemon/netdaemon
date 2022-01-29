namespace NetDaemon.AppModel.Internal.Resolver;

internal class FactoryAppInstanceResolver<TAppType> : IAppInstanceResolver where TAppType : class
{
    private readonly Func<IServiceProvider, TAppType> _factoryFunc;

    public FactoryAppInstanceResolver(Func<IServiceProvider, TAppType> factoryFunc)
    {
        _factoryFunc = factoryFunc;
    }

    public IReadOnlyCollection<IAppInstance> GetApps()
    {
        return new[] { new FactoryAppInstance<TAppType>(_factoryFunc) };
    }
}