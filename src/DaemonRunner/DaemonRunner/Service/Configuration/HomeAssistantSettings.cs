namespace NetDaemon.Service.Configuration
{
    public class HomeAssistantSettings
    {
        public string Host { get; set; } = "localhost";
        public short Port { get; set; } = 8123;
        public bool Ssl { get; set; } = false;
        public string Token { get; set; } = "";
    }
}