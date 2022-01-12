namespace NetDaemon.HassModel.CodeGenerator.Model;

internal record HassServiceDomain
{
    public string? Domain { get; init; }
    public IReadOnlyCollection<HassService>? Services { get; init; }
}