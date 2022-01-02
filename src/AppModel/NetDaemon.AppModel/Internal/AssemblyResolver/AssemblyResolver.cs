using System.Reflection;
using NetDaemon.AppModel.Internal.Compiler;

namespace NetDaemon.AppModel.Internal;

internal class AssemblyResolver : IAssemblyResolver
{
    private readonly Assembly _assembly;

    public AssemblyResolver(
        Assembly assembly
    )
    {
        _assembly = assembly;
    }

    public Assembly GetResolvedAssembly() => _assembly;
}

internal class DynamicallyCompiledAssemblyResolver : IAssemblyResolver
{
    private readonly ICompilerFactory _compilerFactory;
    private CollectibleAssemblyLoadContext? _currentContext;

    public DynamicallyCompiledAssemblyResolver(
        ICompilerFactory compilerFactory
    )
    {
        _compilerFactory = compilerFactory;
    }

    public Assembly GetResolvedAssembly()
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
        var (loadContext, compiledAssembly) = compiler.Compile();
        _currentContext = loadContext;
        return compiledAssembly;
    }
}

