namespace NetDaemon.HassModel.Tests.CodeGenerator;

/// <summary>
/// Marker attribute for classes from dynamically compiled code in tests that should be created
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class NetDaemonTestAppAttribute : Attribute;
