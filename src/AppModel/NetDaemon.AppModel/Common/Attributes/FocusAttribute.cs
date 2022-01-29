namespace NetDaemon.AppModel;

/// <summary>
///     Marks a class to be loaded exclusively while in development environment
/// </summary>
/// <remarks>
///     If one or more app classes have this attribute, only those apps will be loaded
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public sealed class FocusAttribute : Attribute
{
}
