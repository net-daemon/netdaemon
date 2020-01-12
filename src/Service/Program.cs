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
            await daemonHost.Run("192.168.1.7", 8123, false, "", CancellationToken.None);

            await daemonHost.Action.Toggle.Entity("light.tomas_rum").ExecuteAsync();
            // await daemonHost.Action.TurnOn.Entity("light.tomas_rum").ExecuteAsync();

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
