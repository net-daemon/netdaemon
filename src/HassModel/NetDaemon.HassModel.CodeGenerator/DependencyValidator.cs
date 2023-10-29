using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Xml;

namespace NetDaemon.HassModel.CodeGenerator;

public static class DependencyValidator
{
    /// <summary>
    /// Check if the PackageReferences for the NetDameon Nugets match the version of the nd-codegen tool
    /// </summary>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
    public static void ValidatePackageRefrences()
    {
        try
        {
            var packages = GetProjects().SelectMany(project => GetPackageReferences(project).Select(package => (project, package)));
            var mismatches = packages.Where(p => p.package.name.StartsWith("NetDaemon.", StringComparison.InvariantCulture) && p.package.version != VersionHelper.GeneratorVersion.ToString(3)).ToList();
            if (mismatches.Any())
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Warning: NetDaemon PackageReferences were found that do not match the version of nd-codegen ({VersionHelper.GeneratorVersion}) " + Environment.NewLine +
                                  "It is advised to keep the ND-codegen tool in sync with the installed versions of the nuget packages");
                Console.ResetColor();

                foreach (var (project, (packagename, packgeversion)) in mismatches)
                {
                    Console.WriteLine($"    Project: {project}, Package: {packagename}:{packgeversion}");
                }
            }
        }
        catch (Exception ex)
        {
            // ignored, we do not want to fail in case something goes wrong in eg. parsing the project files
            Console.WriteLine("Unable to validate PackageReferences");
            Console.WriteLine(ex);
        }
    }

    public static IEnumerable<string> GetProjects() => Directory.GetFiles(".", "*.csproj", SearchOption.AllDirectories);

    private static IEnumerable<(string name, string version)>GetPackageReferences(string projectFilePath)
    {
        var csproj = new XmlDocument();
        csproj.Load(projectFilePath);
        var nodes = csproj.SelectNodes("//PackageReference[@Include and @Version]");
        if (nodes is null) yield break;

        foreach (XmlNode packageReference in nodes)
        {
            var packageName = packageReference.Attributes?["Include"]?.Value;
            var version = packageReference.Attributes?["Version"]?.Value;
            if (packageName is not null && version is not null)
            {
                yield return (packageName, version);
            }
        }
    }
}
