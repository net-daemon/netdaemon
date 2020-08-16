namespace NetDaemon.Common.Configuration
{
    /// <summary>
    ///     Settings related to NetDaemon instance
    /// </summary>
    public class NetDaemonSettings
    {
        /// <summary>
        ///     Set true to generate entieies from Home Assistant
        /// </summary>
        public bool? GenerateEntities { get; set; } = false;
        /// <summary>
        ///     If Admin gui would be  used
        /// </summary>
        public bool? Admin { get; set; } = false;
        /// <summary>
        ///     Where the apps are found
        /// </summary>
        public string? SourceFolder { get; set; } = null;
        /// <summary>
        ///     Points to non default csproj file
        /// </summary>
        public string? ProjectFolder { get; set; } = string.Empty;
    }
}