using System.Reflection;

namespace NetDaemon.AppModel.Internal.Compiler;

internal interface ICompilerFactory
{
    ICompiler New();
}
