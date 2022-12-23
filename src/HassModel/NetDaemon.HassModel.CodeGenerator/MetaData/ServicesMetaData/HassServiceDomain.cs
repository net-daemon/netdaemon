namespace NetDaemon.HassModel.CodeGenerator.Model;

internal record HassServiceDomain
{
    public required string Domain { get; init; }
    public required IReadOnlyCollection<HassService> Services { get; init; }
}