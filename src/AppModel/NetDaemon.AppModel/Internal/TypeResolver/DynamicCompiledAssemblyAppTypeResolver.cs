using NetDaemon.AppModel.Internal.Compiler;

namespace NetDaemon.AppModel.Internal.TypeResolver;

/// <summary>
///     Resolves types from dynamically compiled files given a path
/// </summary>
internal class DynamicCompiledAssemblyAppTypeResolver : IAppTypeResolver
{
    private readonly ICompilerFactory _compilerFactory;
    private CollectibleAssemblyLoadContext? _currentContext;

    public DynamicCompiledAssemblyAppTypeResolver(
        ICompilerFactory compilerFactory
    )
    {
        _compilerFactory = compilerFactory;
    }

    public IReadOnlyCollection<Type> GetTypes()
    {
        // Check if we have  
        if (_currentContext is not null)
        {
            _currentContext.Unload();
            // Finally do cleanup and release memory
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        var compiler = _compilerFactory.New();
        _currentContext = compiler.Compile();

        if (_currentContext is null)
            throw new InvalidOperationException("Failed to compile apps");

        return _currentContext.Assemblies.SelectMany(n => n.GetTypes()).ToList();
    }
}