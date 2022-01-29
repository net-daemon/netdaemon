namespace NetDaemon.HassModel.CodeGenerator.Model;

internal record HassServiceField
{
    public string? Field { get; init; }
    public string? Description { get; init; }
    public bool? Required { get; init; }
    public object? Example { get; init; }
    public object? Selector { get; init; }
}
