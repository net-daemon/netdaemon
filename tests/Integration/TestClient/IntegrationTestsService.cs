using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Daemon;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace TestClient
{
    /// <summary>
    ///     IntegrationTestService tests integration on a real instance of HA and NetDaemon
    /// </summary>
    [SuppressMessage("", "CA1303")]
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

            var daemonHost = _serviceProvider.GetService<NetDaemonHost>();
            if (daemonHost is null)
            {
                Debug.Fail("Faild to get daemonHost!");
                _globalCancellationSource.Cancel();
                return;
            }
            while (!stoppingToken.IsCancellationRequested && !daemonHost.IsConnected)
            {
                await Task.Delay(2000, stoppingToken).ConfigureAwait(false);
            }

            var testCaseManager = new TestCases(daemonHost);
            if (!testCaseManager.RunTestCases())
            {
                Environment.ExitCode = 0;
            }
            _globalCancellationSource.Cancel();


            // Test case #1 call service and check correct state by wait for result 

            // End test case #2

            Environment.ExitCode = 0;
            _globalCancellationSource.Cancel();
        }

    }
}