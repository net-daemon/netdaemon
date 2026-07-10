using System.Text.Json;
using BenchmarkDotNet.Attributes;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.PerformanceBenchmarks.Support;

namespace NetDaemon.PerformanceBenchmarks.Benchmarks;

[MemoryDiagnoser]
public class StateChangeJsonBenchmarks : IDisposable
{
    private readonly JsonDocument _document;
    private readonly JsonElement _eventData;

    public StateChangeJsonBenchmarks()
    {
        _document = JsonDocument.Parse(HassPayloadFactory.StateChangedEvent(1));
        _eventData = _document.RootElement.GetProperty("event").GetProperty("data");
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

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
            _document.Dispose();
    }
}
