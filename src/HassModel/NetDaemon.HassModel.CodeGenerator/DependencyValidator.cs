using System.Diagnostics.CodeAnalysis;
using System.Xml;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace NetDaemon.HassModel.CodeGenerator;

public static class VersionValidator
{
    /// <summary>
    /// Check if the PackageReferences for the NetDameon Nugets match the version of the nd-codegen tool
    /// </summary>
    [SuppressMessage("Design", "CA1031:Do not catch general exception types")]
    public static void ValidatePackageReferences()
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

    public static async Task ValidateLatestVersion()
    {
        try
        {
            var repo = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
            var packageSearchResource = await repo.GetResourceAsync<PackageMetadataResource>();
            var metaData = await packageSearchResource.GetMetadataAsync(
                packageId: "NetDaemon.HassModel.CodeGen",
                includePrerelease: false,
                includeUnlisted: false,
                sourceCacheContext: new SourceCacheContext { NoCache = true },
                log: NullLogger.Instance, token: CancellationToken.None);

            var maxVersion = metaData.OfType<PackageSearchMetadata>().Max(m => m.Version.Version);

            if (VersionHelper.GeneratorVersion == maxVersion)
            {
                Console.WriteLine("Cool, you are using the latest version of nd-codegen!");
                return;
            }

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($$"""
                                You are not using the latest version of nd-codegen ({{maxVersion.ToString(3)}}).
                                It is advised to keep the ND-codegen tool and the NetDaemon nuget packages up to date.
                                """);
            Console.ResetColor();

            Console.WriteLine("""
                              When using the local tool update it using:
                              dotnet tool update netdaemon.hassmodel.codegen

                              When using the global tool update it using:
                              dotnet tool update -g netdaemon.hassmodel.codegen

                              """);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unable to verify if you have the latest version of nd-codegen. '{ex.Message}'");
        }
    }

    public static IEnumerable<string> GetProjects() => Directory.GetFiles(".", "*.csproj", SearchOption.AllDirectories);

    /// <summary>
    /// Trims the pre-release information from a version string
    /// </summary>
    internal static string TrimPreReleaseVersion(string version)
    {
        var parts = version.Split('-');
        return parts[0];
    }

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
                version = TrimPreReleaseVersion(version);
                yield return (packageName, version);
            }
        }
    }
}
