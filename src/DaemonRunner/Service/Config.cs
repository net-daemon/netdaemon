using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace JoySoftware.HomeAssistant.NetDaemon.DaemonRunner
{
    public class Config
    {
        public Config()
        {
        }


        [JsonPropertyName("log_level")]
        public LogLevel? LoggingLevel { get; set; } = LogLevel.Information;
        [JsonPropertyName("token")]
        public string? Token { get; set; } = "enter hass token here";
        [JsonPropertyName("host")]
        public string? Host { get; set; } = "localhost";
        [JsonPropertyName("port")]
        public short? Port { get; set; } = 8123;
        [JsonPropertyName("ssl")]
        public bool? Ssl { get; set; } = false;
        [JsonPropertyName("ssl")]
        public string? SourceFolder { get; set; } = "";


    }
}
