using System.Reactive.Subjects;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Logging.Abstractions;
using NetDaemon.HassModel.Internal;

namespace NetDaemon.PerformanceBenchmarks.Benchmarks;

[MemoryDiagnoser]
public class QueuedObservableBenchmarks
{
    private const int EventCount = 10_000;

    [Params(1, 10, 50)]
    public int AppScopes { get; set; }

    [Benchmark]
    public async Task FanOutToAppScopedQueues()
    {
        using var source = new Subject<int>();
        var queues = new QueuedObservable<int>[AppScopes];

        for (var i = 0; i < queues.Length; i++)
        {
            queues[i] = new QueuedObservable<int>(source, NullLogger.Instance);
            queues[i].Subscribe(static _ => { });
        }

        for (var i = 0; i < EventCount; i++)
        {
            source.OnNext(i);
        }

        foreach (var queue in queues)
        {
            await queue.DisposeAsync().ConfigureAwait(false);
        }
    }
}
