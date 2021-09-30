using System;
using Microsoft.Extensions.Logging;
using NetDaemon.Common;

namespace BaseLessNs
{
    [NetDaemonApp]
    public class BaseLessYaml
    {
        public BaseLessYaml()
        { }

        public string ConfigValue { get; init; }
    }
}

