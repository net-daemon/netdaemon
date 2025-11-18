namespace NetDaemon.Client.HomeAssistant.Model;

public record HassDevice
{
    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("manufacturer")]
    public string? Manufacturer { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("model")]
    public string? Model { get; init; }

    [JsonPropertyName("id")]
    [JsonConverter(typeof(EnsureStringConverter))]
    public string? Id { get; init; }

    [JsonPropertyName("area_id")]
    [JsonConverter(typeof(EnsureStringConverter))]
    public string? AreaId { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("name_by_user")]
    public string? NameByUser { get; init; }

    [JsonConverter(typeof(EnsureArrayOfStringConverter))]
    [JsonPropertyName("labels")]
    public IReadOnlyList<string> Labels { get; init; } = [];

#pragma warning disable CA1056 // It's ok for this URL to be a string
    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("configuration_url")]
    public string? ConfigurationUrl { get; init; }
#pragma warning restore CA1056

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("hw_version")]
    public string? HardwareVersion { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("sw_version")]
    public string? SoftwareVersion { get; init; }

    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("serial_number")]
    public string? SerialNumber { get; init; }

    [JsonConverter(typeof(EnsureArrayOfArrayOfStringConverter))]
    [JsonPropertyName("identifiers")]
    public IReadOnlyList<IReadOnlyList<string>> Identifiers { get; init; } = [];
}
