using System.Diagnostics;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.HassModel;

#pragma warning disable CA1050

/// <summary>
///     Second app process all state changes and logs the performance
/// </summary>
[NetDaemonApp]
public class PerformanceTestSecondApp
{
    public PerformanceTestSecondApp(IHaContext ha, ILogger<PerformanceTestSecondApp> logger)
    {
        var counter = 0;
        var timer = new Stopwatch();
        ha.StateChanges()
            .Subscribe(x =>
            {
                if (counter == 0)
                {
                    timer.Start();
                }
                if (x.New?.State == "stop")
                {
                    timer.Stop();
                    logger.LogInformation("Performance test completed in {Time}ms, with {Counter} call with performance {MsgPerSec} msg/sec", timer.ElapsedMilliseconds, counter, counter / (timer.ElapsedMilliseconds / 1000));
                    timer.Reset();
                }
                counter++;
            });
        logger.LogInformation("Waiting for starting performance test");
    }
}
