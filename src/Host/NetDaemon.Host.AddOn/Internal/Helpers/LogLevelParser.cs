using System;
using Microsoft.Extensions.Logging;
using Serilog;

namespace NetDaemon.Host.AddOn.Internal.Helpers;

internal static class LogLevelParser
{
    public static LogLevel ParseLogLevelFromSetting(string? logLevelSetting)
    {
        logLevelSetting ??= "information";
        // try parsing the loglevel ignoring case since
        // home assistant add-ons usually is configured with
        // lower case only
        return Enum.TryParse(logLevelSetting, true, out LogLevel logLevel) ? logLevel : LogLevel.Information;
    }
}