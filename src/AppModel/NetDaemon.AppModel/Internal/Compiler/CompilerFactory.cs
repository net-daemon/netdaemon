namespace NetDaemon.AppModel.Internal.Compiler;

internal class CompilerFactory : ICompilerFactory
{
    private readonly ISyntaxTreeResolver _syntaxResolver;
    private readonly ILogger<Compiler> _logger;

    public CompilerFactory(
        ISyntaxTreeResolver syntaxResolver,
        ILogger<Compiler> logger
    )
    {
        _syntaxResolver = syntaxResolver;
        _logger = logger;
    }

    public ICompiler New() => new Compiler(_syntaxResolver, _logger);
}