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
        public ZhaApp(IEventProvider eventProvider)
        {
            eventProvider.GetEventDataOfType<ZhaEventData>("zha_event")
                .Where(e => e.DeviceIeee == "84:71:27:ff:fe:40:78:7b")
                .Subscribe(e => Console.WriteLine($"device {e.DeviceIeee} sent command: { e.Command} endpoint: {e.EndpointId}"));
            
            // Prints: `device 84:71:27:ff:fe:40:78:7b sent command: off endpoint: 1`
            
            // The IObservable<ZhaEventData> we get here provides just the Data property of the event
            // while te full event looks like this, so we miss some of the metadata, but I thinkink the most common case
            // that is not needed and it does simplify the usage of this API
            /*

                "event_type": "zha_event",
                "data": {
                    "device_ieee": "84:71:27:ff:fe:40:78:7b",
                    "unique_id": "84:71:27:ff:fe:40:78:7b:2:0x0006",
                    "device_id": "56aa6e3e2dbc6fbd6093c4ce85744e77",
                    "endpoint_id": 2,
                    "cluster_id": 6,
                    "command": "off",
                    "args": []
                },
                "origin": "LOCAL",
                "time_fired": "2021-08-25T13:44:13.756329+00:00",
                "context": {
                    "id": "1642fd5a6c175b90a8d9841a095e0dc1",
                    "parent_id": null,
                    "user_id": null
                }
            }
             */
        }
        
        public record ZhaEventData
        {
            [JsonPropertyName("device_ieee")] public string? DeviceIeee { get; set; }
            [JsonPropertyName("unique_id")] public string? UniqueId { get; set; }
            [JsonPropertyName("endpoint_id")] public int? EndpointId { get; set; }
            [JsonPropertyName("endpoint_id")] public int? ClusterId { get; set; }
            [JsonPropertyName("command")] public string? Command { get; set; }
            [JsonPropertyName("args")] public IReadOnlyCollection<object>? Args { get; set; }
        }
    }
}