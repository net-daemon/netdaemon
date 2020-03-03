using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using YamlDotNet.RepresentationModel;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace JoySoftware.HomeAssistant.NetDaemon.DaemonRunner.Service.Config
{
    public static class PropertyInfoExtensions
    {
        public static IList? CreateListOfPropertyType(this Type listType)
        {
            Type gen = typeof(List<>).MakeGenericType(listType!);
            object? list = Activator.CreateInstance(gen);

            return list as IList;
        }
    }

    public static class YamlExtensions
    {
        public static PropertyInfo? GetYamlProperty(this Type type, string propertyName)
        {
            var prop = type.GetProperty(propertyName);

            if (prop == null)
            {
                // Lets try convert from python style to CamelCase
                prop = type.GetProperty(propertyName.ToCamelCase());
            }
            return prop;
        }

        public static object? ToObject(this YamlScalarNode node, Type valueType)
        {
            Type? underlyingNullableType = Nullable.GetUnderlyingType(valueType);
            if (underlyingNullableType != null)
            {
                // It is nullable type
                valueType = underlyingNullableType;
            }
            switch (valueType.Name)
            {
                case "String":
                    return node.Value;

                case "Int32":
                    if (int.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out int i32Value))
                    {
                        return i32Value;
                    }
                    break;

                case "Int64":
                    if (long.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out long i64Value))
                    {
                        return i64Value;
                    }
                    break;

                case "Decimal":
                    if (decimal.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out decimal decimalValue))
                    {
                        return decimalValue;
                    }
                    break;

                case "Float":
                    if (decimal.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out decimal floatValue))
                    {
                        return floatValue;
                    }
                    break;

                case "Double":
                    if (double.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out double doubleValue))
                    {
                        return doubleValue;
                    }
                    break;

                case "Boolean":
                    if (bool.TryParse(node.Value, out bool boolValue))
                    {
                        return boolValue;
                    }
                    break;
            }
            return null;
        }
    }
}