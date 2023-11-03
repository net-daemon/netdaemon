using Microsoft.CodeAnalysis;

namespace NetDaemon.AppModel.Internal.Compiler;

/// <summary>
///     Gets the syntax tree from any source code (file, string)
/// </summary>
internal interface ISyntaxTreeResolver
{
    IReadOnlyCollection<SyntaxTree> GetSyntaxTrees();
}