using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using NetDaemon.AppModel.Internal.Config;

namespace NetDaemon.AppModel;

/// <summary>
///     Extensions for IConfigurationBuilder
/// </summary>
public static class ConfigurationBuilderExtensions
{
    /// <summary>
    ///     Adds json configurations for apps given the path
    /// </summary>
    /// <param name="builder">Builder</param>
    /// <param name="appConfigPath">Path to the folder containing configurations</param>
    public static IConfigurationBuilder AddJsonAppConfig(this IConfigurationBuilder builder, string appConfigPath)
    {
        Directory.EnumerateFiles(appConfigPath, "*.json", SearchOption.AllDirectories)
            .ToList()
            .ForEach(x => builder.AddJsonFile(x, false, false));
        return builder;
    }

    /// <summary>
    ///     Adds yaml configurations for apps given the path
    /// </summary>
    /// <param name="builder">Builder</param>
    /// <param name="appConfigPath">Path to the folder containing configurations</param>
    public static IConfigurationBuilder AddYamlAppConfig(this IConfigurationBuilder builder, string appConfigPath)
    {
        Directory.EnumerateFiles(appConfigPath, "*.y*", SearchOption.AllDirectories)
            .ToList()
            .ForEach(x => builder.AddYamlFile(x, false, false));
        return builder;
    }

    internal static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder builder, string filePath, bool optional,
        bool reloadOnChange)
    {
        return AddYamlFile(builder, null, filePath, optional, reloadOnChange);
    }

    internal static IConfigurationBuilder AddYamlFile(this IConfigurationBuilder builder, IFileProvider? provider,
        string filePath, bool optional, bool reloadOnChange)
    {
        if (provider == null && Path.IsPathRooted(filePath))
        {
            provider = new PhysicalFileProvider(Path.GetDirectoryName(filePath)??"");
            filePath = Path.GetFileName(filePath);
        }

        var source = new YamlConfigurationSource
        {
            FileProvider = provider,
            Path = filePath,
            Optional = optional,
            ReloadOnChange = reloadOnChange
        };
        builder.Add(source);
        return builder;
    }
}
