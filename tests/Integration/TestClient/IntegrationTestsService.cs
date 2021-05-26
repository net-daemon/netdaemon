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
            Environment.ExitCode = -1;

            var daemonHost  = _serviceProvider.GetService<NetDaemonHost>();
            if (daemonHost is null)
            {
                Debug.Fail("Faild to get daemonHost!");
                _globalCancellationSource.Cancel();
               return;
            }
            while (!stoppingToken.IsCancellationRequested && !daemonHost.IsConnected) {
                await Task.Delay(2000, stoppingToken).ConfigureAwait(false);
            }
            // Test case #1 call service and check correct state 

            daemonHost.CallService("input_select", "select_option", new {entity_id = "input_select.who_cooks", option="Paulus"}); //.ConfigureAwait(false);
            await Task.Delay(2000, stoppingToken).ConfigureAwait(false);

            if (FailEq<string>(daemonHost.GetState("input_select.who_cooks")?.State, "Paulus"))
                return;
            // End test case #1

            Environment.ExitCode = 0;
            _globalCancellationSource.Cancel();
        }

        private static bool FailEq<T>(T actual, T expected)
        {
            var IsEqual = actual?.Equals(expected) ?? false;
            if (!IsEqual)
            {
                Console.WriteLine($"EXPECTED: {expected}, GOT: {actual}");
            }
            return Fail(IsEqual);
        }
        private static bool Fail(bool check)
        {
            if (!check)
            {
                Environment.ExitCode = -1;
                _globalCancellationSource.Cancel();
            }
            return !check;
        }
    }
}