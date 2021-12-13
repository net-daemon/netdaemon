namespace NetDaemon.HassClient.Tests.Json;

public class JsonConverterTests
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
    public void TestConvertionWithNumericValue()
    {
        const string jsonDevice = @"
        {
            ""config_entries"": [],
            ""connections"": [],
            ""manufacturer"": ""Google Inc."",
            ""model"": 123123,
            ""name"": ""My TV"",
            ""sw_version"": null,
            ""id"": ""42cdda32a2a3428e86c2e27699d79ead"",
            ""via_device_id"": null,
            ""area_id"": null,
            ""name_by_user"": null
        }
        ";
        var hassDevice = JsonSerializer.Deserialize<HassDevice>(jsonDevice, _defaultSerializerOptions);
        hassDevice!.Model.Should().Be("123123");
    }

    [Fact]
    public void TestConvertionWithStringValue()
    {
        const string jsonDevice = @"
        {
            ""config_entries"": [],
            ""connections"": [],
            ""manufacturer"": ""Google Inc."",
            ""model"": ""Chromecast"",
            ""name"": ""My TV"",
            ""sw_version"": null,
            ""id"": ""42cdda32a2a3428e86c2e27699d79ead"",
            ""via_device_id"": null,
            ""area_id"": null,
            ""name_by_user"": null
        }
        ";
        var hassDevice = JsonSerializer.Deserialize<HassDevice>(jsonDevice, _defaultSerializerOptions);
        hassDevice!.Model.Should().Be("Chromecast");
    }
}