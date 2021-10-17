using System;
using System.Collections.Generic;
using System.Linq;

namespace NetDaemon.HassModel.CodeGenerator.Helpers
{
    internal static class TypeHelper
    {
        public static Type GetType(object? obj)
        {
            var defaultType = typeof(string);

            if (obj is null)
            {
                return typeof(object);
            }

            if (TryGetValueType(obj, out var valueType))
            {
                return valueType;
            }

            switch (obj)
            {
                case string:
                    return defaultType;
                case IList<object> list:
                {
                    var listItem = list.FirstOrDefault();

                    Type genericType = listItem is not null
                        ? GetType(listItem)
                        : defaultType;

                    return typeof(List<>).MakeGenericType(genericType);
                }
                case Dictionary<string, object> dictionary:
                {
                    var dictionaryValue = dictionary.Values.FirstOrDefault();

                    Type genericType = dictionaryValue is not null
                        ? GetType(dictionaryValue)
                        : defaultType;

                    return typeof(Dictionary<,>).MakeGenericType(typeof(string), genericType);
                }
                default: return defaultType;
            }
        }

        private static bool TryGetValueType(object obj, out Type valueType)
        {
            valueType = null!;

            Type? nullableType = null;

            var objType = obj.GetType();
            if (objType.IsValueType)
            {
                nullableType = objType;
            }
            else if (obj is string str)
            {
                nullableType = str switch
                {
                    var s when long.TryParse(s, out _) => typeof(long),
                    var s when double.TryParse(s, out _) => typeof(double),
                    var s when bool.TryParse(s, out _) => typeof(bool),
                    var s when DateTime.TryParse(s, out _) => typeof(DateTime),
                    _ => null!
                };
            }

            if (nullableType is not null)
            {
                valueType = nullableType;
            }

            return nullableType is not null;
        }
    }
}