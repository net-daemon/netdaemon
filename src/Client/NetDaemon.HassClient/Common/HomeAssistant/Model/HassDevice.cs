namespace NetDaemon.Client.HomeAssistant.Model;

public record HassDevice
{
    [JsonPropertyName("manufacturer")] public string? Manufacturer { get; init; }

    [JsonPropertyName("model")]
    [JsonConverter(typeof(EnsureStringConverter))]
    public string? Model { get; init; }

    [JsonPropertyName("id")] public string? Id { get; init; }

    [JsonPropertyName("area_id")] public string? AreaId { get; init; }
    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("name")] public string? Name { get; init; }

    [JsonPropertyName("name_by_user")] public string? NameByUser { get; init; }

    [JsonPropertyName("labels")] public IReadOnlyList<string> Labels { get; init; } = [];

    #pragma warning disable CA1056 // It's ok for this URL to be a string
    [JsonPropertyName("configuration_url")] public string? ConfigurationUrl { get; init; }
    #pragma warning restore CA1056
    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("hw_version")] public string? HardwareVersion { get; init; }
    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("sw_version")] public string? SoftwareVersion { get; init; }
    [JsonConverter(typeof(EnsureStringConverter))]
    [JsonPropertyName("serial_number")] public string? SerialNumber { get; init; }

    [JsonConverter(typeof(EnsureArrayOfArrayOfStringConverter))]
    [JsonPropertyName("identifiers")] public IReadOnlyList<IReadOnlyList<string>> Identifiers { get; init; } = [];
}
