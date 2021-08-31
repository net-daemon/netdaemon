using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetDaemon.Common;
using NetDaemon.Common.Configuration;
using NetDaemon.Daemon.Config;
using NetDaemon.Service.App;
using Xunit;
using NetDaemon.Daemon.Fakes;
using static NetDaemon.Daemon.Tests.DaemonRunner.CommonTestMethods;

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

        private static ServiceProvider ServiceProvider
        {
            get
            {
                var moqDaemon = new Mock<INetDaemonHost>();
                var moqLogger = new LoggerMock();

                moqDaemon.SetupGet(n => n.Logger).Returns(moqLogger.Logger);

                var serviceProvider = new ServiceCollection()
                    .AddSingleton(moqLogger.Logger)
                    .AddSingleton<INetDaemon>(moqDaemon.Object)
                    .BuildServiceProvider();
                return serviceProvider;
            }
        }

        [Fact]
        public void FaultyApplicationShouldLogError()
        {
            // ARRANGE
            var path = Path.Combine(FaultyAppPath, "CompileErrors");
            var loggerMock = new LoggerMock();
            var (daemonApps, _) = DaemonCompiler.GetDaemonApps(path, loggerMock.Logger);
            var netDaemonSettings = CreateSettings(path);

            // ACT
            _ = new CodeManager(daemonApps, loggerMock.Logger, new YamlConfigProvider(netDaemonSettings, new YamlConfigReader(new IoWrapper())));

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
            Assert.Equal(14, codeManager.DaemonAppTypes.Count());
        }

        [Fact]
        public void ConfigForUnknownAppClassShouldReturnNull()
        {
            // ARRANGE
            const string? yamlConfig = @"
                app:
                    class: NotFoundApp";

            var yamlAppConfigProvider = new YamlAppConfigProvider(new YamlConfigProvider(CreateSettings(ConfigFixturePath), GetYamlConfigReader(yamlConfig)), Mock.Of<ILogger>());

            // ACT
            var yamlConfigEntry = yamlAppConfigProvider.GetConfigs(typeof(AssemblyDaemonApp)).FirstOrDefault();

            // ASSERT
            Assert.Null(yamlConfigEntry);
        }

        [Fact]
        public void InstanceAppWithoutConfigShouldNotBeConfiguredByConfig()
        {
            // ARRANGE
            const string? testString = "a string";
            var yamlConfig = $@"
                app:
                    class: NotFoundApp
                    StringConfig: {testString}";

            var yamlConfigProviderMock = new Mock<IYamlConfig>();

            yamlConfigProviderMock
                    .Setup(x => x.GetAllConfigs())
                    .Returns(() => new[]
                    {
                            new YamlConfigEntry("path is mocked by yamlConfigReader ->", GetYamlConfigReader(yamlConfig), Mock.Of<IYamlSecretsProvider>())
                    });

            var codeManager = new CodeManager(new[] { typeof(AssemblyDaemonApp) }, Mock.Of<ILogger>(), yamlConfigProviderMock.Object);

            // ACT
            var instance = codeManager
                    .InstanceDaemonApps(ServiceProvider)
                    .FirstOrDefault()?
                    .ApplicationInstance as AssemblyDaemonApp;

            // ASSERT
            Assert.NotNull(instance);
            Assert.NotEqual(instance!.StringConfig, testString);
        }

        [Fact]
        public void InstanceAppFromConfigShouldHaveCorrectProperties()
        {
            // ARRANGE
            const string? yamlConfig = @"
            app:
                class: NetDaemon.Daemon.Tests.DaemonRunner.App.AssemblyDaemonApp
                StringConfig: a string
                IntConfig: 10
                EnumerableConfig:
                    - string 1
                    - string 2
            ";

            var yamlConfigProviderMock = new Mock<IYamlConfig>();

            yamlConfigProviderMock
                    .Setup(x => x.GetAllConfigs())
                    .Returns(() => new[]
                    {
                            new YamlConfigEntry("path is mocked by yamlConfigReader ->", GetYamlConfigReader(yamlConfig), Mock.Of<IYamlSecretsProvider>())
                    });

            var codeManager = new CodeManager(new[] { typeof(AssemblyDaemonApp) }, Mock.Of<ILogger>(), yamlConfigProviderMock.Object);

            // ACT
            var instance = codeManager
                    .InstanceDaemonApps(ServiceProvider)
                    .First()
                    .ApplicationInstance as AssemblyDaemonApp;

            // ASSERT
            Assert.Equal("a string", instance?.StringConfig);
            Assert.Equal(10, instance?.IntConfig);
            Assert.Equal(2, instance?.EnumerableConfig?.Count());
        }

        [Fact]
        public void InstanceAppFromConfigWithSecretsShouldHaveCorrectProperties()
        {
            // ARRANGE
            const string? yamlConfig = @"
            app:
                class: NetDaemon.Daemon.Tests.DaemonRunner.App.AssemblyDaemonApp
                test_secret_string: !secret a_secret_string
                test_secret_int: !secret a_secret_int
                test_normal_string: not a secret string
                test_normal_int: 0
            ";
            var yamlConfigDirectory = Path.Combine(ConfigFixturePath, "level2", "level3");
            var yamlConfigFilePath = Path.Combine(yamlConfigDirectory, "any.cs");

            var yamlConfigProviderMock = new Mock<IYamlConfig>();
            yamlConfigProviderMock
                    .Setup(x => x.GetAllConfigs())
                    .Returns(() => new[]
                    {
                        new YamlConfigEntry(yamlConfigFilePath, GetYamlConfigReader(yamlConfig), new YamlSecretsProvider(yamlConfigDirectory, GetYamlConfigReader()))
                    });

            var codeManager = new CodeManager(new[] { typeof(AssemblyDaemonApp) }, Mock.Of<ILogger>(), yamlConfigProviderMock.Object);

            // ACT
            var instance = codeManager
                    .InstanceDaemonApps(ServiceProvider)
                    .First()
                    .ApplicationInstance as AssemblyDaemonApp;

            //ASSERT
            Assert.Equal("this is a secret string", instance?.TestSecretString);
            Assert.Equal(99, instance?.TestSecretInt);
            Assert.Equal(0, instance?.TestNormalInt);
        }

        [Fact]
        public void InstanceAppFromConfigShouldHaveCorrectPropertiesCamelCaseConvert()
        {
            // ARRANGE
            const string? yamlConfig = @"
            app:
                class: NetDaemon.Daemon.Tests.DaemonRunner.App.AssemblyDaemonApp
                string_config: a string
                int_config: 10
                enumerable_config:
                    - string 1
                    - string 2
            ";

            var yamlConfigProviderMock = new Mock<IYamlConfig>();

            yamlConfigProviderMock
                    .Setup(x => x.GetAllConfigs())
                    .Returns(() => new[]
                    {
                            new YamlConfigEntry("path is mocked by yamlConfigReader ->", GetYamlConfigReader(yamlConfig), Mock.Of<IYamlSecretsProvider>())
                    });

            var codeManager = new CodeManager(new[] { typeof(AssemblyDaemonApp) }, Mock.Of<ILogger>(), yamlConfigProviderMock.Object);

            // ACT
            var instance = codeManager
                    .InstanceDaemonApps(ServiceProvider)
                    .First()
                    .ApplicationInstance as AssemblyDaemonApp;

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
            var serviceProvider = ServiceProvider;

            // ACT
            var codeManager = CM(path);
            // ASSERT
            Assert.Equal(2, codeManager.InstanceDaemonApps(serviceProvider).Count());
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
            Assert.Equal(2, codeManager.InstanceDaemonApps(ServiceProvider).Count());
        }

        [Fact]
        public async Task StorageShouldRestoreWithCorrectValues()
        {
            // ARRANGE

            await using var appContext = new ApplicationContext(typeof(AssemblyDaemonApp), "id", ServiceProvider);
            var instance = appContext.ApplicationInstance as AssemblyDaemonApp;

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
        public void InstanceAppFromConfigFilesInFolderReceiveInjectedDependencies()
        {
            // ARRANGE
            var path = Path.Combine(ConfigFixturePath, "injection");
            var moqDaemon = new Mock<INetDaemonHost>();
            var moqLogger = new LoggerMock();

            moqDaemon.SetupGet(n => n.Logger).Returns(moqLogger.Logger);
            // ACT
            var codeManager = CM(path);
            var mockAction = new Mock<Action<string>>();

            var serviceProvider = new ServiceCollection()
                .AddSingleton(moqLogger.Logger)
                .AddSingleton<INetDaemon>(moqDaemon.Object)
                .AddSingleton(mockAction.Object)
                .BuildServiceProvider();

            // ASSERT
            Assert.Equal(1, codeManager.InstanceDaemonApps(serviceProvider).Count());
            mockAction.Verify(a => a("Hello logger"));
        }

        [Fact]
        public void YamllessAppShouldBeInitialized()
        {
            // ARRANGE
            var codeManager = CM(Path.Combine(ConfigFixturePath, "yamlless"));
            // ACT
            // ASSERT
            Assert.Contains(codeManager.DaemonAppTypes, type => type.FullName == "YamllessApp");
        }

        [Fact]
        public void YamllessAppShouldBeIdentifiedUsingClassName()
        {
            // ARRANGE
            var appTypeName = "YamllessApp";
            var codeManager = CM(Path.Combine(ConfigFixturePath, "yamlless"));
            // ACT
            // ASSERT
            var appId = codeManager
                    .InstanceDaemonApps(ServiceProvider)
                    .FirstOrDefault(app => app.ApplicationInstance.GetType().Name == appTypeName)
                    ?.Id;

            Assert.Equal(appTypeName, appId);
        }

        [Fact]
        public void YamllessAppShouldBeIdentifiedUsingAttribute()
        {
            // ARRANGE
            var appTypeName = "YamllessAppIdentifiedUsingAttribute";
            var expectedAppId = "This app is identified using attribute";

            var codeManager = CM(Path.Combine(ConfigFixturePath, "yamlless"));
            // ACT
            // ASSERT
            var appId = codeManager
                    .InstanceDaemonApps(ServiceProvider)
                    .FirstOrDefault(app => app.ApplicationInstance.GetType().Name == appTypeName)
                    ?.Id;

            Assert.Equal(expectedAppId, appId);
        }

        public static CodeManager CM(string path)
        {
            var loggerMock = new LoggerMock().Logger;
            var config = new YamlConfigProvider(CreateSettings(path), GetYamlConfigReader());

            var (daemonApps, _) = DaemonCompiler.GetDaemonApps(path, loggerMock);
            return new CodeManager(daemonApps, loggerMock, config);
        }
    }
}