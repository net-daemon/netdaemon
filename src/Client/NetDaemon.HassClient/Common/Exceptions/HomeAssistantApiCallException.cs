using System.Net;

namespace NetDaemon.Client.Internal.Exceptions;

public class HomeAssistantApiCallException : ApplicationException
{
    public HttpStatusCode Code { get; private set; }
    public HomeAssistantApiCallException(string? message, HttpStatusCode code) : base(message)
    {
        Code = code;
    }

}