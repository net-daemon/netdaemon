using System;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;

namespace BaseLessNs
{
    [NetDaemonApp]
    public class BaseLessYaml
    {
        private readonly Action<string> _logger;

        public BaseLessYaml(Action<string> logger)
        {
            _logger = logger;
        }

        public string ConfigValue { get; init; }
    }
}

