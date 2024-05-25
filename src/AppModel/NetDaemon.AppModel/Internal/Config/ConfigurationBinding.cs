using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace NetDaemon.AppModel.Internal.Config;

internal interface IConfigurationBinding
{
    T? ToObject<T>(IConfiguration configuration);
}

/// <summary>
///     Class that allows binding strongly typed objects to configuration values.
/// </summary>
/// <remarks>
///     This is a scaled down and modified version of .NET 7 implementation.
/// </remarks>
internal class ConfigurationBinding : IConfigurationBinding
{
    private readonly IServiceProvider _provider;

    public ConfigurationBinding(IServiceProvider provider)
    {
        _provider = provider;
    }

    /// <summary>
    ///     Attempts to bind the configuration instance to a new instance of type T.
    ///     If  configuration section has a value, that will be used.
    ///     Otherwise binding by matching property names against configuration keys recursively.
    /// </summary>
    /// <typeparam name="T">The type of the new instance to bind.</typeparam>
    /// <param name="configuration">The configuration instance to bind.</param>
    /// <returns>The new instance of T if successful, default(T) otherwise.</returns>
    public T? ToObject<T>(IConfiguration configuration)
    {
        var result = GetObject(configuration, typeof(T));
        if (result == null) return default;
        return (T)result;
    }

    /// <summary>
    ///     Attempts to bind the configuration instance to a new instance of type T.
    ///     If  configuration section has a value, that will be used.
    ///     Otherwise binding by matching property names against configuration keys recursively.
    /// </summary>
    /// <param name="configuration">The configuration instance to bind.</param>
    /// <param name="type">The type of the new instance to bind.</param>
    /// <returns>The new instance if successful, null otherwise.</returns>
    private object? GetObject(IConfiguration configuration, Type type)
    {
        return BindInstance(type, null, configuration);
    }

    private void BindNonScalar(IConfiguration configuration, object? instance)
    {
        if (instance == null) return;

        // If we came this far and it is readonly collection
        // it means that the collection was already initialized
        // that is not a valid operation
        ThrowIfReadonlyCollection(instance);

        foreach (var property in GetAllProperties(instance.GetType().GetTypeInfo()))
            BindProperty(property, instance, configuration);
    }

    private static void ThrowIfReadonlyCollection(object? instance)
    {
        var typ = instance?.GetType()!;

        if (FindOpenGenericInterface(typeof(IEnumerable<>), typ) != null)
        {
            throw new InvalidOperationException("Read-only collections cannot be initialized and needs to be nullable!");
        }

        if (FindOpenGenericInterface(typeof(IReadOnlyCollection<>), typ) != null)
        {
            throw new InvalidOperationException("Read-only collections cannot be initialized and needs to be nullable!");
        }
    }

    private void BindProperty(PropertyInfo property, object? instance, IConfiguration config)
    {
        // We don't support set only, non public, or indexer properties
        if (property.GetMethod?.IsPublic != true ||
            property.GetMethod.GetParameters().Length > 0)
            return;

        var propertyValue = property.GetValue(instance);
        var hasSetter = property.SetMethod?.IsPublic == true;

        if (propertyValue == null && !hasSetter)
            // Property doesn't have a value and we cannot set it so there is no
            // point in going further down the graph
            return;

        propertyValue = BindInstance(property.PropertyType, propertyValue, config.GetSection(property.Name));

        if (propertyValue != null && hasSetter) property.SetValue(instance, propertyValue);
    }

    private object BindToCollection(TypeInfo typeInfo, IConfiguration config)
    {
        var type = typeof(List<>).MakeGenericType(typeInfo.GenericTypeArguments[0]);
        var instance = Activator.CreateInstance(type) ??
                       throw new InvalidOperationException();
        BindCollection(instance, type, config);
        return instance;
    }

    // Try to create an array/dictionary instance to back various collection interfaces
    private object? AttemptBindToCollectionInterfaces(Type type, IConfiguration config)
    {
        var typeInfo = type.GetTypeInfo();

        if (!typeInfo.IsInterface) return null;

        var collectionInterface = FindOpenGenericInterface(typeof(IReadOnlyList<>), type);
        if (collectionInterface != null)
            // IEnumerable<T> is guaranteed to have exactly one parameter
            return BindToCollection(typeInfo, config);

        collectionInterface = FindOpenGenericInterface(typeof(IReadOnlyDictionary<,>), type);
        if (collectionInterface != null)
        {
            var dictionaryType =
                typeof(Dictionary<,>).MakeGenericType(typeInfo.GenericTypeArguments[0],
                    typeInfo.GenericTypeArguments[1]);
            var instance = Activator.CreateInstance(dictionaryType);
            BindDictionary(instance, dictionaryType, config);
            return instance;
        }

        collectionInterface = FindOpenGenericInterface(typeof(IDictionary<,>), type);
        if (collectionInterface != null)
        {
            var instance = Activator.CreateInstance(
                typeof(Dictionary<,>).MakeGenericType(typeInfo.GenericTypeArguments[0],
                    typeInfo.GenericTypeArguments[1]));
            BindDictionary(instance, collectionInterface, config);
            return instance;
        }

        collectionInterface = FindOpenGenericInterface(typeof(IReadOnlyCollection<>), type);
        if (collectionInterface != null)
            // IReadOnlyCollection<T> is guaranteed to have exactly one parameter
            return BindToCollection(typeInfo, config);

        collectionInterface = FindOpenGenericInterface(typeof(ICollection<>), type);
        if (collectionInterface != null)
            // ICollection<T> is guaranteed to have exactly one parameter
            return BindToCollection(typeInfo, config);

        collectionInterface = FindOpenGenericInterface(typeof(IEnumerable<>), type);
        return collectionInterface != null ? BindToCollection(typeInfo, config) : null;
    }

    private object? BindInstance(Type type, object? instance, IConfiguration config)
    {
        // if binding IConfigurationSection, break early
        if (type == typeof(IConfigurationSection)) return config;

        var section = config as IConfigurationSection;
        var configValue = section?.Value;
        if (configValue != null && TryConvertValue(type, configValue, out var convertedValue, out var error))
        {
            if (error != null) throw error;

            // Leaf nodes are always reinitialized
            return convertedValue;
        }

        if (config.GetChildren().Any() != true) return instance;

        // If we don't have an instance, try to create one
        if (instance == null)
        {
            // We are already done if binding to a new collection instance worked
            instance = AttemptBindToCollectionInterfaces(type, config);
            if (instance != null) return instance;

            instance = CreateInstance(type);
        }

        // See if its a Dictionary
        var collectionInterface = FindOpenGenericInterface(typeof(IDictionary<,>), type);
        if (collectionInterface != null)
        {
            BindDictionary(instance, collectionInterface, config);
        }
        else if (type.IsArray)
        {
            instance = BindArray((Array)instance, config);
        }
        else
        {
            // See if its an ICollection
            collectionInterface = FindOpenGenericInterface(typeof(ICollection<>), type);
            if (collectionInterface != null)
                BindCollection(instance, collectionInterface, config);
            // Something else
            else
                BindNonScalar(config, instance);
        }

        return instance;
    }

    private static object CreateInstance(Type type)
    {
        var typeInfo = type.GetTypeInfo();

        if (typeInfo.IsInterface || typeInfo.IsAbstract) throw new InvalidOperationException();

        if (type.IsArray)
        {
            if (typeInfo.GetArrayRank() > 1) throw new InvalidOperationException();
            var typ = typeInfo.GetElementType()!;
            return Array.CreateInstance(typ, 0);
        }

        var hasDefaultConstructor =
            typeInfo.DeclaredConstructors.Any(ctor => ctor.IsPublic && ctor.GetParameters().Length == 0);
        if (!hasDefaultConstructor) throw new InvalidOperationException();

        try
        {
            return Activator.CreateInstance(type)!;
        }
        catch (Exception)
        {
            throw new InvalidOperationException();
        }
    }

    private void BindDictionary(object? dictionary, Type dictionaryType, IConfiguration config)
    {
        var typeInfo = dictionaryType.GetTypeInfo();

        // IDictionary<K,V> is guaranteed to have exactly two parameters
        var keyType = typeInfo.GenericTypeArguments[0];
        var valueType = typeInfo.GenericTypeArguments[1];
        var keyTypeIsEnum = keyType.GetTypeInfo().IsEnum;

        if (keyType != typeof(string) && !keyTypeIsEnum)
            // We only support string and enum keys
            return;

        var setter = typeInfo.GetDeclaredProperty("Item") ??
                     throw new InvalidOperationException();

        foreach (var child in config.GetChildren())
        {
            var item = BindInstance(
                valueType,
                null,
                child);
            if (item == null) continue;
            if (keyType == typeof(string))
            {
                var key = child.Key;
                setter.SetValue(dictionary, item, [key]);
            }
            else if (keyTypeIsEnum)
            {
                var key = Convert.ToInt32(Enum.Parse(keyType, child.Key), CultureInfo.InvariantCulture);
                setter.SetValue(dictionary, item, [key]);
            }
        }
    }

    private void BindCollection(object collection, Type collectionType, IConfiguration config)
    {
        var typeInfo = collectionType.GetTypeInfo();

        // ICollection<T> is guaranteed to have exactly one parameter
        var itemType = typeInfo.GenericTypeArguments[0];
        var addMethod = typeInfo.GetDeclaredMethod("Add") ??
                        throw new InvalidOperationException();

        foreach (var section in config.GetChildren())
            try
            {
                var item = BindInstance(
                    itemType,
                    null,
                    section);
                if (item != null) addMethod.Invoke(collection, [item]);
            }
            catch
            {
                // ignored
            }
    }

    private Array BindArray(Array source, IConfiguration config)
    {
        var children = config.GetChildren().ToArray();
        var arrayLength = source.Length;
        var elementType = source.GetType().GetElementType() ??
                          throw new InvalidOperationException();

        var newArray = Array.CreateInstance(elementType, arrayLength + children.Length);

        // binding to array has to preserve already initialized arrays with values
        if (arrayLength > 0) Array.Copy(source, newArray, arrayLength);

        for (var i = 0; i < children.Length; i++)
            try
            {
                var item = BindInstance(
                    elementType,
                    null,
                    children[i]);
                if (item != null) newArray.SetValue(item, arrayLength + i);
            }
            catch
            {
                // ignored
            }

        return newArray;
    }

    private bool TryConvertValue(Type type, string value, out object? result, out Exception? error)
    {
        error = null;
        result = null;
        if (type == typeof(object))
        {
            result = value;
            return true;
        }

        if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            if (string.IsNullOrEmpty(value)) return true;
            var typ = Nullable.GetUnderlyingType(type) ??
                      throw new InvalidOperationException();

            return TryConvertValue(typ, value, out result, out error);
        }

        var converter = TypeDescriptor.GetConverter(type);
        if (converter.CanConvertFrom(typeof(string)))
        {
            try
            {
                result = converter.ConvertFromInvariantString(value);
            }
            catch (Exception)
            {
                error = new InvalidOperationException();
            }

            return true;
        }

        // No standard converter is available so lets´s try find
        // a registered service that converts to the type from string

        try
        {
            result = ActivatorUtilities.CreateInstance(_provider, type, value);
        }
        catch (Exception e)
        {
            throw new InvalidOperationException($"Failed to convert from string to type {type.FullName}, please check you have one string in constructor!", e);
        }

        return true;
    }

    private static Type? FindOpenGenericInterface(Type expected, Type actual)
    {
        var actualTypeInfo = actual.GetTypeInfo();
        if (actualTypeInfo.IsGenericType &&
            actual.GetGenericTypeDefinition() == expected)
            return actual;

        return actualTypeInfo.ImplementedInterfaces.FirstOrDefault(interfaceType =>
            interfaceType.GetTypeInfo().IsGenericType && interfaceType.GetGenericTypeDefinition() == expected);
    }

    private static IEnumerable<PropertyInfo> GetAllProperties(TypeInfo type)
    {
        var allProperties = new List<PropertyInfo>();

        var currentType = type;
        do
        {
            allProperties.AddRange(currentType.DeclaredProperties);
            currentType = currentType.BaseType?.GetTypeInfo() ??
                          throw new InvalidOperationException();
        } while (currentType != typeof(object).GetTypeInfo());

        return allProperties;
    }
}
