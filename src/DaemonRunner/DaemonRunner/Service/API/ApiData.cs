using System.Collections.Generic;
using NetDaemon.Service.Configuration;

namespace NetDaemon.Service.Api
{

    public class ApiApplication
    {
        public string? Id { get; set; }
        public IEnumerable<string>? Dependencies { get; set; }

        public bool IsEnabled { get; set; }

    }

    public class ApiConfig
    {
        public NetDaemonSettings? DaemonSettings { get; set; }
        public HomeAssistantSettings? HomeAssistantSettings { get; set; }

    }

}