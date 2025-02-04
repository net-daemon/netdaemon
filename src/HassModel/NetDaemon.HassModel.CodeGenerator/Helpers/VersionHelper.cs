﻿using System.Reflection;

namespace NetDaemon.HassModel.CodeGenerator.Helpers;

#pragma warning disable CA1303

/// <summary>
/// Helper class for managing NetDaemon version tasks
/// </summary>
public static class VersionHelper
{
    /// <summary>
    /// Returns current version of NetDaemon
    /// </summary>
    public static Version GeneratorVersion { get; } =
        Assembly.GetAssembly(typeof(Generator))!.GetName().Version!;

    /// <summary>
    /// Pretty prints version information to console
    /// </summary>
    public static void PrintVersion()
    {
        Console.Write("nd-codegen version: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(GeneratorVersion.ToString(3));
        Console.ResetColor();
    }
}
