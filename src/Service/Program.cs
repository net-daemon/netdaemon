using System;
using System.Threading;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace runner
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var daemonHost = new NetDaemonHost(new HassClient());
            var runTask =  daemonHost.Run("192.168.1.7", 8123, false, "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpc3MiOiJiOTMzNmFhZDdkNjY0ZDhhYjE1YTdiYmZlOTNiZWE4MCIsImlhdCI6MTU3NzA0Njg1OCwiZXhwIjoxODkyNDA2ODU4fQ.bMH-Vy8dLQLtjR6ixWHcmQiWf4eoIPdKVOZfmnwH_Bc", CancellationToken.None);

            daemonHost.Entity("binary_sensor.tomas_rum_pir")
                .StateChanged("on")
                    .Entity("light.tomas_rum")
                        .TurnOn()
                            .UsingAttribute("transition", 0)
                .Execute();

            daemonHost.Entity("binary_sensor.tomas_rum_pir")
                .StateChanged("off")
                    .Entity("light.tomas_rum")
                        .TurnOff()
                            .UsingAttribute("transition", 0)
                .Execute();

            await Task.WhenAny(runTask, Task.Delay(120000));
            await daemonHost.Stop();

            //CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices(services => { services.AddHostedService<RunnerService>(); })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();
                });
    }
}
