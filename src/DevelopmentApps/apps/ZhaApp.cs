using System;
using System.Reactive.Linq;
using Model3.ModelV3;
using Model3.ModelV3.EventTypes;
using NetDaemon.Common;

namespace NetDaemon.DevelopmentApps.apps
{
    [NetDaemonApp]
    public class ZhaApp
    {
        public ZhaApp(IEventProvider eventProvider)
        {
            eventProvider.GetEventDataOfType<ZhaEventData>("zha_event")
                .Where(e => e.device_ieee == "84:71:27:ff:fe:40:78:7b")
                .Subscribe(e => Console.WriteLine($"device {e.device_ieee} sent command: { e.command} endpoint: {e.endpoint_id}"));
            
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
    }
}