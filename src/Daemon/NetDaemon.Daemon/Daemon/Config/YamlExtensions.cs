using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using NetDaemon.Common;
using NetDaemon.Common.Exceptions;
using NetDaemon.Common.Reactive;
using NetDaemon.Common.Reactive.Services;
using YamlDotNet.RepresentationModel;

[assembly: InternalsVisibleTo("NetDaemon.Daemon.Tests")]

namespace NetDaemon.Daemon.Config
{
    public static class PropertyInfoExtensions
    {
        public static IList? CreateListOfPropertyType(this Type listType)
        {
            Type gen = typeof(List<>).MakeGenericType(listType);
            var list = Activator.CreateInstance(gen);

            return list as IList;
        }
    }

    internal static class YamlExtensions
    {
        public static PropertyInfo? GetYamlProperty(this Type type, string propertyName)
        {
            _ = type ??
                throw new NetDaemonArgumentNullException(nameof(type));

            // Lets try convert from python style to CamelCase

            return type.GetProperty(propertyName) ?? type.GetProperty(propertyName.ToCamelCase());
        }

        public static object? ToObject(this YamlScalarNode node, Type valueType, ApplicationContext applicationContext)
        {
            _ = valueType ??
                throw new NetDaemonArgumentNullException(nameof(valueType));
            _ = node ??
                throw new NetDaemonArgumentNullException(nameof(node));

            valueType = Nullable.GetUnderlyingType(valueType) ?? valueType;

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

            if (valueType.IsEnum && Enum.TryParse(valueType, node.Value, out var enumValue)) return enumValue;

            if (valueType.IsAssignableTo(typeof(RxEntityBase)))
            {
                // ctor of RXEntityBase has a string[] parameters for the EntityId(s) 
                return Activator.CreateInstance(valueType, (INetDaemonRxApp)applicationContext.ApplicationInstance,
                    new[] { node.Value });
            }

            return CreateInstance(valueType, node.Value, applicationContext.ServiceProvider);
        }

        private static object CreateInstance(Type valueType, string? nodeValue, IServiceProvider serviceProvider)
        {
            // Create an instance of the property's type

            var constructors = valueType.GetConstructors();

            object[] additionalParameters = Array.Empty<object>();
            if (nodeValue != null && constructors.Any(c => c.GetParameters().Any(p => p.ParameterType == typeof(string))))
            {
                additionalParameters = new object[] { nodeValue };
            }

            return ActivatorUtilities.CreateInstance(serviceProvider, valueType, additionalParameters);
        }
    }
}