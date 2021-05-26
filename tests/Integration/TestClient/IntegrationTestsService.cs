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

        private int _testCaseNumber;

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
            LogTestCase();
            daemonHost.CallService("input_select", "select_option", new {entity_id = "input_select.who_cooks", option="Paulus"}); //.ConfigureAwait(false);
            await Task.Delay(2000, stoppingToken).ConfigureAwait(false);

            if (FailEq<string>(daemonHost.GetState("input_select.who_cooks")?.State, "Paulus"))
                return;
            // End test case #1

            LogTestCase();
            daemonHost.CallService("input_select", "select_option", new {entity_id = "input_select.who_cooks", option="Anne Therese"}, true); //.ConfigureAwait(false);
            await Task.Delay(300, stoppingToken).ConfigureAwait(false);

            if (FailEq<string>(daemonHost.GetState("input_select.who_cooks")?.State, "Paulus"))
                return;

            Environment.ExitCode = 0;
            _globalCancellationSource.Cancel();
        }

        private void LogTestCase()
        {
            if (_testCaseNumber > 0)
            {
                Console.WriteLine("");
                Console.WriteLine("");
            }
            _testCaseNumber++;
            Console.WriteLine($"-------- Test case: {_testCaseNumber} --------");
        }
        private static bool FailEq<T>(T actual, T expected)
        {
            var IsEqual = actual?.Equals(expected) ?? false;
            if (!IsEqual)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Write("ERROR: ");
                Console.ResetColor();
                Console.Write("EXPECTED: ");
                Console.BackgroundColor = ConsoleColor.Green;
                Console.Write($"{expected}");
                Console.ResetColor();
                Console.Write(", GOT: ");
                Console.BackgroundColor = ConsoleColor.Yellow;
                Console.Write($"{actual}");
                Console.ResetColor();
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