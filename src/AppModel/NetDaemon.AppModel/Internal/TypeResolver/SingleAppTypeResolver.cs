namespace NetDaemon.AppModel.Internal.TypeResolver;

internal class SingleAppTypeResolver : IAppTypeResolver
{
    private readonly Type _appType;

    public SingleAppTypeResolver(Type appType)
    {
        _appType = appType;
    }

    public IReadOnlyCollection<Type> GetTypes()
    {
        return new[] { _appType };
    }
}