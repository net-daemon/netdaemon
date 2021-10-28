using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text.Json.Serialization;
using NetDaemon.Common;
using NetDaemon.HassModel.Common;

namespace NetDaemon.DevelopmentApps.apps
{
    [NetDaemonApp]
    public class ZhaApp
    {
        public ZhaApp(IHaContext ha)    
        {
            ha.Events.Filter<ZhaEventData>("zha_event")
                .Where(e => e.Data?.DeviceIeee == "00:15:8d:00:05:d9:d0:37")
                .Subscribe(e => Console.WriteLine(@$"device {e.Data?.DeviceIeee} sent 
command: {e.Data?.Command},
endpoint: {e.Data?.EndpointId}"));
            
            ha.Events.Filter<ZhaEventData>("zha_event")
                .Where(e => e.Data?.DeviceIeee == "00:15:8d:00:05:d9:d0:37")
                .Subscribe(e => Console.WriteLine(e.Data!.Args.TryGetValue("relative_degrees", out var deg) ? deg : null));
        }
        
        public record ZhaEventData
        {
            [JsonPropertyName("device_ieee")] public string? DeviceIeee { get; init; }
            [JsonPropertyName("unique_id")] public string? UniqueId { get; init; }
            [JsonPropertyName("endpoint_id")] public int? EndpointId { get; init; }
            [JsonPropertyName("cluster_id")] public int? ClusterId { get; init; }
            [JsonPropertyName("command")] public string? Command { get; init; }
            [JsonPropertyName("args")] public Dictionary<string, object> Args { get; init; } = new();
        }
    }
}