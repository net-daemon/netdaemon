using System.Text.Json;
using System.Text.Json.Serialization;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel.Internal;

namespace NetDaemon.HassModel.Tests.Internal;

public class HassObjectMapperTest
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
    public void TestConversionWithIdentifiers()
    {
        const string jsonDevice = """
        {
            "config_entries": [],
            "connections": [],
            "manufacturer": "Google Inc.",
            "model": "Chromecast",
            "name": "My TV",
            "sw_version": null,
            "id": "42cdda32a2a3428e86c2e27699d79ead",
            "via_device_id": null,
            "area_id": null,
            "name_by_user": null,
            "identifiers": [
                [
                    "Google",
                    "42cdda32a2a3428e86c2e27699d79ead"
                ],
                [
                    "SKIP"
                ]
            ]
        }
        """;
        var hassDevice = JsonSerializer.Deserialize<HassDevice>(jsonDevice, _defaultSerializerOptions);
        Assert.Equal(hassDevice!.Identifiers[0][0], "Google");
        Assert.Equal(hassDevice.Identifiers[0][1], "42cdda32a2a3428e86c2e27699d79ead");
        Assert.Equal(hassDevice!.Identifiers[1][0], "SKIP");

        var ndDevice = hassDevice.Map(Mock.Of<IHaRegistryNavigator>());

        Assert.Single(ndDevice.Identifiers!);
        Assert.Equal(ndDevice.Identifiers![0].Item1, "Google");
        Assert.Equal(ndDevice.Identifiers![0].Item2, "42cdda32a2a3428e86c2e27699d79ead");
    }
}
