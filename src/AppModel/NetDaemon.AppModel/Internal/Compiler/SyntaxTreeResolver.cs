using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace NetDaemon.AppModel.Internal.Compiler;

internal class SyntaxTreeResolver : ISyntaxTreeResolver
{
    private readonly AppConfigurationLocationSetting _settings;

    public SyntaxTreeResolver(
        IOptions<AppConfigurationLocationSetting> settings
    )
    {
        _settings = settings.Value;
    }

    public IReadOnlyCollection<SyntaxTree> GetSyntaxTrees()
    {
        var fullPath = Path.GetFullPath(_settings.ApplicationConfigurationFolder);
        // Get the paths for all .cs files recursively in app folder
        var csFiles = Directory.EnumerateFiles(
            fullPath,
            "*.cs",
            SearchOption.AllDirectories).ToArray();

        var result = new List<SyntaxTree>(csFiles.Length);

        result.AddRange(from csFile in csFiles
                        let fs = new FileStream(csFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                        let sourceText = SourceText.From(fs, Encoding.UTF8, canBeEmbedded: true)
                        let syntaxTree = SyntaxFactory.ParseSyntaxTree(sourceText, path: csFile)
                        select syntaxTree);
        return result;
    }
}
