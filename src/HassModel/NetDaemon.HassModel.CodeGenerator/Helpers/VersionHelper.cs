using System.Reflection;

namespace NetDaemon.HassModel.CodeGenerator.Helpers;

/// <summary>
/// Returns current version of NetDaemon
/// </summary>
public class VersionHelper
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