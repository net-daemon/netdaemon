using System.Reflection;

namespace NetDaemon.HassModel.CodeGenerator.Helpers;

#pragma warning disable CA1303

/// <summary>
/// Returns current version of NetDaemon
/// </summary>
public static class VersionHelper
{
    public static string GeneratorVersion { get; } =
        Assembly.GetAssembly(typeof(Generator))!.GetName().Version!.ToString();

    public static void PrintVersion()
    {
        Console.Write("Codegen version: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(GeneratorVersion);
        Console.ResetColor();
    }
}