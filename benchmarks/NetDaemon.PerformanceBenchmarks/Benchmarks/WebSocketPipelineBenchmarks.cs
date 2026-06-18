using BenchmarkDotNet.Attributes;
using NetDaemon.Client.Internal.HomeAssistant.Commands;
using NetDaemon.Client.Internal.Net;
using NetDaemon.PerformanceBenchmarks.Support;

namespace NetDaemon.PerformanceBenchmarks.Benchmarks;

[MemoryDiagnoser]
public class WebSocketPipelineBenchmarks
{
    private const int CoalescedEventCount = 64;
    private readonly string _singleEvent = HassPayloadFactory.StateChangedEvent(1);
    private readonly string _coalescedEvents = HassPayloadFactory.CoalescedStateChangedEvents(CoalescedEventCount);
    private readonly object _serviceCommand = new { id = 42, type = "call_service", domain = "light", service = "turn_on" };

    [Benchmark(Baseline = true)]
    public async Task<HassMessage[]> ReadSingleEvent()
    {
        var websocket = new BenchmarkWebSocketClient();
        websocket.EnqueueJson(_singleEvent);
        var pipeline = new WebSocketClientTransportPipeline(websocket);

        return await pipeline.GetNextMessagesAsync<HassMessage>(CancellationToken.None).ConfigureAwait(false);
    }

    [Benchmark]
    public async Task<HassMessage[]> ReadCoalescedEvents()
    {
        var websocket = new BenchmarkWebSocketClient();
        websocket.EnqueueJson(_coalescedEvents);
        var pipeline = new WebSocketClientTransportPipeline(websocket);

        return await pipeline.GetNextMessagesAsync<HassMessage>(CancellationToken.None).ConfigureAwait(false);
    }

    [Benchmark]
    public async Task SendServiceCommand()
    {
        var websocket = new BenchmarkWebSocketClient();
        var pipeline = new WebSocketClientTransportPipeline(websocket);

        await pipeline.SendMessageAsync(_serviceCommand, CancellationToken.None).ConfigureAwait(false);
    }
}
