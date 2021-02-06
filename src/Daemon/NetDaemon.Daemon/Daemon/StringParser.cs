using System.Globalization;

namespace NetDaemon.Daemon
{
    internal static class StringParser
    {
        public static object? ParseDataType(string? state)
        {
            if (long.TryParse(state, NumberStyles.Number, CultureInfo.InvariantCulture, out long intValue))
                return intValue;

            if (double.TryParse(state, NumberStyles.Number, CultureInfo.InvariantCulture, out double doubleValue))
                return doubleValue;

            return state;
        }
    }
}