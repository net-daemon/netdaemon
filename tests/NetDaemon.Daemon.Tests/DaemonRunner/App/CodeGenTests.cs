using System.Linq;
using NetDaemon.Service.App;
using Xunit;

namespace NetDaemon.Daemon.Tests.DaemonRunner.App
{
    public class CodeGenerationTests
    {
        [Fact]
        public void TestGenerateCode()
        {
            // ARRANGE
            // ACT
            var code = CodeGenerator.GenerateCode("Netdaemon.Generated.Extensions", new string[] { "light.koket_fonster", "media_player.my_player" });

            // ASSERT

            Assert.Equal(System.IO.File.ReadAllText("DaemonRunner/CodeGen/CodeGenTestFixture.cs"), code);
            // System.IO.File.WriteAllText("/root/src/tests/NetDaemon.Daemon.Tests/DaemonRunner/CodeGen/CodeGenTestFixture.cs", code);
        }

        [Fact]
        public void WhenGivenAnArrayOfEntitiesTheDomainShouldReturnCorrectDomains()
        {
            // ARRANGE
            var entities = new string[]
            {
                "light.the_light",
                "light.kitchen",
                "media_player.player",
                "scene.thescene",
                "switch.myswitch",
                "switch.myswitch2",
                "camera.acamera",
                "automation.wowautomation",
                "script.myscript"
            };
            // ACT
            var domainsInCamelCase = CodeGenerator.GetDomainsFromEntities(entities);
            // ASSERT
            Assert.Equal(7, domainsInCamelCase.Count());
            Assert.Collection(domainsInCamelCase,
            n => Assert.Equal("light", n),
            n => Assert.Equal("media_player", n),
            n => Assert.Equal("scene", n),
            n => Assert.Equal("switch", n),
            n => Assert.Equal("camera", n),
            n => Assert.Equal("automation", n),
            n => Assert.Equal("script", n));
        }
    }
}