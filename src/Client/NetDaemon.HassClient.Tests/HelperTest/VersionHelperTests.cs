namespace NetDaemon.HassClient.Tests.HelperTest;

public class VersionHelperTests
{
    [Theory]
    [InlineData("2022.8.12", "2022.8.12")]
    [InlineData("2022.8.0b7", "2022.8.0")]
    [InlineData("2022.9.0", "2022.9.0")]
    [InlineData("2022.9.0b1", "2022.9.0")]
    public void WithoutBeta_ValidInput_ReturnsExpectedVersion(string input, string expectedOutput)
    {
        // Arrange
        var expectedVersion = new Version(expectedOutput);

        // Act
        var parsedVersion = VersionHelper.ReplaceBeta(input);

        // Assert
        Assert.Equal(expectedVersion, parsedVersion);
    }
}