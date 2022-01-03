namespace NetDaemon.AppModel.Internal.TypeResolver;

internal class AppTypeResolver : IAppTypeResolver
{
    private readonly IEnumerable<IAssemblyResolver> _assemblyResolvers;

    public AppTypeResolver(IEnumerable<IAssemblyResolver> assemblyResolvers)
    {
        _assemblyResolvers = assemblyResolvers;
    }

    public IReadOnlyCollection<Type> GetTypes()
    {
        return _assemblyResolvers
            .Select(n => n.GetResolvedAssembly())
            .SelectMany(s => s.GetTypes()).ToList();
    }
}