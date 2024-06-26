
namespace NetDaemon.HassClient.Tests.Json;

public class EnsureStringConverterTests
{
    /// <summary>
    ///     Default Json serialization options, Hass expects intended
    /// </summary>
    private readonly JsonSerializerOptions _defaultSerializerOptions = new()
    {
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [Fact]
    public void TestConvertAValidString()
    {
        const string jsonDevice = @"
        {
            ""config_entries"": [],
            ""connections"": [],
            ""manufacturer"": ""Google Inc."",
            ""model"": 123123,
            ""name"": 123,
            ""serial_number"": ""1.0"",
            ""sw_version"": ""100"",
            ""hw_version"": null,
            ""id"": ""42cdda32a2a3428e86c2e27699d79ead"",
            ""via_device_id"": null,
            ""area_id"": null,
            ""name_by_user"": null
        }
        ";
        var hassDevice = JsonSerializer.Deserialize<HassDevice>(jsonDevice, _defaultSerializerOptions);
        hassDevice!.SerialNumber.Should().Be("1.0");
        hassDevice!.SoftwareVersion.Should().Be("100");
        hassDevice!.HardwareVersion.Should().BeNull();
    }

    [Fact]
    public void TestConverAInvalidString()
    {
        const string jsonDevice = @"
        {
            ""config_entries"": [],
            ""connections"": [],
            ""model"": ""Chromecast"",
            ""serial_number"": [""1.0"", ""2.0""],
            ""name"": ""My TV"",
            ""sw_version"": { ""attribute"": ""1.0"" },
            ""manufacturer"": ""Google Inc."",
            ""hw_version"": 100,
            ""id"": ""42cdda32a2a3428e86c2e27699d79ead"",
            ""via_device_id"": null,
            ""area_id"": null,
            ""name_by_user"": null
        }
        ";
        var hassDevice = JsonSerializer.Deserialize<HassDevice>(jsonDevice, _defaultSerializerOptions);
        hassDevice!.SerialNumber.Should().BeNull();
        hassDevice!.SoftwareVersion.Should().BeNull();
        hassDevice!.HardwareVersion.Should().BeNull();

        // Make sure it is skipped correctly by checking the next property is read
        hassDevice!.Manufacturer.Should().Be("Google Inc.");
        hassDevice!.Id.Should().Be("42cdda32a2a3428e86c2e27699d79ead");
        hassDevice!.Name.Should().Be("My TV");
    }
}
