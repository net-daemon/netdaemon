using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Text.Json;
using BenchmarkDotNet.Attributes;
using NetDaemon.Client.Internal.HomeAssistant.Commands;
using NetDaemon.PerformanceBenchmarks.Support;

namespace NetDaemon.PerformanceBenchmarks.Benchmarks;

[MemoryDiagnoser]
public class ResultDispatchBenchmarks
{
    private HassMessage[] _results = [];

    [Params(1, 10, 100, 1000)]
    public int PendingCommands { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _results = Enumerable.Range(1, PendingCommands)
            .Select(id => JsonSerializer.Deserialize<HassMessage>(HassPayloadFactory.ResultMessage(id))!)
            .ToArray();
    }

    [Benchmark(Baseline = true)]
    public async Task PerCommandRxFilterSubscriptions()
    {
        using var subject = new Subject<HassMessage>();
        var tasks = Enumerable.Range(1, PendingCommands)
            .Select(id => subject.Where(n => n.Type == "result" && n.Id == id).FirstAsync().ToTask())
            .ToArray();

        foreach (var result in _results)
        {
            subject.OnNext(result);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    [Benchmark]
    public void DictionaryResultDispatch()
    {
        var pending = Enumerable.Range(1, PendingCommands)
            .ToDictionary(static id => id, static _ => new TaskCompletionSource<HassMessage>(TaskCreationOptions.RunContinuationsAsynchronously));

        foreach (var result in _results)
        {
            if (pending.Remove(result.Id, out var completionSource))
            {
                completionSource.SetResult(result);
            }
        }
    }
}
