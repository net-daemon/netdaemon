namespace NetDaemon.HassModel.Entities.Core;
#pragma warning disable CS1591

public record MediaPlayerAttributesBase
{
    [JsonPropertyName("app_id")]
    public string? AppId { get; init; }

    [JsonPropertyName("app_name")]
    public string? AppName { get; init; }

    [JsonPropertyName("device_class")]
    public string? DeviceClass { get; init; }

    [JsonPropertyName("entity_id")]
    public object? EntityId { get; init; }

    [JsonPropertyName("entity_picture")]
    public string? EntityPicture { get; init; }

    [JsonPropertyName("entity_picture_local")]
    public string? EntityPictureLocal { get; init; }

    [JsonPropertyName("friendly_name")]
    public string? FriendlyName { get; init; }

    [JsonPropertyName("icon")]
    public string? Icon { get; init; }

    [JsonPropertyName("is_volume_muted")]
    public bool? IsVolumeMuted { get; init; }

    [JsonPropertyName("media_album_name")]
    public string? MediaAlbumName { get; init; }

    [JsonPropertyName("media_artist")]
    public string? MediaArtist { get; init; }

    [JsonPropertyName("media_content_id")]
    public string? MediaContentId { get; init; }

    [JsonPropertyName("media_content_type")]
    public string? MediaContentType { get; init; }

    [JsonPropertyName("media_duration")]
    public double? MediaDuration { get; init; }

    [JsonPropertyName("media_position")]
    public double? MediaPosition { get; init; }

    [JsonPropertyName("media_position_updated_at")]
    public string? MediaPositionUpdatedAt { get; init; }

    [JsonPropertyName("media_title")]
    public string? MediaTitle { get; init; }

    [JsonPropertyName("media_track")]
    public double? MediaTrack { get; init; }

    [JsonPropertyName("repeat")]
    public string? Repeat { get; init; }

    [JsonPropertyName("shuffle")]
    public bool? Shuffle { get; init; }

    [JsonPropertyName("source")]
    public string? Source { get; init; }

    [JsonPropertyName("source_list")]
    public IReadOnlyList<string>? SourceList { get; init; }

    [JsonPropertyName("supported_features")]
    public double? SupportedFeatures { get; init; }

    [JsonPropertyName("volume_level")]
    public double? VolumeLevel { get; init; }
}