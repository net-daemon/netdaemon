namespace NetDaemon.HassModel.Entities.Core;
#pragma warning disable CS1591
#pragma warning disable CA1056

[Obsolete("Usage of attribute base classes are deprecated, default meta data is not used to replace it")]
public record MediaPlayerAttributesBase
{
    [JsonPropertyName("app_id")]
    public string? AppId { get; init; }

    [JsonPropertyName("app_name")]
    public string? AppName { get; init; }

    /// <summary>
    /// Type of media player.
    /// </summary>
    [JsonPropertyName("device_class")]
    public string? DeviceClass { get; init; }

    /// <summary>
    /// Entity ids of the entities in the media player group. Null if not a group.
    /// </summary>
    [JsonPropertyName("entity_id")]
    public IReadOnlyList<string>? EntityId { get; init; }

    /// <summary>
    /// URL of the picture for the entity.
    /// </summary>
    [JsonPropertyName("entity_picture")]
    public string? EntityPicture { get; init; }

    [JsonPropertyName("entity_picture_local")]
    public string? EntityPictureLocal { get; init; }

    /// <summary>
    /// Name of the media player as displayed in the UI.
    /// </summary>
    [JsonPropertyName("friendly_name")]
    public string? FriendlyName { get; init; }

    [JsonPropertyName("icon")]
    public string? Icon { get; init; }

    [JsonPropertyName("is_volume_muted")]
    public bool? IsVolumeMuted { get; init; }

    [JsonPropertyName("media_album_name")]
    public string? MediaAlbumName { get; init; }
    
    /// <summary>
    /// URL that represents the current image.
    /// </summary>
    [JsonPropertyName("media_image_url")]
    public string? MediaImageUrl { get; init; }

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
    public object? MediaTrack { get; init; }

    [JsonPropertyName("repeat")]
    public string? Repeat { get; init; }

    [JsonPropertyName("shuffle")]
    public bool? Shuffle { get; init; }
    
    /// <summary>
    /// The current sound mode of the media player
    /// </summary>
    [JsonPropertyName("sound_mode")]
    public string? SoundMode { get; init; }
    
    /// <summary>
    /// Dynamic list of available sound modes (set by platform, empty means sound mode not supported)
    /// </summary>
    [JsonPropertyName("sound_mode_list")]
    public IReadOnlyList<string>? SoundModeList { get; init; }

    /// <summary>
    /// The currently selected input source for the media player.
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; init; }

    /// <summary>
    /// The list of possible input sources for the media player. (This list should contain human readable names, suitable for frontend display)
    /// </summary>
    [JsonPropertyName("source_list")]
    public IReadOnlyList<string>? SourceList { get; init; }

    [JsonPropertyName("supported_features")]
    public double? SupportedFeatures { get; init; }

    /// <summary>
    /// Float for volume level between 0..1
    /// </summary>
    [JsonPropertyName("volume_level")]
    public double? VolumeLevel { get; init; }
}