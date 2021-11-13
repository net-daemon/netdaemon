namespace NetDaemon.HassModel.CodeGenerator.Helpers
{
    /// <summary>
    ///     Home Assistant related settings
    /// </summary>
    internal class HomeAssistantSettings
    {
        /// <summary>
        ///     Home Assistant address
        /// </summary>
        public string Host { get; set; } = "localhost";
        /// <summary>
        ///     Home Assistant port
        /// </summary>
        public short Port { get; set; } = 8123;
        /// <summary>
        ///     Connect using ssl
        /// </summary>
        public bool Ssl { get; set; }
        /// <summary>
        ///     Token to authorize
        /// </summary>
        public string Token { get; set; } = string.Empty;
    }
}