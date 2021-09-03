using YamlDotNet.RepresentationModel;

namespace NetDaemon.Daemon.Config
{
    public class YamlConfigEntry
    {
        private readonly IYamlSecretsProvider _yamlSecretsProvider;
        private readonly IYamlConfigReader _configReader;
        private readonly string _path;

        public YamlConfigEntry(string path,
                IYamlConfigReader configReader,
                IYamlSecretsProvider yamlSecretsProvider)
        {
            _path = path;
            _yamlSecretsProvider = yamlSecretsProvider;
            _configReader = configReader;
        }

        public string? GetSecret(string secret)
        {
            return _yamlSecretsProvider.GetSecretFromPath(secret, _path);
        }

        public YamlStream GetYamlStream()
        {
            return _configReader.GetYamlStream(_path);
        }
    }
}