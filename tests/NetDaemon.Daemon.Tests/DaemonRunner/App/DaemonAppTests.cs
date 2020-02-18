using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JoySoftware.HomeAssistant.NetDaemon.Common;
using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.App;
using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.Config;
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
            Assert.Equal(1, codeManager.DaemonAppTypes.Select(n => n.Name == "LevOneApp").Count());

        }

        [Fact]
        public void ApplicationAllShouldBePresentInCsFile()
        {
            // ARRANGE
            var codeManager = new CodeManager(ConfigFixturePath);
            // ACT
            // ASSERT
            Assert.Equal(3, codeManager.DaemonAppTypes.Select(n => n.Name.StartsWith("Lev")).Count());

        }

        [Fact]
        public void InstanceAppFromConfigShouldReturnCorrectType()
        {
            // ARRANGE
            IEnumerable<Type> types = new List<Type>() { typeof(AssmeblyDaemonApp) };
            var yamlConfig = "app:\n\tclass: AssmeblyDaemonApp";
            // ACT
            var instance = types.InstanceFromYamlConfig(new StringReader(yamlConfig)) as INetDaemonApp;
            // ASSERT
            Assert.NotNull(instance);
        }

        [Fact]
        public void InstanceAppFromConfigNotFoundShouldReturnNull()
        {
            // ARRANGE
            IEnumerable<Type> types = new List<Type>() { typeof(AssmeblyDaemonApp) };
            var yamlConfig = "app:\n\tclass: NotFoundApp";
            // ACT
            var instance = types.InstanceFromYamlConfig(new StringReader(yamlConfig)) as INetDaemonApp;
            // ASSERT
            Assert.Null(instance);
        }

        [Fact]
        public void InstanceAppFromConfigShouldHaveCorrectProperties()
        {
            // ARRANGE
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
            var instance = types.InstanceFromYamlConfig(new StringReader(yamlConfig)) as AssmeblyDaemonApp;
            // ASSERT
            Assert.Equal("a string", instance?.StringConfig);
            Assert.Equal(10, instance?.IntConfig);
            Assert.Equal(2, instance?.EnumerableConfig.Count());
        }

        [Fact]
        public void InstanceAppFromConfigShouldHaveCorrectPropertiesCamelCaseConvert()
        {
            // ARRANGE
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
            var instance = types.InstanceFromYamlConfig(new StringReader(yamlConfig)) as AssmeblyDaemonApp;
            // ASSERT
            Assert.Equal("a string", instance?.StringConfig);
            Assert.Equal(10, instance?.IntConfig);
            Assert.Equal(2, instance?.EnumerableConfig.Count());
        }



        [Fact]
        public void InstanceAppFromConfigFilesInFolderShouldReturnCorrectInstances()
        {
            // ARRANGE
            var path = Path.Combine(AppContext.BaseDirectory, "DaemonRunner", "App");
            IEnumerable<Type> types = new List<Type>() { typeof(AssmeblyDaemonApp) };
            // ACT
            var instance = YamlConfig.GetInstancesFromYaml(path, types);
            // ASSERT
            Assert.Equal(1, instance.Count());
        }
    }

    
}