using System.Net;

namespace NetDaemon.Client.Internal.Exceptions;

[SuppressMessage("", "RCS1194")]
public class HomeAssistantApiCallException : Exception
{
    public HttpStatusCode Code { get; private set; }
    public HomeAssistantApiCallException(string? message, HttpStatusCode code) : base(message)
    {
        Code = code;
    }

    public HomeAssistantApiCallException()
    {
    }

    public HomeAssistantApiCallException(string message) : base(message)
    {
    }

    public HomeAssistantApiCallException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
