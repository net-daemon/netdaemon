using System.Text.Json;
using BenchmarkDotNet.Attributes;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.Client.Internal.HomeAssistant.Commands;
using NetDaemon.PerformanceBenchmarks.Support;

namespace NetDaemon.PerformanceBenchmarks.Benchmarks;

[MemoryDiagnoser]
public class StateChangeJsonBenchmarks
{
    private readonly JsonElement _eventData;

    public StateChangeJsonBenchmarks()
    {
        var message = JsonSerializer.Deserialize<HassMessage>(HassPayloadFactory.StateChangedEvent(1))
            ?? throw new InvalidOperationException("Failed to create benchmark event");
        _eventData = message.Event!.DataElement!.Value;
    }

    [Benchmark(Baseline = true)]
    public string? ExtractEntityIdAndLazyNewState()
    {
        var entityId = _eventData.GetProperty("entity_id").GetString();
        _ = _eventData.GetProperty("new_state");
        return entityId;
    }

    [Benchmark]
    public HassState? ForceDeserializeNewState()
    {
        return _eventData.GetProperty("new_state").Deserialize<HassState>();
    }
}
