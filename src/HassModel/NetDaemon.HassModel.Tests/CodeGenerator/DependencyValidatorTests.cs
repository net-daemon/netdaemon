using System.Runtime.CompilerServices;
using Microsoft.Build.Locator;
using NetDaemon.HassModel.CodeGenerator;

namespace NetDaemon.HassModel.Tests.CodeGenerator;

public class DependencyValidatorTests
{
    [ModuleInitializer]
    internal static void Init()
    {
        MSBuildLocator.RegisterDefaults();
    }

    // Test that pre-release versions are trimmed using the TrimPreReleaseVersion method using theory
    [Theory]
    [InlineData("1.0.0-alpha", "1.0.0")]
    [InlineData("24.17.0-alpha-2", "24.17.0")]
    [InlineData("24.17.0", "24.17.0")]
    public void TrimPreReleaseVersionShouldReturnCorrectVersion(string version, string expected)
    {
        // ARRANGE

        // ACT
        var result = VersionValidator.TrimPreReleaseVersion(version);

        // ASSERT
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(@"CodeGenerator\TestFiles\RawVersions\RawVersions.csproj")]
    [InlineData(@"CodeGenerator\TestFiles\SubstitutedVersions\SubstitutedVersions.csproj")]
    public void ValidateProjectFileParsed(string projectFile)
    {
        VersionValidator.GetPackageReferences(projectFile)
            .Should().BeEquivalentTo([
                ("NetDaemon.AppModel", "25.6.0"),
                ("NetDaemon.Runtime", "25.6.0"),
                ("NetDaemon.HassModel", "25.6.0"),
                ("NetDaemon.Client", "25.6.0"),
                ("NetDaemon.Extensions.Scheduling", "25.6.0"),
                ("NetDaemon.Extensions.Logging", "25.6.0"),
                ("NetDaemon.Extensions.Tts", "25.6.0")
            ]);
    }
}
