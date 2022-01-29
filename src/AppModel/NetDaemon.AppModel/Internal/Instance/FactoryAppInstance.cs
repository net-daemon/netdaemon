namespace NetDaemon.AppModel.Internal.Resolver;

internal class FactoryAppInstance<TAppType> : IAppInstance where TAppType : class
{
    private readonly Func<IServiceProvider, TAppType> _factoryFunc;

    public FactoryAppInstance(Func<IServiceProvider, TAppType> factoryFunc)
    {
        _factoryFunc = factoryFunc;

        Id = AppInstanceHelper.GetAppId(typeof(TAppType));
        HasFocus = AppInstanceHelper.GetAppFocus(typeof(TAppType));
    }

    public object Create(IServiceProvider scopedServiceProvider)
    {
        return _factoryFunc.Invoke(scopedServiceProvider);
    }

    public string Id { get; }

    public bool HasFocus { get; }
}