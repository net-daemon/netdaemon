//------------------------------------------------------------------------------
// <non-auto-generated>
// Generated using NetDaemon CodeGenerator nd-codegen v23.32.0.0
//   At: 2023-08-13T22:55:56.3430066+02:00
//
// *** Make sure the version of the codegen tool and your nugets Joysoftware.NetDaemon.* have the same version.***
// You can use following command to keep it up to date with the latest version:
//   dotnet tool update JoySoftware.NetDaemon.HassModel.CodeGen
//
// To update this file with latest entities run this command in your project directory:
//   dotnet tool run nd-codegen
//
// In the template projects we provided a convenience powershell script that will update
// the codegen and nugets to latest versions update_all_dependencies.ps1.
//
// For more information: https://netdaemon.xyz/docs/v3/hass_model/hass_model_codegen
// For more information about NetDaemon: https://netdaemon.xyz/
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetDaemon.HassModel.Entities.Core;

namespace HomeAssistantGenerated;

public partial class Entities
{
    private readonly IHaContext _haContext;
    public Entities(IHaContext haContext)
    {
        _haContext = haContext;
    }

    public LightEntities Light => new(_haContext);
}


public partial class LightEntities
{
    private readonly IHaContext _haContext;
    public LightEntities(IHaContext haContext)
    {
        _haContext = haContext;
    }

    ///<summary>Woonkamer</summary>
    public LightEntity Woonkamer => new(_haContext, "light.woonkamer");
    ///<summary>Alles Beneden</summary>
    ///<summary>Zolder</summary>
    public LightEntity Zolder => new(_haContext, "light.zolder");
}

public partial record LightEntity : Entity<LightEntity, EntityState<LightAttributes>, LightAttributes>, ILightEntityCore
{
    public LightEntity(IHaContext haContext, string entityId) : base(haContext, entityId)
    {
    }

    public LightEntity(Entity entity) : base(entity)
    {
    }
}

public partial record LightAttributes
{
    [JsonPropertyName("min_color_temp_kelvin")]
    public double? MinColorTempKelvin { get; init; }

    [JsonPropertyName("max_color_temp_kelvin")]
    public double? MaxColorTempKelvin { get; init; }

    [JsonPropertyName("color_temp_kelvin")]
    public double? ColorTempKelvin { get; init; }

    [JsonPropertyName("off_with_transition")]
    public bool? OffWithTransition { get; init; }

    [JsonPropertyName("off_brightness")]
    public double? OffBrightness { get; init; }

    [JsonPropertyName("restored")]
    public bool? Restored { get; init; }

    [JsonPropertyName("supported_color_modes")]
    public IReadOnlyList<string>? SupportedColorModes { get; init; }

    [JsonPropertyName("friendly_name")]
    public string? FriendlyName { get; init; }

    [JsonPropertyName("supported_features")]
    public double? SupportedFeatures { get; init; }

    [JsonPropertyName("min_mireds")]
    public double? MinMireds { get; init; }

    [JsonPropertyName("max_mireds")]
    public double? MaxMireds { get; init; }

    [JsonPropertyName("effect_list")]
    public IReadOnlyList<string>? EffectList { get; init; }

    [JsonPropertyName("entity_id")]
    public IReadOnlyList<string>? EntityId { get; init; }

    [JsonPropertyName("icon")]
    public string? Icon { get; init; }

    [JsonPropertyName("color_mode")]
    public string? ColorMode { get; init; }

    [JsonPropertyName("brightness")]
    public double? Brightness { get; init; }

    [JsonPropertyName("color_temp")]
    public double? ColorTemp { get; init; }

    [JsonPropertyName("hs_color")]
    public IReadOnlyList<double>? HsColor { get; init; }

    [JsonPropertyName("rgb_color")]
    public IReadOnlyList<double>? RgbColor { get; init; }

    [JsonPropertyName("xy_color")]
    public IReadOnlyList<double>? XyColor { get; init; }
}






