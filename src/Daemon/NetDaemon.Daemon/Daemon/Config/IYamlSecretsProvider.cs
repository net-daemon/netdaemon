namespace NetDaemon.Daemon.Config
{
    public interface IYamlSecretsProvider
    {
        string? GetSecretFromPath(string secret, string path);
    }
}