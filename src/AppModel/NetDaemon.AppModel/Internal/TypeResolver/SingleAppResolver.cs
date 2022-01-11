namespace NetDaemon.AppModel.Internal.TypeResolver;

internal class SingleAppResolver : IAppTypeResolver
{
    private readonly Type _appType;

    public SingleAppResolver(Type appType)
    {
        _appType = appType;
    }

    public IReadOnlyCollection<Type> GetTypes()
    {
        return new[] { _appType };
    }
}