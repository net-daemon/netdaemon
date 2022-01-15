namespace NetDaemon.Extensions.Tts;

/// <summary>
///     Provides Text To Speech service
/// </summary>
public interface ITextToSpeechService
{
    /// <summary>
    ///     Speak a message to a media player
    /// </summary>
    /// <param name="entityId">Entity id of the media player to play speech</param>
    /// <param name="message">The message spoken</param>
    /// <param name="service">The tts service to use</param>
    /// <param name="language">The language to use</param>
    /// <param name="options">Tts provider specific options</param>
    void Speak(string entityId, string message, string service, string? language = null, object? options = null);
}