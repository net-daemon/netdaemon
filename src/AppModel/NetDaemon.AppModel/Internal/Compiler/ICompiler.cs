namespace NetDaemon.AppModel.Internal.Compiler;

internal interface ICompiler : IDisposable
{
    CollectibleAssemblyLoadContext? Compile();
}
