﻿namespace NetDaemon.Client.Common.HomeAssistant.Model;

public record HassVariable
{
    [JsonPropertyName("trigger")] public JsonElement? TriggerElement { get; init; }
}