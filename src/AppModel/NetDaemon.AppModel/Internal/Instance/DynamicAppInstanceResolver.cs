namespace NetDaemon.AppModel.Internal.Resolver;

internal class DynamicAppInstanceResolver : IAppInstanceResolver
{
    private readonly IEnumerable<IAppTypeResolver> _resolvers;

    public DynamicAppInstanceResolver(IEnumerable<IAppTypeResolver> resolvers)
    {
        _resolvers = resolvers;
    }

    public IReadOnlyCollection<IAppInstance> GetApps()
    {
        return _resolvers
            .SelectMany(resolver => resolver.GetTypes())
            .Select(type => new DynamicAppInstance(type))
            .ToList();
    }
}