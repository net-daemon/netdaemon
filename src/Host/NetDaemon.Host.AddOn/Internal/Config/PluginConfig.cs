using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetDaemon.Host.AddOn.Internal.Config;

internal record AddOnConfig
{
    [JsonPropertyName("log_level")] public string LogLevel { get; set; } = "information";

    [JsonPropertyName("app_config_path")] public string ApplicationConfigFolderPath { get; set; } = string.Empty;
}

internal static class ConfigManager
{
    private const string AddOnConfigPath = "/data/options.json";

    public static AddOnConfig Get()
    {
        if (!Debugger.IsAttached)
            return JsonSerializer.Deserialize<AddOnConfig>(File.ReadAllBytes(AddOnConfigPath))
                   ?? throw new InvalidOperationException("Failed to read addon config");
        
        // If we are in a debugging session we are obviously not in an add-on
        // in this case we fake the data so we will be able to debug addon host
        return new AddOnConfig {LogLevel = "trace", ApplicationConfigFolderPath = "./"};
    }
}