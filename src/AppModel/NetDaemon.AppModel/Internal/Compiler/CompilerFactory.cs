namespace NetDaemon.AppModel.Internal.Compiler;

internal class CompilerFactory : ICompilerFactory
{
    private readonly ILogger<Compiler> _logger;
    private readonly ISyntaxTreeResolver _syntaxResolver;

    public CompilerFactory(
        ISyntaxTreeResolver syntaxResolver,
        ILogger<Compiler> logger
    )
    {
        _syntaxResolver = syntaxResolver;
        _logger = logger;
    }

    public ICompiler New()
    {
        return new Compiler(_syntaxResolver, _logger);
    }
}