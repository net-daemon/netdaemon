using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Extensions.Tts.Internal;

namespace NetDaemon.Extensions.Tts;

/// <summary>
///   Extension methods for text-to-speech
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///    Use the text-to-speech engine of NetDaemon
    /// </summary>
    /// <param name="services"></param>
    public static IServiceCollection AddNetDaemonTextToSpeech(this IServiceCollection services)
    {
        services.AddSingleton<TextToSpeechService>();
        services.AddSingleton<ITextToSpeechService>(s => s.GetRequiredService<TextToSpeechService>());
        return services;
    }
}
