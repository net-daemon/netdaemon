using System.Collections.Generic;
using System.Threading.Tasks;
using NetDaemon.Common;

namespace NetDaemon.Daemon.Tests.DaemonRunner.App
{
    /// <summary>
    ///     Greets (or insults) people when coming home :)
    /// </summary>
    public class AssemblyDaemonApp : Common.NetDaemonApp
    {
        #region -- Test config --

        public string? StringConfig { get; set; }
        public int? IntConfig { get; set; }
        public IEnumerable<string>? EnumerableConfig { get; set; }

        #endregion -- Test config --

        #region -- Test secrets --

        public string? TestSecretString { get; set; }
        public int? TestSecretInt { get; set; }

        public string? TestNormalString { get; set; }
        public int? TestNormalInt { get; set; }

        #endregion -- Test secrets --

        // For testing
        public bool HandleServiceCallIsCalled { get; set; }

        public override Task InitializeAsync()
        {
            // Do nothing

            return Task.CompletedTask;
        }

        [HomeAssistantServiceCall]
        public Task HandleServiceCall(dynamic _)
        {
            HandleServiceCallIsCalled = true;
            return Task.CompletedTask;
        }
    }
}