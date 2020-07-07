using System;
using System.Globalization;

namespace NetDaemon.Daemon
{
    internal static class StringParser
    {
        public static object ParseDataType(string state)
        {
            if (Int64.TryParse(state, NumberStyles.Number, CultureInfo.InvariantCulture, out Int64 intValue))
                return intValue;

            if (Double.TryParse(state, NumberStyles.Number, CultureInfo.InvariantCulture, out Double doubleValue))
                return doubleValue;

            return state;
        }
    }
}