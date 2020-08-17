using System.Collections.Generic;
using System.Text.Json.Serialization;
using NetDaemon.Service.Api;

namespace NetDaemon.Daemon.Tests.DaemonRunner.Api
{
    public class WsAppsResult
    {
        [JsonPropertyName("type")] public string Type { get; set; } = "unknown";
        [JsonPropertyName("data")] public IEnumerable<ApiApplication>? Data { get; set; } = null;
    }

    public class WsConfigResult
    {
        [JsonPropertyName("type")] public string Type { get; set; } = "unknown";
        [JsonPropertyName("data")] public ApiConfig? Data { get; set; } = null;
    }
}
