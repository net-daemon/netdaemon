﻿namespace NetDaemon.Client.Common.HomeAssistant.Model;

public record HassDevice
{
    [JsonPropertyName("manufacturer")]
    public string? Manufacturer { get; init; }

    [JsonPropertyName("model")]
    [JsonConverter(typeof(HassDeviceModelConverter))]
    public string? Model { get; init; }

    [JsonPropertyName("id")]
    public string? Id { get; init; }

    [JsonPropertyName("area_id")]
    public string? AreaId { get; init; }

    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonPropertyName("name_by_user")]
    public string? NameByUser { get; init; }
}
