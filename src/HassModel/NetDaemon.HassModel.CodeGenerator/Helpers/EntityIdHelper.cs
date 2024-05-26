namespace NetDaemon.HassModel.CodeGenerator.Helpers;

internal static class EntityIdHelper
{
    public static readonly string[] NumericDomains = ["input_number", "number", "proximity"];
    public static readonly string[] MixedDomains = ["sensor"];

    public static string GetDomain(string str)
    {
        return str[..str.IndexOf('.', StringComparison.InvariantCultureIgnoreCase)];
    }

    public static string GetEntity(string str)
    {
        return str[(str.IndexOf('.', StringComparison.InvariantCultureIgnoreCase) + 1)..];
    }
}
