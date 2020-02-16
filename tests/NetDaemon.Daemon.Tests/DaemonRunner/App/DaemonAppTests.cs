using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.App;
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
            var codeManager = new CodeManager(Path.Combine(ConfigFixturePath));
            // ACT
            // ASSERT
            Assert.Equal(3, codeManager.DaemonAppTypes.Select(n => n.Name.StartsWith("Lev")).Count());

        }

    }
}