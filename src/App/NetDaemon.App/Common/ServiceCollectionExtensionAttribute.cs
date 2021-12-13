using System;

namespace NetDaemon.Common;

/// <summary>
/// Indicates method in dynamically compile code should be called as a ServiceCollectionExtension to setup services 
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ServiceCollectionExtensionAttribute : Attribute
{}