using System.Text.Json.Serialization;

namespace NetDaemon.HassModel.CodeGenerator.Model;

internal record HassService
{
    public string Service { get; init; } = ""; // cannot be required because the JsonSerializer will complain
    public string? Description { get; init; }

    [JsonIgnore]
    public IReadOnlyCollection<HassServiceField>? Fields { get; init; }
    public TargetSelector? Target { get; init; }
    public Response? Response { get; init; }
}
