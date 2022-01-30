namespace NetDaemon.AppModel.Internal.AppFactory;

internal class FuncAppFactory<TAppType> : FuncAppFactory where TAppType : class
{
    public FuncAppFactory(Func<IServiceProvider, TAppType> factoryFunc, string? id = default, bool? focus = default) :
        base(
            factoryFunc,
            id ?? AppFactoryHelper.GetAppId(typeof(TAppType)),
            focus ?? AppFactoryHelper.GetAppFocus(typeof(TAppType))
        )
    {
    }
}

internal class FuncAppFactory : IAppFactory
{
    private readonly Func<IServiceProvider, object> _factoryFunc;

    public FuncAppFactory(Func<IServiceProvider, object> factoryFunc, string id, bool focus)
    {
        _factoryFunc = factoryFunc;

        Id = id;
        HasFocus = focus;
    }

    public object Create(IServiceProvider provider) => _factoryFunc.Invoke(provider);

    public string Id { get; }

    public bool HasFocus { get; }
}