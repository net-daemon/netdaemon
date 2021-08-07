namespace NetDaemon.Common
{
    internal class ApplicationMetadata : IApplicationMetadata
    {
        public string? Id { get; set; }
        public string? Description { get; set; }
        public bool IsEnabled { get; set; } = true;
        public AppRuntimeInfo RuntimeInfo { get; } = new ();
    }
}