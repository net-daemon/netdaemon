namespace NetDaemon.Service.Configuration
{
    public class NetDaemonSettings
    {
        public bool? GenerateEntities { get; set; } = false;
        public bool? Admin { get; set; } = false;
        public string? SourceFolder { get; set; } = null;
        public string? ProjectFolder { get; set; } = string.Empty;
    }
}