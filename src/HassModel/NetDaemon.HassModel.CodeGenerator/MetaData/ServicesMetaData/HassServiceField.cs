using System.Text.Json.Serialization;

namespace NetDaemon.HassModel.CodeGenerator.Model;

internal record HassServiceField
{
    public string Field { get; init; } = "";  // cannot be required because the JsonSerializer will complain
    public string? Description { get; init; }
    public bool? Required { get; init; }
    public object? Example { get; init; }
    
    [JsonConverter(typeof(SelectorConverter))]
    public Selector? Selector { get; init; }
}