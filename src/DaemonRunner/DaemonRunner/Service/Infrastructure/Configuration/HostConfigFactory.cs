using Microsoft.Extensions.Configuration;

namespace NetDaemon.Service.Infrastructure.Configuration
{
    public class HostConfigFactory
    {
        private readonly IConfiguration _configuration;

        public HostConfigFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public HostConfig Create()
        {
            var config = new HostConfig();

            var host = GetSetting(_configuration, "Host", "HASS_HOST");
            var port = GetSettingShort(_configuration, "port", "HASS_PORT");
            var token = GetSetting(_configuration, "token", "HASS_TOKEN");
            var sourceFolder = GetSetting(_configuration, "source_folder", "HASS_DAEMONAPPFOLDER");
            
            config.Ssl = GetSettingBool(_configuration, "ssl", "HASS_SSL").GetValueOrDefault(false);
            config.GenerateEntitiesOnStartup = GetSettingBool(_configuration, "generate_entities", "HASS_GEN_ENTITIES").GetValueOrDefault(false);

            if (string.IsNullOrEmpty(host))
                return null!;

            config.Host = host;

            if (port.HasValue)
                config.Port = port.Value;

            if (!string.IsNullOrEmpty(token))
                config.Token = token;
            if (!string.IsNullOrEmpty(sourceFolder))
                config.SourceFolder = sourceFolder;

            return config;
        }


        private bool? GetSettingBool(IConfiguration configuration, string configName, string envName)
        {
            var theValue = GetSetting(configuration, configName, envName);

            if (string.IsNullOrEmpty(theValue))
                return null;

            return bool.Parse(theValue);
        }

        private short? GetSettingShort(IConfiguration configuration, string configName, string envName)
        {
            var theValue = GetSetting(configuration, configName, envName);

            if (string.IsNullOrEmpty(theValue))
                return null;

            return short.Parse(theValue);
        }

        private string GetSetting(IConfiguration configuration, string configName, string envName)
        {
            if (!string.IsNullOrEmpty(configuration[envName]))
                return configuration[envName];

            return configuration[configName];
        }
    }
}