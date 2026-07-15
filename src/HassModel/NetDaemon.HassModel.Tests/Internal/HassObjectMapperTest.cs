using System.Text.Json;
using System.Text.Json.Serialization;
using NetDaemon.Client.HomeAssistant.Model;
using NetDaemon.Client.Internal.Json;
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
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
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
                ],
                [1,2],
                [1.1, 2],
                ["1", 2.2]
            ]
        }
        """;
        var hassDevice = JsonSerializer.Deserialize<HassDevice>(jsonDevice, _defaultSerializerOptions);
        Assert.Equal(hassDevice!.Identifiers[0][0], "Google");
        Assert.Equal(hassDevice.Identifiers[0][1], "42cdda32a2a3428e86c2e27699d79ead");
        Assert.Equal(hassDevice!.Identifiers[1][0], "SKIP");

        var ndDevice = hassDevice.Map(Mock.Of<IHaRegistryNavigator>());

        Assert.Equal(4, ndDevice.Identifiers!.Count);
        Assert.Equal(ndDevice.Identifiers![0].Item1, "Google");
        Assert.Equal(ndDevice.Identifiers![0].Item2, "42cdda32a2a3428e86c2e27699d79ead");

        Assert.Equal(ndDevice.Identifiers![1].Item1, "1");
        Assert.Equal(ndDevice.Identifiers![1].Item2, "2");

        Assert.Equal(ndDevice.Identifiers![2].Item1, "1.1");
        Assert.Equal(ndDevice.Identifiers![2].Item2, "2");

        Assert.Equal(ndDevice.Identifiers![3].Item1, "1");
        Assert.Equal(ndDevice.Identifiers![3].Item2, "2.2");
    }

    [Fact]
    public void DeserializeEventWithContext()
    {
        const string eventJson = """
                                 {
                                   "data":{
                                      "entity_id":"light.bed_light",
                                      "new_state":{
                                         "entity_id":"light.bed_light",
                                         "last_changed":"2016-11-26T01:37:24.265390+00:00",
                                         "state":"on",
                                         "attributes":{
                                            "rgb_color":[
                                               254,
                                               208,
                                               0
                                            ],
                                            "color_temp":380,
                                            "supported_features":147,
                                            "xy_color":[
                                               0.5,
                                               0.5
                                            ],
                                            "brightness":180,
                                            "white_value":200,
                                            "friendly_name":"Bed Light"
                                         },
                                         "last_updated":"2016-11-26T01:37:24.265390+00:00",
                                         "context": {
                                            "id": "326ef27d19415c60c492fe330945f954",
                                            "parent_id": null,
                                            "user_id": "31ddb597e03147118cf8d2f8fbea5553"
                                         }
                                      },
                                      "old_state":{
                                         "entity_id":"light.bed_light",
                                         "last_changed":"2016-11-26T01:37:10.466994+00:00",
                                         "state":"off",
                                         "attributes":{
                                            "supported_features":147,
                                            "friendly_name":"Bed Light"
                                         },
                                         "last_updated":"2016-11-26T01:37:10.466994+00:00",
                                         "context": {
                                            "id": "e4af5b117137425e97658041a0538441",
                                            "parent_id": null,
                                            "user_id": "31ddb597e03147118cf8d2f8fbea5553"
                                         }
                                      }
                                   },
                                   "event_type":"state_changed",
                                   "time_fired":"2016-11-26T01:37:24.265429+00:00",
                                   "origin":"LOCAL",
                                   "context": {
                                      "id": "326ef27d19415c60c492fe330945f954",
                                      "parent_id": null,
                                      "user_id": "31ddb597e03147118cf8d2f8fbea5553"
                                   }
                                 }
                                 """;

        var hassEvent = JsonSerializer.Deserialize<HassEvent>(eventJson, _defaultSerializerOptions);
        Assert.NotNull(hassEvent);

        var mappedEvent = hassEvent.Map();

        Assert.NotNull(mappedEvent);
        Assert.NotNull(mappedEvent.Context);
        Assert.Equal("326ef27d19415c60c492fe330945f954", mappedEvent.Context!.Id);
        Assert.Null(mappedEvent.Context.ParentId);
        Assert.Equal("31ddb597e03147118cf8d2f8fbea5553", mappedEvent.Context.UserId);
    }

}
