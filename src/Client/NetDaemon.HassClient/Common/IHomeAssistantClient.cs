namespace NetDaemon.Client.Common;

public interface IHomeAssistantClient
{
    Task<IHomeAssistantConnection> ConnectAsync(string host, int port, bool ssl, string token,
        CancellationToken cancelToken);
}