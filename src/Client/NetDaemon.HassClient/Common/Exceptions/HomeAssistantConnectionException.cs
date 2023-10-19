namespace NetDaemon.Client.Exceptions;

// We allow exception not to have default constructors since
// in this case it only makes sense to have a reason
[SuppressMessage("", "RCS1194")]
public class HomeAssistantConnectionException : Exception
{
    public HomeAssistantConnectionException(DisconnectReason reason) : base(
        $"Home assistant disconnected reason:{reason}")
    {
        Reason = reason;
    }

    public DisconnectReason Reason { get; set; }

    public HomeAssistantConnectionException()
    {
    }

    public HomeAssistantConnectionException(string message) : base(message)
    {
    }

    public HomeAssistantConnectionException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
