using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JoySoftware.HomeAssistant.Client;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.Daemon;
using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.App;
using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.Config;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace NetDaemon.Daemon.Tests.DaemonRunner.App
{
    public class AppTests
    {
        public static readonly string ConfigFixturePath =
            Path.Combine(AppContext.BaseDirectory, "DaemonRunner", "Fixtures");

        public string GetFixtureContent(string filename) => File.ReadAllText(Path.Combine(AppTests.ConfigFixturePath, filename));

        public string GetFixturePath(string filename) => Path.Combine(AppTests.ConfigFixturePath, filename);


        [Fact]
        public void NrOfCsFilesShouldBeCorrect()
        {
            // ARRANGE
            var targetCount = Directory.EnumerateFiles(ConfigFixturePath, "*.cs", SearchOption.AllDirectories).Count();

            // ACT
            IEnumerable<string> files = CodeManager.GetCsFiles(ConfigFixturePath);

            // ASSERT
            Assert.Equal(targetCount, files.Count());
        }

        [Fact]
        public void ApplicationShouldBePresentInCsFile()
        {
            // ARRANGE
            var codeManager = new CodeManager(Path.Combine(ConfigFixturePath, "level2", "level3"));
            // ACT
            // ASSERT
            Assert.Single(codeManager.DaemonAppTypes.Select(n => n.Name == "LevOneApp"));
        }

        [Fact]
        public void ApplicationAllShouldBePresentInCsFile()
        {
            // ARRANGE
            var codeManager = new CodeManager(ConfigFixturePath);
            // ACT
            // ASSERT
            Assert.Equal(5, codeManager.DaemonAppTypes.Select(n => n.Name.StartsWith("Lev")).Count());

        }

        [Fact]
        public void InstanceAppFromConfigShouldReturnCorrectType()
        {
            // ARRANGE
            var yamlConfigMock = new Mock<YamlConfig>(Path.Combine(ConfigFixturePath, "level2", "level3"));
            IEnumerable<Type> types = new List<Type>() { typeof(AssmeblyDaemonApp) };
            var yamlConfig = "app:\n\tclass: AssmeblyDaemonApp";
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
            var yamlConfigMock = new Mock<YamlConfig>(Path.Combine(ConfigFixturePath, "level2", "level3"));
            IEnumerable<Type> types = new List<Type>() { typeof(AssmeblyDaemonApp) };
            var yamlConfig = "app:\n\tclass: NotFoundApp";
            // ACT
            var instances = new YamlAppConfig(types, new StringReader(yamlConfig), yamlConfigMock.Object, "").Instances;
            // ASSERT
            Assert.Empty(instances);
        }

        [Fact]
        public void InstanceAppFromConfigShouldHaveCorrectProperties()
        {
            // ARRANGE
            var yamlConfigMock = new Mock<YamlConfig>(Path.Combine(ConfigFixturePath, "level2", "level3"));
            IEnumerable<Type> types = new List<Type>() { typeof(AssmeblyDaemonApp) };
            var yamlConfig = @"
app:
    class: AssmeblyDaemonApp
    StringConfig: a string
    IntConfig: 10
    EnumerableConfig:
        - string 1
        - string 2
";
            // ACT
            var instances = new YamlAppConfig(types, new StringReader(yamlConfig), yamlConfigMock.Object, "").Instances;
            var instance = instances.FirstOrDefault() as AssmeblyDaemonApp;
            // ASSERT
            Assert.Equal("a string", instance?.StringConfig);
            Assert.Equal(10, instance?.IntConfig);
            Assert.Equal(2, instance?.EnumerableConfig.Count());
        }

        [Fact]
        public void InstanceAppFromConfigWithSecretsShouldHaveCorrectProperties()
        {
            // ARRANGE
            var yamlConfigMock = new Mock<YamlConfig>(Path.Combine(ConfigFixturePath, "level2", "level3"));
            IEnumerable<Type> types = new List<Type>() { typeof(AssmeblyDaemonApp) };
            var yamlConfig = @"
app:
    class: AssmeblyDaemonApp
    test_secret_string: !secret a_secret_string
    test_secret_int: !secret a_secret_int
    test_normal_string: not a secret string
    test_normal_int: 0
";
            // ACT
            var instances = new YamlAppConfig(types, new StringReader(yamlConfig), yamlConfigMock.Object,
                Path.Combine(ConfigFixturePath, "level2", "level3", "any.cs")).Instances;
            var instance = instances.FirstOrDefault() as AssmeblyDaemonApp;
            // ASSERT
            Assert.Equal("this is a secret string", instance?.TestSecretString);
            Assert.Equal(99, instance?.TestSecretInt);
            Assert.Equal(0, instance?.TestNormalInt);
        }

        [Fact]
        public void InstanceAppFromConfigShouldHaveCorrectPropertiesCamelCaseConvert()
        {
            // ARRANGE
            var yamlConfigMock = new Mock<YamlConfig>(Path.Combine(ConfigFixturePath, "level2", "level3"));
            IEnumerable<Type> types = new List<Type>() { typeof(AssmeblyDaemonApp) };
            var yamlConfig = @"
app:
    class: AssmeblyDaemonApp
    string_config: a string
    int_config: 10
    enumerable_config:
        - string 1
        - string 2
";
            // ACT
            var instances = new YamlAppConfig(types, new StringReader(yamlConfig), yamlConfigMock.Object, "").Instances;
            var instance = instances.FirstOrDefault() as AssmeblyDaemonApp;
            // ASSERT
            Assert.Equal("a string", instance?.StringConfig);
            Assert.Equal(10, instance?.IntConfig);
            Assert.Equal(2, instance?.EnumerableConfig.Count());
        }



        [Fact]
        public void InstanceAppFromConfigFilesInFolderShouldReturnCorrectInstances()
        {
            // ARRANGE
            var path = Path.Combine(ConfigFixturePath, "level2");
            var moqDaemon = new Mock<INetDaemon>();
            var moqLogger = new LoggerMock();

            moqDaemon.SetupGet(n => n.Logger).Returns(moqLogger.Logger);
            // ACT
            var codeManager = new CodeManager(path);
            // ASSERT
            Assert.Equal(2, codeManager.InstanceAndInitApplications(moqDaemon.Object).Result.Count());
        }


        [Fact]
        public void InstanceAppMultipleInstancesInConfigShouldReturnCorrectInstances()
        {
            // ARRANGE
            var path = Path.Combine(ConfigFixturePath, "mulitinstance");
            var moqDaemon = new Mock<INetDaemon>();
            var moqLogger = new LoggerMock();

            moqDaemon.SetupGet(n => n.Logger).Returns(moqLogger.Logger);
            // ACT
            var codeManager = new CodeManager(path);
            // ASSERT
            Assert.Equal(2, codeManager.InstanceAndInitApplications(moqDaemon.Object).Result.Count());
        }

        [Fact]
        public async Task InstanceAndInitApplicationWithNullShouldThrowArgumentNullException()
        {
            // ARRANGE
            var codeManager = new CodeManager(ConfigFixturePath);
            // ACT/ASSERT
            await Assert.ThrowsAsync<ArgumentNullException>(() => codeManager.InstanceAndInitApplications(null));

        }

        [Fact]
        public void AttributeServiceCallShouldFindCorrectFunction()
        {
            // ARRANGE
            var app = new AssmeblyDaemonApp();
            var netDaemonMock = new Mock<INetDaemon>();

            // ACT
            app.HandleAttributeInitialization(netDaemonMock.Object);

            // ASSERT
            var expObject = new ExpandoObject();
            dynamic data = expObject;
            data.method = "HandleServiceCall";
            data.@class = "AssmeblyDaemonApp";

            netDaemonMock.Verify(n => n.CallService("netdaemon", "register_service", expObject, false), Times.Once);
            netDaemonMock.Verify(n => n.ListenServiceCall("netdaemon", "AssmeblyDaemonApp_HandleServiceCall", It.IsAny<Func<dynamic?, Task>>()), Times.Once);
        }

        [Theory]
        [InlineData("Some string")] //string
        [InlineData(10)]            //integer
        [InlineData(100.5)]         //float
        public async Task StorageShouldReturnSameValueAsSet(object data)
        {
            // ARRANGE
            var yamlConfigMock = new Mock<YamlConfig>(Path.Combine(ConfigFixturePath, "level2", "level3"));
            IEnumerable<Type> types = new List<Type>() { typeof(AssmeblyDaemonApp) };
            var yamlConfig = @"
        app:
            class: AssmeblyDaemonApp
        ";

            var instance = new YamlAppConfig(types, new StringReader(yamlConfig), yamlConfigMock.Object, "").Instances.FirstOrDefault() as AssmeblyDaemonApp;
            var daemonMock = new Mock<INetDaemon>();
            await instance!.StartUpAsync(daemonMock.Object);
            instance!.Id = "somefake_id";
            await Task.Delay(50); // Delay to get the task up and running

            // ACT
            instance!.Storage.Data = data;

            await Task.Delay(50); // Delay to let the task save state

            // ASSERT
            var expected = new FluentExpandoObject();
            expected["Data"] = data;
            Assert.Equal(data, instance.Storage.Data);
            daemonMock.Verify(n => n.SaveDataAsync<FluentExpandoObject>("assmeblydaemonapp_somefake_id", expected), Times.Once);
        }

        [Fact]
        public async Task StorageShouldRestoreWithCorrectValues()
        {
            // ARRANGE
            var yamlConfigMock = new Mock<YamlConfig>(Path.Combine(ConfigFixturePath, "level2", "level3"));
            IEnumerable<Type> types = new List<Type>() { typeof(AssmeblyDaemonApp) };
            var yamlConfig = @"
        app:
            class: AssmeblyDaemonApp
        ";

            var instance = new YamlAppConfig(types, new StringReader(yamlConfig), yamlConfigMock.Object, "").Instances.FirstOrDefault() as AssmeblyDaemonApp;
            var daemonMock = new Mock<INetDaemon>();

            var storageItem = new FluentExpandoObject();
            storageItem["Data"] = "SomeData";

            daemonMock.Setup(n => n.GetDataAsync<FluentExpandoObject>(It.IsAny<string>())).ReturnsAsync(storageItem);

            await instance!.StartUpAsync(daemonMock.Object);

            // ACT
            await instance.RestoreAppStateAsync();

            // ASSERT
            Assert.Equal("SomeData", instance.Storage.Data);
        }

    }


}