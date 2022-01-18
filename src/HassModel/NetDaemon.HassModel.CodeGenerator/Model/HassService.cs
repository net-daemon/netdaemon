namespace NetDaemon.HassModel.CodeGenerator.Model;

internal record HassService
{
    public string? Service { get; init; }
    public string? Description { get; init; }
    public IReadOnlyCollection<HassServiceField>? Fields { get; init; }
    public TargetSelector? Target { get; init; }
}