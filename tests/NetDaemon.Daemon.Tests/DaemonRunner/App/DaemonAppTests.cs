using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NetDaemon.Common;
using NetDaemon.Common.Configuration;
using NetDaemon.Common.Fluent;
using NetDaemon.Daemon.Config;
using NetDaemon.Service.App;
using Xunit;
using NetDaemon.Daemon.Fakes;

namespace NetDaemon.Daemon.Tests.DaemonRunner.App
{
    public class AppTests
    {
        public static readonly string ConfigFixturePath =
            Path.Combine(AppContext.BaseDirectory, "DaemonRunner", "Fixtures");

        public static readonly string FaultyAppPath =
            Path.Combine(AppContext.BaseDirectory, "DaemonRunner", "FaultyApp");

        public static string GetFixtureContent(string filename) => File.ReadAllText(Path.Combine(AppTests.ConfigFixturePath, filename));

        public static string GetFixturePath(string filename) => Path.Combine(AppTests.ConfigFixturePath, filename);

        public static IOptions<NetDaemonSettings> CreateSettings(string appSource) => new OptionsWrapper<NetDaemonSettings>(new NetDaemonSettings
        {
            AppSource = appSource
        });

        [Fact]
        public void FaultyApplicationShouldLogError()
        {
            // ARRANGE
            var path = Path.Combine(FaultyAppPath, "CompileErrors");
            var loggerMock = new LoggerMock();
            var (daemonApps, _) = DaemonCompiler.GetDaemonApps(path, loggerMock.Logger);
            var netDaemonSettings = CreateSettings(path);

            // ACT
            _ = new CodeManager(daemonApps, loggerMock.Logger, new YamlConfig(netDaemonSettings));

            // ASSERT
            loggerMock.AssertLogged(LogLevel.Error, Times.AtLeastOnce());
        }

        [Fact]
        public void ApplicationShouldBePresentInCsFile()
        {
            // ARRANGE
            var codeManager = CM(Path.Combine(ConfigFixturePath, "level2", "level3"));
            // ACT
            // ASSERT
            Assert.Single(codeManager.DaemonAppTypes.Select(n => n.Name == "LevOneApp"));
        }

        [Fact]
        public void ApplicationShouldBePresentInCsFileWithFullNameNameSpace()
        {
            // ARRANGE
            var codeManager = CM(Path.Combine(ConfigFixturePath, "fullname"));
            // ACT
            // ASSERT
            Assert.Single(codeManager.DaemonAppTypes.Where(n => n.FullName == "TheAppNameSpace.FullNameApp"));
            Assert.Single(codeManager.DaemonAppTypes.Where(n => n.FullName == "TheAppNameSpace2.FullNameApp"));
            Assert.Equal(2, codeManager.DaemonAppTypes.Count());
        }

        [Fact]
        public void ApplicationAllShouldBePresentInCsFile()
        {
            // ARRANGE
            var codeManager = CM(ConfigFixturePath);
            // ACT
            // ASSERT
            Assert.Equal(11, codeManager.DaemonAppTypes.Count());
        }

        [Fact]
        public void InstanceAppFromConfigShouldReturnCorrectType()
        {
            // ARRANGE
            var yamlConfigMock = new Mock<IYamlConfig>();
            yamlConfigMock.Setup(x => x.GetAllConfigFilePaths())
                .Returns(new[] { Path.Combine(ConfigFixturePath, "level2", "level3") });

            IEnumerable<Type> types = new List<Type>() { typeof(AssemblyDaemonApp) };
            const string? yamlConfig = "app:\n  class: NetDaemon.Daemon.Tests.DaemonRunner.App.AssemblyDaemonApp";
            // ACT
            var instances = new YamlAppConfig(types, new StringReader(yamlConfig), yamlConfigMock.Object, "").Instances;
            // ASSERT
            Assert.Single(instances);
            // Assert.Equal(1, instances.Count());
            Assert.NotNull(instances.FirstOrDefault() as INetDaemonApp);
        }

        [Fact]
        public void InstanceAppFromConfigNotFoundShouldReturnNull()
        {
            // ARRANGE
            var yamlConfigMock = new Mock<IYamlConfig>();

            yamlConfigMock.Setup(x => x.GetAllConfigFilePaths())
                .Returns(new[] { Path.Combine(ConfigFixturePath, "level2", "level3") });

            IEnumerable<Type> types = new List<Type>() { typeof(AssemblyDaemonApp) };
            const string? yamlConfig = "app:\n  class: NotFoundApp";

            // ACT
            var instances = new YamlAppConfig(types, new StringReader(yamlConfig), yamlConfigMock.Object, "").Instances;

            // ASSERT
            Assert.Empty(instances);
        }

        [Fact]
        public void InstanceAppFromConfigShouldHaveCorrectProperties()
        {
            // ARRANGE
            var yamlConfigMock = new Mock<IYamlConfig>();
            yamlConfigMock.Setup(x => x.GetAllConfigFilePaths())
                .Returns(new[] { Path.Combine(ConfigFixturePath, "level2", "level3") });

            IEnumerable<Type> types = new List<Type>() { typeof(AssemblyDaemonApp) };
            const string? yamlConfig = @"
        app:
            class: NetDaemon.Daemon.Tests.DaemonRunner.App.AssemblyDaemonApp
            StringConfig: a string
            IntConfig: 10
            EnumerableConfig:
                - string 1
                - string 2
        ";
            // ACT
            var instances = new YamlAppConfig(types, new StringReader(yamlConfig), yamlConfigMock.Object, "").Instances;
            var instance = instances.FirstOrDefault() as AssemblyDaemonApp;
            // ASSERT
            Assert.Equal("a string", instance?.StringConfig);
            Assert.Equal(10, instance?.IntConfig);
            Assert.Equal(2, instance?.EnumerableConfig?.Count());
        }

        [Fact]
        public void InstanceAppFromConfigWithSecretsShouldHaveCorrectProperties()
        {
            // ARRANGE
            var config = new YamlConfig(CreateSettings(Path.Combine(ConfigFixturePath, "level2", "level3")));

            IEnumerable<Type> types = new List<Type>() { typeof(AssemblyDaemonApp) };
            const string? yamlConfig = @"
        app:
            class: NetDaemon.Daemon.Tests.DaemonRunner.App.AssemblyDaemonApp
            test_secret_string: !secret a_secret_string
            test_secret_int: !secret a_secret_int
            test_normal_string: not a secret string
            test_normal_int: 0
        ";
            // ACT
            var instances = new YamlAppConfig(
                types,
                new StringReader(yamlConfig),
                config,
                Path.Combine(ConfigFixturePath, "level2", "level3", "any.cs")
            ).Instances;
            var instance = instances.FirstOrDefault() as AssemblyDaemonApp;
            // ASSERT
            Assert.Equal("this is a secret string", instance?.TestSecretString);
            Assert.Equal(99, instance?.TestSecretInt);
            Assert.Equal(0, instance?.TestNormalInt);
        }

        [Fact]
        public void InstanceAppFromConfigShouldHaveCorrectPropertiesCamelCaseConvert()
        {
            // ARRANGE
            var yamlConfigMock = new Mock<IYamlConfig>();
            yamlConfigMock.Setup(x => x.GetAllConfigFilePaths())
                .Returns(new[] { Path.Combine(ConfigFixturePath, "level2", "level3") });

            IEnumerable<Type> types = new List<Type>() { typeof(AssemblyDaemonApp) };
            const string? yamlConfig = @"
        app:
            class: NetDaemon.Daemon.Tests.DaemonRunner.App.AssemblyDaemonApp
            string_config: a string
            int_config: 10
            enumerable_config:
                - string 1
                - string 2
        ";
            // ACT
            var instances = new YamlAppConfig(types, new StringReader(yamlConfig), yamlConfigMock.Object, "").Instances;
            var instance = instances.FirstOrDefault() as AssemblyDaemonApp;
            // ASSERT
            Assert.Equal("a string", instance?.StringConfig);
            Assert.Equal(10, instance?.IntConfig);
            Assert.Equal(2, instance?.EnumerableConfig?.Count());
        }

        [Fact]
        public void InstanceAppFromConfigFilesInFolderShouldReturnCorrectInstances()
        {
            // ARRANGE
            var path = Path.Combine(ConfigFixturePath, "level2");
            var moqDaemon = new Mock<INetDaemonHost>();
            var moqLogger = new LoggerMock();

            moqDaemon.SetupGet(n => n.Logger).Returns(moqLogger.Logger);
            // ACT
            var codeManager = CM(path);
            // ASSERT
            Assert.Equal(2, codeManager.InstanceDaemonApps().Count());
        }

        [Fact]
        public void InstanceAppMultipleInstancesInConfigShouldReturnCorrectInstances()
        {
            // ARRANGE
            var path = Path.Combine(ConfigFixturePath, "mulitinstance");
            var moqDaemon = new Mock<INetDaemonHost>();
            var moqLogger = new LoggerMock();

            moqDaemon.SetupGet(n => n.Logger).Returns(moqLogger.Logger);
            // ACT
            var codeManager = CM(path);
            // ASSERT
            Assert.Equal(2, codeManager.InstanceDaemonApps().Count());
        }

        [Theory]
        [InlineData("Some string")] //string
        [InlineData(10)]            //integer
        [InlineData(100.5)]         //float
        public async Task StorageShouldReturnSameValueAsSet(object data)
        {
            // ARRANGE
            var yamlConfigMock = new Mock<IYamlConfig>();

            yamlConfigMock.Setup(x => x.GetAllConfigFilePaths())
                .Returns(new[] { Path.Combine(ConfigFixturePath, "level2", "level3") });

            IEnumerable<Type> types = new List<Type>() { typeof(AssemblyDaemonApp) };
            const string? yamlConfig = @"
                app:
                    class: NetDaemon.Daemon.Tests.DaemonRunner.App.AssemblyDaemonApp
                ";

            var daemonMock = new Mock<INetDaemon>();
            daemonMock.SetupGet(x => x.Logger).Returns(new Mock<ILogger>().Object);

            await using var instance = new YamlAppConfig(types, new StringReader(yamlConfig), yamlConfigMock.Object, "").Instances.FirstOrDefault() as AssemblyDaemonApp;

            instance!.Id = "somefake_id";
            instance.InternalStorageObject = new FluentExpandoObject(false, true, daemon: instance);
            instance.Logger = new Mock<ILogger>().Object;

            // ACT
            instance!.Storage.Data = data;

            // ASSERT
            Assert.Equal(data, instance.Storage.Data);
            var stateQueueResult = await instance.InternalLazyStoreStateQueue.Reader.WaitToReadAsync().ConfigureAwait(false);
            Assert.True(stateQueueResult);
        }

        [Fact]
        public async Task StorageShouldRestoreWithCorrectValues()
        {
            // ARRANGE
            var yamlConfigMock = new Mock<IYamlConfig>();

            yamlConfigMock.Setup(x => x.GetAllConfigFilePaths())
                .Returns(new[] { Path.Combine(ConfigFixturePath, "level2", "level3") });

            IEnumerable<Type> types = new List<Type>() { typeof(AssemblyDaemonApp) };
            const string? yamlConfig = @"
                app:
                    class: NetDaemon.Daemon.Tests.DaemonRunner.App.AssemblyDaemonApp
                ";

            await using var instance = new YamlAppConfig(types, new StringReader(yamlConfig), yamlConfigMock.Object, "").Instances.FirstOrDefault() as AssemblyDaemonApp;
            var daemonMock = new Mock<INetDaemon>();
            daemonMock.SetupGet(x => x.Logger).Returns(new Mock<ILogger>().Object);

            var storageItem = new FluentExpandoObject
            {
                ["Data"] = "SomeData"
            };

            daemonMock.SetupGet(x => x.Logger).Returns(new Mock<ILogger>().Object);
            daemonMock.Setup(n => n.GetDataAsync<IDictionary<string, object>>(It.IsAny<string>()))
                .ReturnsAsync((IDictionary<string, object>)storageItem);

            await instance!.StartUpAsync(daemonMock.Object).ConfigureAwait(false);

            // ACT
            await instance.RestoreAppStateAsync().ConfigureAwait(false);

            // ASSERT
            Assert.Equal("SomeData", instance.Storage.Data);
        }

        [Fact]
        public void InsanceAppsThatHasMissingExecuteShouldLogError()
        {
            // ARRANGE
            var path = Path.Combine(FaultyAppPath, "ExecuteWarnings");
            var moqLogger = new LoggerMock();
            var (daemonApps, _) = DaemonCompiler.GetDaemonApps(path, moqLogger.Logger);
            var configMock = new Mock<IYamlConfig>();

            // ACT
            _ = new CodeManager(daemonApps, moqLogger.Logger, configMock.Object);

            // ASSERT
            moqLogger.AssertLogged(LogLevel.Error, Times.Exactly(13));
        }

        [Fact]
        public void InsanceAppsThatHasMissingExecuteAndSupressLogsShouldNotLogError()
        {
            // ARRANGE
            var path = Path.Combine(FaultyAppPath, "SupressLogs");
            var moqLogger = new LoggerMock();
            var (daemonApps, _) = DaemonCompiler.GetDaemonApps(path, moqLogger.Logger);
            var configMock = new Mock<IYamlConfig>();

            // ACT
            _ = new CodeManager(daemonApps, moqLogger.Logger, configMock.Object);

            // ASSERT
            moqLogger.AssertLogged(LogLevel.Error, Times.Exactly(1));
        }

        public static CodeManager CM(string path)
        {
            var loggerMock = new LoggerMock().Logger;
            var config = new YamlConfig(CreateSettings(path));

            var (daemonApps, _) = DaemonCompiler.GetDaemonApps(path, loggerMock);
            return new CodeManager(daemonApps, loggerMock, config);
        }
    }
}