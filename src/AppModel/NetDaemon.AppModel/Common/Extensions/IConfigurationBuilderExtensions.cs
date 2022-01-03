using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using NetDaemon.AppModel.Internal.Config;

namespace NetDaemon.AppModel;

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddJsonAppConfig(this IConfigurationBuilder builder, string appPath)
    {
        Directory.EnumerateFiles(appPath, "*.json", SearchOption.AllDirectories)
            .ToList()
            .ForEach(x => builder.AddJsonFile(x, optional: false, reloadOnChange: false));
        return builder;
    }

    public static IConfigurationBuilder AddYamlAppConfig(this IConfigurationBuilder builder, string appPath)
    {
        Directory.EnumerateFiles(appPath, "*.y*", SearchOption.AllDirectories)
            .ToList()
            .ForEach(x => builder.AddYamlFile(x, optional: false, reloadOnChange: false));
        return builder;
    }

    internal static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder builder, string path, bool optional, bool reloadOnChange)
    {
        return AddYamlFile(builder, provider: null, path: path, optional: optional, reloadOnChange: reloadOnChange);
    }

    internal static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder builder, IFileProvider? provider, string path, bool optional, bool reloadOnChange)
    {
        if (provider == null && Path.IsPathRooted(path))
        {
            provider = new PhysicalFileProvider(Path.GetDirectoryName(path));
            path = Path.GetFileName(path);
        }
        var source = new YamlConfigurationSource
        {
            FileProvider = provider,
            Path = path,
            Optional = optional,
            ReloadOnChange = reloadOnChange
        };
        builder.Add(source);
        return builder;
    }
}
