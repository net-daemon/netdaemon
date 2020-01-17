using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;

namespace JoySoftware.HomeAssistant.NetDaemon.Common
{
    public  class ExpandoObjectOperators
    {
        public static bool operator ==(ExpandoObjectOperators a, int b) => false;
        public static bool operator !=(ExpandoObjectOperators a, int b) => true;
    }
    public class FluentExpandoObject : DynamicObject, IDictionary<string, object>
    {
        private Dictionary<string, object> _dict;
        private bool _ignoreCase;
        private bool _returnEmptyStringForMissingProperties;

        /// <summary>
        /// Creates a BetterExpando object/
        /// </summary>
        /// <param name="ignoreCase">Don't be strict about property name casing.</param>
        /// <param name="returnEmptyStringForMissingProperties">If true, returns String.Empty for missing properties.</param>
        /// <param name="root">An ExpandoObject to consume and expose.</param>
        public FluentExpandoObject(bool ignoreCase = false,
          bool returnEmptyStringForMissingProperties = false,
          ExpandoObject root = null)
        {
            _dict = new Dictionary<string, object>();
            _ignoreCase = ignoreCase;
            _returnEmptyStringForMissingProperties = returnEmptyStringForMissingProperties;
            if (root != null)
            {
                Augment(root);
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            UpdateDictionary(binder.Name, value);
            return true;
        }

        public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
        {
            if (indexes[0] is string)
            {
                var key = indexes[0] as string;
                UpdateDictionary(NormalisePropertyName(key), value);
                return true;
            }
            else
            {
                return base.TrySetIndex(binder, indexes, value);
            }
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            
            var key = NormalisePropertyName(binder.Name);
            if (_dict.ContainsKey(key))
            {
                result = _dict[key];
                return true;
            }
            if (_returnEmptyStringForMissingProperties)
            {
                result = null;
                return true;
            }
            return base.TryGetMember(binder, out result);
        }

        /// <summary>
        /// Combine two instances together to get a union.
        /// </summary>
        /// <returns>This instance but with additional properties</returns>
        /// <remarks>Existing properties are not overwritten.</remarks>
        public dynamic Augment(FluentExpandoObject obj)
        {
            obj._dict
              .Where(pair => !_dict.ContainsKey(NormalisePropertyName(pair.Key)))
              .ToList()
              .ForEach(pair => UpdateDictionary(pair.Key, pair.Value));
            return this;
        }

        public dynamic Augment(ExpandoObject obj)
        {
            ((IDictionary<string, object>)obj)
              .Where(pair => !_dict.ContainsKey(NormalisePropertyName(pair.Key)))
              .ToList()
              .ForEach(pair => UpdateDictionary(pair.Key, pair.Value));
            return this;
        }

        public T ValueOrDefault<T>(string propertyName, T defaultValue)
        {
            propertyName = NormalisePropertyName(propertyName);
            return _dict.ContainsKey(propertyName)
              ? (T)_dict[propertyName]
              : defaultValue;
        }

        /// <summary>
        /// Check if BetterExpando contains a property.
        /// </summary>
        /// <remarks>Respects the case sensitivity setting</remarks>
        public bool HasProperty(string name)
        {
            return _dict.ContainsKey(NormalisePropertyName(name));
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        /// <summary>
        /// Returns this object as comma-separated name-value pairs.
        /// </summary>
        public override string ToString()
        {
            return String.Join(", ", _dict.Select(pair => pair.Key + " = " + pair.Value ?? "(null)").ToArray());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _dict).GetEnumerator();
        }

        private void UpdateDictionary(string name, object value)
        {
            var key = NormalisePropertyName(name);
            if (_dict.ContainsKey(key))
            {
                _dict[key] = value;
            }
            else
            {
                _dict.Add(key, value);
            }
        }

        private string NormalisePropertyName(string propertyName)
        {
            return _ignoreCase ? propertyName.ToLower() : propertyName;
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _dict.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _dict.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return _dict.Contains(item);
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            object val;
            return _dict.Remove(item.Key, out val);
        }

        public int Count => _dict.Count;

        public bool IsReadOnly =>  false;

        public void Add(string key, object value)
        {
            _dict.Add(key, value);
        }

        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _dict.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            return _dict.TryGetValue(key, out value);
        }

        public object this[string key]
        {
            get => _dict[key];
            set => _dict[key] = value;
        }

        public ICollection<string> Keys => ((IDictionary<string, object>) _dict).Keys;

        public ICollection<object> Values => ((IDictionary<string, object>) _dict).Values;
    }
}
