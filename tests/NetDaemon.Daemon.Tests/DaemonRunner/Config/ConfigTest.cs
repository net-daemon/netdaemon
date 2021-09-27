using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
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
using static NetDaemon.Daemon.Tests.DaemonRunner.CommonTestMethods;

namespace NetDaemon.Daemon.Tests.DaemonRunner.Config
{
    public class YamlTests
    {
        public static readonly string ConfigFixturePath =
            Path.Combine(AppContext.BaseDirectory, "DaemonRunner", "Fixtures");

        public static string GetFixturePath(string filename) => Path.Combine(ConfigFixturePath, filename);

        public static IOptions<NetDaemonSettings> CreateSettings(string appSource) =>
            new OptionsWrapper<NetDaemonSettings>(new NetDaemonSettings
            {
                    AppSource = appSource
            });

        private readonly IServiceProvider _serviceProvider = new ServiceCollection().BuildServiceProvider();

        private ApplicationContext TestAppContext => ApplicationContext.Create(typeof(object), "id", _serviceProvider, Mock.Of<INetDaemon>());

        [Fact]
        public void NormalLoadSecretsShouldGetCorrectValues()
        {
            // ARRANGE AND ACT
            var yamlConfig = new YamlSecretsProvider(ConfigFixturePath, new YamlConfigReader(new IoWrapper()));
            IDictionary<string, string> secrets =
                yamlConfig.GetSecretsFromFile(GetFixturePath("secrets_normal.yaml"));

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

            var yamlConfigReader = GetYamlConfigReader(faultyYaml);

            // ACT & ASSERT
            Assert.Throws<SyntaxErrorException>(
                () => yamlConfigReader.GetYamlStream(faultyYaml));
        }

        [Fact]
        public void SecretLoadAllSecretsInDictionaryShouldBeCorrect()
        {
            // ARRANGE & ACT
            var yamlConfig = new YamlSecretsProvider(ConfigFixturePath, new YamlConfigReader(new IoWrapper()));
            var secrets = yamlConfig.GetAllSecrets();

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
            var yamlSecretsProvider = new YamlSecretsProvider(ConfigFixturePath, new YamlConfigReader(new IoWrapper()));

            // ACT

            string? confValue = yamlSecretsProvider.GetSecretFromPath(secret, configPath);

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
                new object?[]
                {
                        "secret_only_exists_here", Path.Combine(ConfigFixturePath, "level2", "level3"), "test"
                },
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
            Assert.Equal("string", scalarValue.ToObject(typeof(string), TestAppContext));
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
            Assert.Equal(1234, scalarValue.ToObject(typeof(int), TestAppContext));
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
            Assert.Equal(true, scalarValue.ToObject(typeof(bool), TestAppContext));
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
            Assert.Equal((long) 1234, scalarValue.ToObject(typeof(long), TestAppContext));
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
            using var applicationContext = ApplicationContext.Create(typeof(BaseTestApp), "id", _serviceProvider, Mock.Of<INetDaemon>());
            var instance = scalarValue.ToObject(entityType, applicationContext) as RxEntityBase;
            Assert.NotNull(instance);
            Assert.Equal(entityType, instance!.GetType());
            Assert.Equal("1234", instance.EntityId);
        }

        [Fact]
        public void YamlScalarToObjectUsingAnyType()
        {
            // ARRANGE
            const string? yaml = "yaml: 1234\n";
            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(yaml));
            var root = (YamlMappingNode) yamlStream.Documents[0].RootNode;
            var scalar = root.Children.First();

            var scalarValue = (YamlScalarNode) scalar.Value;
            var mockAction = new Mock<ISpeaker>();
            var serviceProvider = new ServiceCollection()
                .AddSingleton(mockAction.Object)
                .BuildServiceProvider();

            using var applicationContext = ApplicationContext.Create(typeof(object), "Id", serviceProvider, Mock.Of<INetDaemon>());

            // ACT & ASSERT
            var instance = scalarValue.ToObject(typeof(TestClass), applicationContext);
            Assert.NotNull(instance);
            Assert.Equal(typeof(TestClass), instance!.GetType());
            mockAction.Verify(a => a.Say("Hello 1234"));
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
            Assert.Equal(1.5m, scalarValue.ToObject(typeof(decimal), TestAppContext));
            Assert.Equal(1.5f, scalarValue.ToObject(typeof(float), TestAppContext));
            Assert.Equal(1.5d, scalarValue.ToObject(typeof(double), TestAppContext));
        }

        [Fact]
        public void YamlScalarNodeToObjectUsingEnum()
        {
            // ARRANGE
            const string? yaml = "yaml: Second\n";
            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(yaml));
            var root = (YamlMappingNode) yamlStream.Documents[0].RootNode;

            var scalar = root.Children.First();

            var scalarValue = (YamlScalarNode) scalar.Value;
            // ACT & ASSERT
            Assert.Equal(EnumTest.Second, scalarValue.ToObject(typeof(EnumTest), TestAppContext));
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

            var instance = InstantiateAppAndSetPropertyConfig<AppComplexConfig>(yaml);

            Assert.Equal("hello world", instance.AString);
            Assert.Equal(10, instance.AnInt);
            Assert.Equal(true, instance.ABool);
            Assert.NotNull(instance.Devices);
            Assert.Equal(1, instance.Devices?.Count());
            Assert.Equal("tv", instance.Devices?.First().Name);
            Assert.Equal("command1", instance.Devices?.First()?.Commands?.ElementAt(0).Name);
            Assert.Equal("some code", instance.Devices?.First()?.Commands?.ElementAt(0).Data);
            Assert.Equal("command2", instance.Devices?.First()?.Commands?.ElementAt(1).Name);
            Assert.Equal("some code2", instance.Devices?.First()?.Commands?.ElementAt(1).Data);
        }

        [Fact]
        public void YamlMultilevelObjectShouldReturnCorrectData()
        {
            const string? rootData = "lorem ipsum 1";
            const string? childData = "lorem ipsum 2";

            var yaml = $@"
            root:
             data: {rootData}
             child:
               child:
                 data: {childData}";

            var instance = InstantiateAppAndSetPropertyConfig<MultilevelMappingConfig>(yaml);

            Assert.Equal(rootData, instance!.Root!.Data);
            Assert.Equal(childData, instance!.Root!.Child!.Child!.Data!);
        }

        private T InstantiateAppAndSetPropertyConfig<T>(string yaml)
        {
            var yamlStream = new YamlStream();
            yamlStream.Load(new StringReader(yaml));
            var root = (YamlMappingNode) yamlStream.Documents[0].RootNode;

            using var applicationContext =
                ApplicationContext.Create(typeof(T), "id", _serviceProvider, Mock.Of<INetDaemon>());
            var instance = (T) applicationContext.ApplicationInstance;

            var config = new YamlConfigEntry("path is mocked by yamlConfigReader ->",
                    GetYamlConfigReader(yaml),
                    Mock.Of<IYamlSecretsProvider>());
            var yamlAppConfigEntry = new YamlAppConfigEntry("id", root, config);

            yamlAppConfigEntry.SetPropertyConfig(applicationContext);

            return instance;
        }
    }

#pragma warning disable CA1812 // internal class is instantiated dynamically

    internal class TestClass
#pragma warning restore CA1812
    {
        public TestClass(ISpeaker speaker, string name)
        {
            speaker.Say("Hello " + name);
        }
    }

    public interface ISpeaker
    {
        public void Say(string message);
    }
}