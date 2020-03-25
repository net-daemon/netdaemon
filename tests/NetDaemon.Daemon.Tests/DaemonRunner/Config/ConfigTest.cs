using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service;
using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.Config;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Xunit;
using YamlDotNet.Core;

namespace NetDaemon.Daemon.Tests.DaemonRunner.Config
{
    public class YamlTests
    {
        public static readonly string ConfigFixturePath =
            Path.Combine(AppContext.BaseDirectory, "DaemonRunner", "Fixtures");

        public string GetFixtureContent(string filename) => File.ReadAllText(Path.Combine(YamlTests.ConfigFixturePath, filename));

        public string GetFixturePath(string filename) => Path.Combine(YamlTests.ConfigFixturePath, filename);

        [Fact]
        public void NormalLoadSecretsShouldGetCorrectValues()
        {
            // ARRANGE AND ACT
            IDictionary<string, string> secrets = YamlConfig.GetSecretsFromSecretsYaml(GetFixturePath("secrets_normal.yaml"));

            // ASSERT
            Assert.True(secrets.ContainsKey("secret_int"));
            Assert.Equal("10", secrets["secret_int"]);

            Assert.True(secrets.ContainsKey("secret_string"));
            Assert.Equal("hello", secrets["secret_string"]);

            Assert.True(secrets.ContainsKey("secret_string_noquotes"));
            Assert.Equal("hello no quotes", secrets["secret_string_noquotes"]);
        }

        [Fact]
        public void SecretsLoadFaultyYamlThrowsException()
        {
            // ARRANGE
            var faultyYaml =
            "yaml: correctLine\n" +
            "yaml_missing: \"missing" +
            "yaml_correct: 10";

            // ACT & ASSERT
            Assert.Throws<SyntaxErrorException>(() => YamlConfig.GetSecretsFromSecretsYaml(new StringReader(faultyYaml)));
        }

        [Fact]
        public void SecretLoadAllSecretsInDictionaryShouldBeCorrect()
        {
            // ARRANGE & ACT
            var secrets = YamlConfig.GetAllSecretsFromPath(ConfigFixturePath);

            // ASSERT
            Assert.Equal(3, secrets.Count);
            Assert.True(secrets.ContainsKey(ConfigFixturePath));
            Assert.True(secrets.ContainsKey(Path.Combine(ConfigFixturePath, "level2")));
            Assert.True(secrets.ContainsKey(Path.Combine(ConfigFixturePath, "level2", "level3")));

            Assert.Equal("test", secrets[ConfigFixturePath]["secret_only_exists_here"]);
            Assert.Equal("20", secrets[Path.Combine(ConfigFixturePath, "level2")]["secret_int"]);
            Assert.Equal("40", secrets[Path.Combine(ConfigFixturePath, "level2", "level3")]["secret_int"]);
        }

        [Theory]
        [MemberData(nameof(SecretData))]
        public void SecretShouldBeRelevantDependingOnFolderLevel(string secret, string configPath, string secretValue)
        {
            // ARRANGE
            var config = new YamlConfig(ConfigFixturePath);

            // ACT

            string? confValue = config.GetSecretFromPath(secret, configPath);

            // ASSERT
            Assert.Equal(secretValue, confValue);
        }

        public static IEnumerable<object?[]> SecretData =>
            new List<Object?[]>
            {
                new object?[] {"secret_int", ConfigFixturePath, "10"},
                new object?[] {"secret_int", Path.Combine(ConfigFixturePath, "level2"), "20"},
                new object?[] {"secret_int", Path.Combine(ConfigFixturePath, "level2", "level3"), "40"},
                new object?[] {"string_setting", Path.Combine(ConfigFixturePath, "level2", "level3"), "level2"},
                new object?[] {"secret_only_exists_here", Path.Combine(ConfigFixturePath, "level2", "level3"), "test"},
                new object?[] {"notexists", Path.Combine(ConfigFixturePath, "level2", "level3"), null},
            };

        [Fact]
        public void JSonSerializeShouldBeCorrectForConfig()
        {
            // ARRANGE & ACT
            var secrets = YamlConfig.GetAllSecretsFromPath(ConfigFixturePath);

            // ASSERT
            var x = new HostConfig
            {
                LoggingLevel = LogLevel.Information,
                Token = "1234",
                Host = "host",
                Port = 1234,
                Ssl = true,
                SourceFolder = "somefolder"
            };

            var obj = JsonSerializer.Serialize<HostConfig>(x);

            Assert.True(obj.Contains("log_level"));
            Assert.True(obj.Contains("token"));
            Assert.True(obj.Contains("host"));
            Assert.True(obj.Contains("port"));
            Assert.True(obj.Contains("ssl"));
            Assert.True(obj.Contains("source_folder"));
        }
    }
}