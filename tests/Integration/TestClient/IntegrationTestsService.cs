using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Daemon;
using System.Diagnostics;

namespace TestClient
{
    /// <summary>
    ///     IntegrationTestService tests integration on a real instance of HA and NetDaemon
    /// </summary>
    public class IntegrationTestsService : BackgroundService
    {
        private static readonly CancellationTokenSource _globalCancellationSource = MainProgram.GlobalCancellationSource;
        private readonly IServiceProvider _serviceProvider;

        public IntegrationTestsService(
            IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var daemonHost  = _serviceProvider.GetService<NetDaemonHost>();
            if (daemonHost is null)
            {
                Debug.Fail("Faild to get daemonHost!");
                Environment.ExitCode = -1;
                _globalCancellationSource.Cancel();
               return;
            }
            while (!stoppingToken.IsCancellationRequested && !daemonHost.IsConnected) {
                await Task.Delay(2000, stoppingToken).ConfigureAwait(false);
            }
            System.Console.WriteLine("CONNECTED TO DAEMON");
            daemonHost.CallService("input_select", "select_option", new {entity_id = "input_select.who_cooks", option="Paulus"}); //.ConfigureAwait(false);
            await Task.Delay(2000, stoppingToken).ConfigureAwait(false);
            Environment.ExitCode = 0;
            _globalCancellationSource.Cancel();
        }
    }
}