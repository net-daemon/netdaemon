using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetDaemon.Service.Api
{
    /// <summary>
    ///     Used to parse incoming messages
    /// </summary>
    public class WsMessage
    {
        [JsonPropertyName("type")] public string Type { get; set; } = "unknown";
        [JsonPropertyName("app")] public string? App { get; set; }
        [JsonPropertyName("data")] public JsonElement? ServiceData { get; set; } = null;
    }

    public class WsAppCommand
    {
        public bool? IsEnabled { get; set; }
    }

    public class WsExternalEvent
    {
        [JsonPropertyName("type")] public string Type { get; set; } = "unknown";
        [JsonPropertyName("data")] public object? Data { get; set; } = null;
    }
}