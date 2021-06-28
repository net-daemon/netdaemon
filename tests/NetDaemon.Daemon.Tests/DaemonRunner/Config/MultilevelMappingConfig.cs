namespace NetDaemon.Daemon.Tests.DaemonRunner.Config
{
    public class MultilevelMappingConfig : Common.Reactive.NetDaemonRxApp
    {
        public Node? Root { get; set; }
    }

    public class Node
    {
        public string? Data { get; set; }
        
        public Node? Child { get; set; }
    }
}