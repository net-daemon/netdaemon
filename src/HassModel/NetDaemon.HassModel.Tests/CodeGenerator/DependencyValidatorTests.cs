using NetDaemon.HassModel.CodeGenerator;

namespace NetDaemon.HassModel.Tests.CodeGenerator;

public class DependencyValidatorTests
{
    // Test that pre-release versions are trimmed using the TrimPreReleaseVersion method using theory
    [Theory]
    [InlineData("1.0.0-alpha", "1.0.0")]
    [InlineData("24.17.0-alpha-2", "24.17.0")]
    [InlineData("24.17.0", "24.17.0")]
    public void TrimPreReleaseVersionShouldReturnCorrectVersion(string version, string expected)
    {
        // ARRANGE

        // ACT
        var result = DependencyValidator.TrimPreReleaseVersion(version);

        // ASSERT
        result.Should().Be(expected);
    }
}
