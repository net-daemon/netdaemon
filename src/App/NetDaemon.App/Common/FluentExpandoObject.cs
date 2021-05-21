using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using JoySoftware.HomeAssistant.Helpers.Json;
using NetDaemon.Common.Exceptions;

namespace NetDaemon.Common
{
    /// <summary>
    ///     A custom expando object to alow to return null values if properties does not exist
    /// </summary>
    /// <remarks>
    ///     Thanks to @lukevendediger for original code and inspiration
    ///     https://gist.github.com/lukevenediger/6327599
    /// </remarks>
    [SuppressMessage("Microsoft.Naming", "CA1710")]
    public class FluentExpandoObject : DynamicObject, IDictionary<string, object>
    {
        private readonly Dictionary<string, object> _dict = new();
        private readonly NetDaemonAppBase? _daemonApp;
        private readonly bool _ignoreCase;
        private readonly bool _returnNullMissingProperties;

        /// <summary>
        ///     Creates a BetterExpando object/
        /// </summary>
        /// <param name="ignoreCase">Don't be strict about property name casing.</param>
        /// <param name="returnNullMissingProperties">If true, returns String.Empty for missing properties.</param>
        /// <param name="root">An ExpandoObject to consume and expose.</param>
        /// <param name="daemon">A NetDaemon object used for persistanse</param>
        public FluentExpandoObject(bool ignoreCase = false,
            bool returnNullMissingProperties = false,
            ExpandoObject? root = null, NetDaemonAppBase? daemon = null)
        {
            _daemonApp = daemon;
            _ignoreCase = ignoreCase;
            _returnNullMissingProperties = returnNullMissingProperties;
            if (root != null) Augment(root);
        }

        /// <inheritdoc/>
        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_dict).GetEnumerator();
        }

        /// <inheritdoc/>
        public void Add(KeyValuePair<string, object> item)
        {
            _dict.Add(item.Key, item.Value);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _dict.Clear();
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<string, object> item)
        {
            return _dict.Contains(item);
        }

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            // Is not used so no use in implementing it
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<string, object> item)
        {
            return _dict.Remove(item.Key, out _);
        }

        /// <inheritdoc/>
        public int Count => _dict.Count;

        /// <inheritdoc/>
        public bool IsReadOnly => false;

        /// <inheritdoc/>
        public void Add(string key, object value)
        {
            _dict.Add(key, value);
        }

        /// <inheritdoc/>
        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }

        /// <inheritdoc/>
        public bool Remove(string key)
        {
            return _dict.Remove(key);
        }

        /// <inheritdoc/>
        public bool TryGetValue(string key, out object value)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            return _dict.TryGetValue(key, out value);
#pragma warning restore CS8601 // Possible null reference assignment.
        }

        /// <inheritdoc/>
        public object this[string key]
        {
            get => _dict[key];
            set => _dict[key] = value;
        }

        /// <inheritdoc/>
        public ICollection<string> Keys => ((IDictionary<string, object>)_dict).Keys;

        /// <inheritdoc/>
        public ICollection<object> Values => ((IDictionary<string, object>)_dict).Values;

        /// <inheritdoc/>
        public override bool TrySetMember(SetMemberBinder binder, object? value)
        {
            if (binder is null)
                throw new NetDaemonNullReferenceException(nameof(binder));

            UpdateDictionary(binder.Name, value);
            // It is supposed to persist, this is the only reason _daemon is present
            _daemonApp?.SaveAppState();

            return true;
        }

        /// <inheritdoc/>
        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object? value)
        {
            if (indexes is null)
                throw new NetDaemonNullReferenceException(nameof(indexes));

            if (!(indexes[0] is string)) return base.TrySetIndex(binder, indexes, value);

            if (indexes[0] is string key) UpdateDictionary(NormalizePropertyName(key), value);
            return true;
        }

        /// <inheritdoc/>
        public override bool TryGetMember(GetMemberBinder binder, out object? result)
        {
            if (binder is null)
                throw new NetDaemonNullReferenceException(nameof(binder));

            var key = NormalizePropertyName(binder.Name);

            if (_dict.ContainsKey(key))
            {
                result = _dict[key];
                return true;
            }

            if (!_returnNullMissingProperties) return base.TryGetMember(binder, out result);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            result = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            return true;
        }

        /// <summary>
        ///     Combine two instances together to get a union.
        /// </summary>
        /// <returns>This instance but with additional properties</returns>
        /// <remarks>Existing properties are not overwritten.</remarks>
        public dynamic Augment(FluentExpandoObject obj)
        {
            if (obj is null)
                throw new NetDaemonNullReferenceException(nameof(obj));

            obj._dict
                .Where(pair => !_dict.ContainsKey(NormalizePropertyName(pair.Key)))
                .ToList()
                .ForEach(pair => UpdateDictionary(pair.Key, pair.Value));
            return this;
        }

        /// <summary>
        ///     Copy all items from  FluentExpandoObject
        /// </summary>
        /// <param name="obj">The object to copy from</param>
        public dynamic CopyFrom(IDictionary<string, object?> obj)
        {
            if (obj is null)
                throw new NetDaemonNullReferenceException(nameof(obj));

            // Clear any items before copy
            Clear();

            foreach (var keyValuePair in obj)
            {
                if (keyValuePair.Value is JsonElement val)
                {
                    var dynValue = val.ToDynamicValue();
                    if (dynValue is not null)
                    {
                        UpdateDictionary(keyValuePair.Key, dynValue);
                    }
                }
                else
                {
                    UpdateDictionary(keyValuePair.Key, keyValuePair.Value);
                }
            }

            return this;
        }

        /// <summary>
        ///     Combine two instances together to get a union.
        /// </summary>
        /// <param name="obj">the object to combine</param>
        public dynamic Augment(ExpandoObject obj)
        {
            obj
                .Where(pair => !_dict.ContainsKey(NormalizePropertyName(pair.Key)))
                .ToList()
                .ForEach(pair => UpdateDictionary(pair.Key, pair.Value));
            return this;
        }

        /// <inheritdoc/>
        public T ValueOrDefault<T>(string propertyName, T defaultValue)
        {
            if (propertyName is null)
                throw new NetDaemonNullReferenceException(nameof(propertyName));

            propertyName = NormalizePropertyName(propertyName);
            return _dict.ContainsKey(propertyName)
                ? (T)_dict[propertyName]
                : defaultValue;
        }

        /// <summary>
        ///     Check if BetterExpando contains a property.
        /// </summary>
        /// <remarks>Respects the case sensitivity setting</remarks>
        public bool HasProperty(string name)
        {
            if (name is null)
                throw new NetDaemonNullReferenceException(nameof(name));

            return _dict.ContainsKey(NormalizePropertyName(name));
        }

        /// <summary>
        ///     Returns this object as comma-separated name-value pairs.
        /// </summary>
        public override string ToString()
        {
            return string.Join(", ", _dict.Select(pair => pair.Key + " = " + pair.Value ?? "(null)").ToArray());
        }

        private void UpdateDictionary(string name, object? value)
        {
            _ = value ?? throw new ArgumentNullException(nameof(value), "value cannot be null");
            var key = NormalizePropertyName(name);
            _dict[key] = value;
        }

        [SuppressMessage("", "CA1308")]
        private string NormalizePropertyName(string propertyName)
        {
            return _ignoreCase ? propertyName.ToLower(CultureInfo.InvariantCulture) : propertyName;
        }
    }
}