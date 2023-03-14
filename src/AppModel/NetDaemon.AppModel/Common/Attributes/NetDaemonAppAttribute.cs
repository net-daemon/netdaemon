using JetBrains.Annotations;

namespace NetDaemon.AppModel;

/// <summary>
///     Marks a class as a NetDaemonApp
/// </summary>
[MeansImplicitUse]
[AttributeUsage(AttributeTargets.Class)]
public sealed class NetDaemonAppAttribute : Attribute
{
    /// <summary>
    ///     Id of an app
    /// </summary>
    public string? Id { get; init; }
}