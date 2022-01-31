namespace NetDaemon.Client.HomeAssistant.Model;

public record HassConfig
{
    [JsonPropertyName("components")] public IReadOnlyCollection<string>? Components { get; init; }

    [JsonPropertyName("config_dir")] public string? ConfigDir { get; init; }

    [JsonPropertyName("elevation")] public int? Elevation { get; init; }

    [JsonPropertyName("latitude")] public float? Latitude { get; init; }

    [JsonPropertyName("location_name")] public string? LocationName { get; init; }

    [JsonPropertyName("longitude")] public float? Longitude { get; init; }

    [JsonPropertyName("time_zone")] public string? TimeZone { get; init; }

    [JsonPropertyName("unit_system")] public HassUnitSystem? UnitSystem { get; init; }

    [JsonPropertyName("version")] public string? Version { get; init; }

    [JsonPropertyName("state")] public string? State { get; init; }

    [JsonPropertyName("whitelist_external_dirs")]
    public IReadOnlyCollection<string>? WhitelistExternalDirs { get; init; }
}