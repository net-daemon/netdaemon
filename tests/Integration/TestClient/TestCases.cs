using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using NetDaemon.Daemon;

namespace TestClient
{
    public class TestCases
    {
        private readonly NetDaemonHost _daemonHost;
        private readonly CancellationToken _stoppingToken;
        private int _testCaseNumber;

        public TestCases(NetDaemonHost daemonHost, CancellationToken stoppingToken)
        {
            _daemonHost = daemonHost;
            _stoppingToken = stoppingToken;
        }

        /// <summary>
        ///     Runs all testcases
        /// </summary>
        internal async Task<bool> RunTestCases()
        {
            if (!await TestCallServiceAndCheckCorrectState().ConfigureAwait(false))
                return false;
            if (!await TestCallServiceAndCheckCorrectStateAndWaitForResult().ConfigureAwait(false))
                return false;
            return true;
        }

        [SuppressMessage("", "CA1849")]
        public async Task<bool> TestCallServiceAndCheckCorrectState()
        {
            LogTestCase();
            _daemonHost.CallService("input_select", "select_option", new { entity_id = "input_select.who_cooks", option = "Paulus" }); //.ConfigureAwait(false);
            await Task.Delay(2000, _stoppingToken).ConfigureAwait(false);

            return FailEq<string>(_daemonHost.GetState("input_select.who_cooks")?.State, "Paulus");
        }

        [SuppressMessage("", "CA1849")]
        public async Task<bool> TestCallServiceAndCheckCorrectStateAndWaitForResult()
        {
            LogTestCase();
            _daemonHost.CallService("input_select", "select_option", new { entity_id = "input_select.who_cooks", option = "Anne Therese" }, true); //.ConfigureAwait(false);
            await Task.Delay(300, _stoppingToken).ConfigureAwait(false);

            return FailEq<string>(_daemonHost.GetState("input_select.who_cooks")?.State, "Anne Therese");
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
                Console.WriteLine($"EXPECTED: {expected}, GOT: {actual}");
            }
            return IsEqual;
        }
    }
}