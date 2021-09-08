namespace NetDaemon.Daemon
{
    /// <summary>
    /// Provides Text To Speech service
    /// </summary>
    public interface ITextToSpeechService
    {
        void Speak(string entityId, string message);
    }
}