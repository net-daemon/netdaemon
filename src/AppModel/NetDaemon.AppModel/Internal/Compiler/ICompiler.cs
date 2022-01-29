namespace NetDaemon.AppModel.Internal.Compiler;

internal interface ICompiler : IDisposable
{
    CompiledAssemblyResult Compile();
}
