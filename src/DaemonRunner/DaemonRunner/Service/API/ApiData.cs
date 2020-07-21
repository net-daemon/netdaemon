using System.Collections.Generic;

namespace NetDaemon.Service.Api
{

    public class ApiApplication
    {
        public string? Id { get; set; }
        public IEnumerable<string>? Dependencies { get; set; }

        public bool IsEnabled { get; set; }

    }

}