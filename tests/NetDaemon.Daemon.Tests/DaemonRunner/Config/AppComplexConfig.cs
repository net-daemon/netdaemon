using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetDaemon.Daemon.Tests.DaemonRunner.Config
{
    /// <summary>
    ///     Greets (or insults) people when coming home :)
    /// </summary>
    public class AppComplexConfig : Common.Reactive.NetDaemonRxApp
    {
        public string? AString { get; set; }
        public int? AnInt { get; set; }
        public bool? ABool { get; set; }
        public IEnumerable<string>? AStringList { get; set; }
        public IEnumerable<Device>? Devices { get; set; }
        public override Task InitializeAsync()
        {
            // Do nothing

            return Task.CompletedTask;
        }
    }

    public class Device
    {
        public string? Name { get; set; }
        public IEnumerable<Command>? Commands { get; set; }
    }
    public class Command
    {
        public string? Name { get; set; }
        public string? Data { get; set; }
    }
}