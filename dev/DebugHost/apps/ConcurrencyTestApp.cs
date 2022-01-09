using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetDaemon.AppModel;
using NetDaemon.HassModel.Common;

namespace Apps;

[NetDaemonApp]
public class ConcurrencyTestApp
{
    public ConcurrencyTestApp(IHaContext context, ILogger<ConcurrencyTestApp> logger)
    {

        context.StateChanges()
            .Where(n => n.Entity.EntityId == "input_select.who_cooks")
            .SubscribeAsync(async s =>
            {

                logger.LogInformation("{time}: Subcription 1 starts", DateTime.Now);
                await Task.Delay(1000);
                logger.LogInformation("{time}: Subcription 1 delay 1 s", DateTime.Now);
            });
        context.StateChanges()
            .Where(n => n.Entity.EntityId == "input_select.who_cooks")
            .SubscribeAsync(async s =>
            {
                logger.LogInformation("{time}: Subcription 2 starts", DateTime.Now);
                await Task.Delay(2000);
                // throw new InvalidOperationException("Ohhh nooo");
                logger.LogInformation("{time}: Subcription 2 delay 2 s", DateTime.Now);
            });

        context.StateChanges()
            .Where(n => n.Entity.EntityId == "input_select.who_cooks")
            .Subscribe(s =>
            {
                logger.LogInformation("{time}: Subcription 3 starts", DateTime.Now);
                Task.Delay(3000).Wait();
                // throw new InvalidOperationException("Ohhh nooo");
                logger.LogInformation("{time}: Subcription 3 delay 3 s", DateTime.Now);
            });

        context.StateChanges()
            .Where(n => n.Entity.EntityId == "input_select.who_cooks")
            .Subscribe(s =>
            {
                logger.LogInformation("{time}: Subcription 4 starts", DateTime.Now);
                Task.Delay(4000).Wait();
                // throw new InvalidOperationException("Ohhh nooo");
                logger.LogInformation("{time}: Subcription 4 delay 4 s", DateTime.Now);
            });
    }
}
[NetDaemonApp]
public class ConcurrencyTestApp2
{
    public ConcurrencyTestApp2(IHaContext context, ILogger<ConcurrencyTestApp2> logger)
    {

        context.StateChanges()
            .Where(n => n.Entity.EntityId == "input_select.who_cooks")
            .SubscribeAsyncConcurrent(async s =>
            {

                logger.LogInformation("{time}: Subcription 5 starts", DateTime.Now);
                await Task.Delay(1000);
                logger.LogInformation("{time}: Subcription 5 delay 1 s", DateTime.Now);
            });
        context.StateChanges()
            .Where(n => n.Entity.EntityId == "input_select.who_cooks")
            .Subscribe(s =>
            {
                logger.LogInformation("{time}: Subcription 6 starts", DateTime.Now);
                Task.Delay(2000).Wait();
                logger.LogInformation("{time}: Subcription 6 delay 2 s", DateTime.Now);
            });
    }
}