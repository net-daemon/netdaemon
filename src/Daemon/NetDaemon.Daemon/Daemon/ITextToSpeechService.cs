namespace NetDaemon.Daemon
{
    // todo: move this interface to another project / namespace? 

    /// <summary>
    /// Provides Text To Speech service
    /// </summary>
    public interface ITextToSpeechService
    {
        void Speak(string entityId, string message);
    }
}