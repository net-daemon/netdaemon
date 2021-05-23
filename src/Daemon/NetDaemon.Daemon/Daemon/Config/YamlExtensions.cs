using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using NetDaemon.Common;
using NetDaemon.Common.Exceptions;
using NetDaemon.Common.Reactive.Services;
using YamlDotNet.RepresentationModel;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Daemon.Config
{
    public static class PropertyInfoExtensions
    {
        public static IList? CreateListOfPropertyType(this Type listType)
        {
            Type gen = typeof(List<>).MakeGenericType(listType!);
            var list = Activator.CreateInstance(gen);

            return list as IList;
        }
    }

    public static class YamlExtensions
    {
        public static PropertyInfo? GetYamlProperty(this Type type, string propertyName)
        {
            _ = type ??
                throw new NetDaemonArgumentNullException(nameof(type));

            // Lets try convert from python style to CamelCase

            return type.GetProperty(propertyName) ?? type.GetProperty(propertyName.ToCamelCase());
        }

        public static object? ToObject(this YamlScalarNode node, Type valueType, INetDaemonAppBase? deamonApp)
        {
            _ = valueType ??
                throw new NetDaemonArgumentNullException(nameof(valueType));
            _ = node ??
                throw new NetDaemonArgumentNullException(nameof(node));

            var underlyingNullableType = Nullable.GetUnderlyingType(valueType);

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
                        CultureInfo.InvariantCulture, out var i32Value))
                    {
                        return i32Value;
                    }

                    break;

                case "Int64":
                    if (long.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out var i64Value))
                    {
                        return i64Value;
                    }

                    break;

                case "Decimal":
                    if (decimal.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out var decimalValue))
                    {
                        return decimalValue;
                    }

                    break;

                case "Single":
                    if (float.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out var floatValue))
                    {
                        return floatValue;
                    }

                    break;

                case "Double":
                    if (double.TryParse(node.Value, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out var doubleValue))
                    {
                        return doubleValue;
                    }

                    break;

                case "Boolean":
                    if (bool.TryParse(node.Value, out var boolValue)) return boolValue;

                    break;
            }

            if (deamonApp != null && valueType.IsAssignableTo(typeof(RxEntityBase)))
                return Activator.CreateInstance(valueType, deamonApp, new[] {node.Value});
            return null;
        }
    }
}