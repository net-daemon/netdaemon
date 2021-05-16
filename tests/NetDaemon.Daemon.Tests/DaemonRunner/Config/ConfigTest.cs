using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using Moq;
using NetDaemon.Common;
using NetDaemon.Common.Configuration;
using NetDaemon.Common.Reactive;
using NetDaemon.Common.Reactive.Services;
using NetDaemon.Daemon.Config;
using Xunit;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace NetDaemon.Daemon.Tests.DaemonRunner.Config
{
    public class YamlTests
    {
        public static readonly string ConfigFixturePath =
            Path.Combine(AppContext.BaseDirectory, "DaemonRunner", "Fixtures");

        public static string GetFixtureContent(string filename) =>
            File.ReadAllText(Path.Combine(YamlTests.ConfigFixturePath, filename));

        public static string GetFixturePath(string filename) => Path.Combine(YamlTests.ConfigFixturePath, filename);

        public static IOptions<NetDaemonSettings> CreateSettings(string appSource) =>
            new OptionsWrapper<NetDaemonSettings>(new NetDaemonSettings
            {
                AppSource = appSource
            });

        [Fact]
        public void NormalLoadSecretsShouldGetCorrectValues()
        {
            // ARRANGE AND ACT
            IDictionary<string, string> secrets =
                YamlConfig.GetSecretsFromSecretsYaml(GetFixturePath("secrets_normal.yaml"));

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
            const string? faultyYaml =
                "yaml: correctLine\n" +
                "yaml_missing: \"missing" +
                "yaml_correct: 10";

            // ACT & ASSERT
            Assert.Throws<SyntaxErrorException>(
                () => YamlConfig.GetSecretsFromSecretsYaml(new StringReader(faultyYaml)));
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
            var config = new YamlConfig(CreateSettings(ConfigFixturePath));

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
        public void YamlScalarNodeToObjectUsingString()
        {
            // ARRANGE
            const string? yaml = "yaml: string\n";
            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(yaml));
            var root = (YamlMappingNode) yamlStream.Documents[0].RootNode;

            var scalar = root.Children.First();

            var scalarValue = (YamlScalarNode) scalar.Value;
            // ACT & ASSERT
            Assert.Equal("string", scalarValue.ToObject(typeof(string), null));
        }

        [Fact]
        public void YamlScalarNodeToObjectUsingInt()
        {
            // ARRANGE
            const string? yaml = "yaml: 1234\n";
            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(yaml));
            var root = (YamlMappingNode) yamlStream.Documents[0].RootNode;

            var scalar = root.Children.First();

            var scalarValue = (YamlScalarNode) scalar.Value;
            // ACT & ASSERT
            Assert.Equal(1234, scalarValue.ToObject(typeof(int), null));
        }

        [Fact]
        public void YamlScalarNodeToObjectUsingBoolean()
        {
            // ARRANGE
            const string? yaml = "yaml: true\n";
            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(yaml));
            var root = (YamlMappingNode) yamlStream.Documents[0].RootNode;

            var scalar = root.Children.First();

            var scalarValue = (YamlScalarNode) scalar.Value;
            // ACT & ASSERT
            Assert.Equal(true, scalarValue.ToObject(typeof(bool), null));
        }

        [Fact]
        public void YamlScalarNodeToObjectUsingLong()
        {
            // ARRANGE
            const string? yaml = "yaml: 1234\n";
            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(yaml));
            var root = (YamlMappingNode) yamlStream.Documents[0].RootNode;

            var scalar = root.Children.First();

            var scalarValue = (YamlScalarNode) scalar.Value;
            // ACT & ASSERT
            Assert.Equal((long) 1234, scalarValue.ToObject(typeof(long), null));
        }

        [Theory]
        [InlineData(typeof(AlarmControlPanelEntity))]
        [InlineData(typeof(AutomationEntity))]
        [InlineData(typeof(BinarySensorEntity))]
        [InlineData(typeof(CameraEntity))]
        [InlineData(typeof(ClimateEntity))]
        [InlineData(typeof(CoverEntity))]
        [InlineData(typeof(DeviceTrackerEntity))]
        [InlineData(typeof(GroupEntity))]
        [InlineData(typeof(ImageProcessingEntity))]
        [InlineData(typeof(InputBooleanEntity))]
        [InlineData(typeof(LightEntity))]
        [InlineData(typeof(LockEntity))]
        [InlineData(typeof(MediaPlayerEntity))]
        [InlineData(typeof(PersistentNotificationEntity))]
        [InlineData(typeof(PersonEntity))]
        [InlineData(typeof(SceneEntity))]
        [InlineData(typeof(ScriptEntity))]
        [InlineData(typeof(SensorEntity))]
        [InlineData(typeof(SunEntity))]
        [InlineData(typeof(SwitchEntity))]
        [InlineData(typeof(VacuumEntity))]
        [InlineData(typeof(ZoneEntity))]
        public void YamlScalarNotToObjectUsingEntityType(Type entityType)
        {
            // ARRANGE
            const string? yaml = "yaml: 1234\n";
            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(yaml));
            var root = (YamlMappingNode) yamlStream.Documents[0].RootNode;
            var appMock = new Mock<INetDaemonRxApp>();
            var scalar = root.Children.First();

            var scalarValue = (YamlScalarNode) scalar.Value;
            // ACT & ASSERT
            var instance = scalarValue.ToObject(entityType, appMock.Object) as RxEntityBase;
            Assert.NotNull(instance);
            Assert.Equal(entityType,instance!.GetType());
            Assert.Equal("1234", instance.EntityId);
        }

        [Fact]
        public void YamlScalarNodeToObjectUsingDecimal()
        {
            // ARRANGE
            const string? yaml = "yaml: 1.5\n";
            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(yaml));
            var root = (YamlMappingNode) yamlStream.Documents[0].RootNode;

            var scalar = root.Children.First();

            var scalarValue = (YamlScalarNode) scalar.Value;
            // ACT & ASSERT
            Assert.Equal((decimal) 1.5, scalarValue.ToObject(typeof(decimal), null));
            Assert.Equal((float) 1.5f, scalarValue.ToObject(typeof(float), null));
            Assert.Equal((double) 1.5, scalarValue.ToObject(typeof(double), null));
        }

        [Fact]
        public void YamlAdvancedObjectsShouldReturnCorrectData()
        {
            const string? yaml = @"
            a_string: hello world
            an_int: 10
            a_bool: true
            a_string_list:
            - hi
            - this
            - is
            - cool!
            devices:
            - name: tv
              commands:
                - name: command1
                  data: some code
                - name: command2
                  data: some code2";

            var yamlConfig = new YamlAppConfig(
                new List<Type>(),
                new System.IO.StringReader(yaml),
                new YamlConfig(CreateSettings(ConfigFixturePath)),
                ConfigFixturePath
            );

            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(yaml));
            var root = (YamlMappingNode) yamlStream.Documents[0].RootNode;

            var instance =
                (AppComplexConfig?) yamlConfig.InstanceAndSetPropertyConfig(typeof(AppComplexConfig), root, "id");

            Assert.Equal("hello world", instance?.AString);
            Assert.Equal(10, instance?.AnInt);
            Assert.Equal(true, instance?.ABool);
            Assert.NotNull(instance?.Devices);
            Assert.Equal(1, instance?.Devices?.Count());
            Assert.Equal("tv", instance?.Devices?.First().Name);
            Assert.Equal("command1", instance?.Devices?.First()?.Commands?.ElementAt(0).Name);
            Assert.Equal("some code", instance?.Devices?.First()?.Commands?.ElementAt(0).Data);
            Assert.Equal("command2", instance?.Devices?.First()?.Commands?.ElementAt(1).Name);
            Assert.Equal("some code2", instance?.Devices?.First()?.Commands?.ElementAt(1).Data);
        }
    }
}