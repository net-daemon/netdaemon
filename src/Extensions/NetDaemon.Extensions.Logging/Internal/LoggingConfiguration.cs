using System.Diagnostics.CodeAnalysis;

namespace NetDaemon.Extensions.Logging.Internal;

internal class LoggingConfiguration
{
    public LogLevel? LogLevel { get; set; }
    public string ConsoleThemeType { get; set; } = "Ansi";
}

[SuppressMessage("", "CA1812")]
internal class LogLevel
{
    public string Default { get; set; } = "Info";
}