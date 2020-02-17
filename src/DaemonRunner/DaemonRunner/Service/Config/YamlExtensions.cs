using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using JoySoftware.HomeAssistant.NetDaemon.Common;
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
        public static void SetPropertyFromYaml(this INetDaemonApp app, PropertyInfo prop, YamlSequenceNode seq)
        {
            if (prop.PropertyType.IsGenericType && prop.PropertyType?.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                Type listType = prop.PropertyType?.GetGenericArguments()[0] ??
                    throw new NullReferenceException($"The property {prop.Name} of Class {app.GetType().Name} is not compatible with configuration");

                IList list = listType.CreateListOfPropertyType() ??
                    throw new NullReferenceException("Failed to create listtype, plese check {prop.Name} of Class {app.GetType().Name}");

                foreach (YamlNode item in seq.Children)
                {
                    if (item.NodeType != YamlNodeType.Scalar)
                    {
                        throw new NotSupportedException($"The property {prop.Name} of Class {app.GetType().Name} is not compatible with configuration");
                    }

                    var value = ((YamlScalarNode)item).ToObject(listType) ??
                        throw new NotSupportedException($"The class {app.GetType().Name} and property {prop.Name} has wrong type in items");

                    list.Add(value);

                }
                // Bind the list to the property
                prop.SetValue(app, list);
            }
        }

        public static void SetPropertyFromYaml(this INetDaemonApp app, PropertyInfo prop, YamlScalarNode sc)
        {

            var scalarValue = sc.ToObject(prop.PropertyType) ??
                throw new NotSupportedException($"The class {app.GetType().Name} and property {prop.Name} unexpected value {sc.Value} is wrong type");

            // Bind the list to the property
            prop.SetValue(app, scalarValue);

        }

        public static INetDaemonApp? InstanceFromYamlConfig(this IEnumerable<Type> types, TextReader reader)
        {
            var yamlAppConfig = new YamlAppConfig(types, reader);
            return yamlAppConfig.Instance;
        }

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