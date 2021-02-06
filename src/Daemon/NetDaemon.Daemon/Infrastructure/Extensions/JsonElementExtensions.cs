using System.Collections.Generic;
using System.Text.Json;
using NetDaemon.Daemon;

namespace NetDaemon.Infrastructure.Extensions
{
    internal static class JsonElementExtensions
    {
        public static object? ConvertToDynamicValue(this JsonElement elem)
        {
            switch (elem.ValueKind)
            {
                case JsonValueKind.String:
                    return StringParser.ParseDataType(elem.GetString());

                case JsonValueKind.False:
                    return false;

                case JsonValueKind.True:
                    return true;

                case JsonValueKind.Number:
                    long retVal;
                    if (elem.TryGetInt64(out retVal))
                    {
                        return retVal;
                    }
                    return elem.GetDouble();

                case JsonValueKind.Array:
                    var list = new List<object?>();
                    foreach (var val in elem.EnumerateArray())
                    {
                        list.Add(val.ConvertToDynamicValue());
                    }
                    return list;

                case JsonValueKind.Object:
                    var obj = new Dictionary<string, object?>();

                    foreach (var prop in elem.EnumerateObject())
                    {
                        obj[prop.Name] = prop.Value.ConvertToDynamicValue();
                    }
                    return obj;
            }

            return null;
        }
    }
}