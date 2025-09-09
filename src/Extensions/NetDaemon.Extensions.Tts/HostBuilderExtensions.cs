using Microsoft.Extensions.Hosting;

namespace NetDaemon.Extensions.Tts;

/// <summary>
///     Extension methods for text-to-speech
/// </summary>
public static class HostBuilderExtensions
{
    /// <summary>
    ///     Use the text-to-speech engine of NetDaemon
    /// </summary>
    /// <param name="hostBuilder">Builder</param>
    public static IHostBuilder UseNetDaemonTextToSpeech(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices(services =>
        {
            services.AddNetDaemonTextToSpeech();
        });
        return hostBuilder;
    }
}
