using System.Reflection;
using NetDaemon.AppModel.Internal.Compiler;

namespace NetDaemon.AppModel.Internal.AppAssemblyProviders;

internal class DynamicallyCompiledAppAssemblyProvider : IAppAssemblyProvider, IDisposable
{
    private readonly ICompiler _compiler;

    private Assembly? _compiledAssembly;
    private CollectibleAssemblyLoadContext? _currentContext;

    public DynamicallyCompiledAppAssemblyProvider(ICompiler compiler)
    {
        _compiler = compiler;
    }

    public Assembly GetAppAssembly()
    {
        // We reuse an already compiled assembly since we only compile once per start
        if (_compiledAssembly is not null)
            return _compiledAssembly;

        var (loadContext, compiledAssembly) = _compiler.Compile();
        _currentContext = loadContext;
        _compiledAssembly = compiledAssembly;
        return compiledAssembly;
    }

    public void Dispose()
    {
        if (_currentContext is null) return;
        _currentContext.Unload();
        // Finally do cleanup and release memory
        GC.Collect();
        GC.WaitForPendingFinalizers();
    }
}