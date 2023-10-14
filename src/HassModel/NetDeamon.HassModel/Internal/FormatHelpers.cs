namespace NetDaemon.HassModel.Internal;

internal class FormatHelpers
{
    public static double? ParseAsDouble(string? value) =>
        double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, NumberFormatInfo.InvariantInfo, out var result) ? result : null;
}
