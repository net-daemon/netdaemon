namespace NetDaemon.AppModel.Internal.AppFactories;

internal class TypeAppFactory : FuncAppFactory
{
    public TypeAppFactory(Type type, string? id = default, bool? focus = default) :
        base(
            CreateFactoryFunc(type),
            id ?? AppFactoryHelper.GetAppId(type),
            focus ?? AppFactoryHelper.GetAppFocus(type)
        )
    {
    }

    private static Func<IServiceProvider, object> CreateFactoryFunc(Type type)
    {
        return provider => ActivatorUtilities.CreateInstance(provider, type);
    }
}