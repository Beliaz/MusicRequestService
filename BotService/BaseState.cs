using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Bot
{
    public class BaseState : IDictionary<string, object>
    {
        public BaseState()
        {
            _dict = new Dictionary<string, object>();
        }

        public BaseState(IDictionary<string, object> source)
        {
            _dict = source;
        }

        protected T GetProperty<T>([CallerMemberName] string propName = null)
        {
            if (TryGetValue(propName ?? throw new ArgumentNullException(nameof(propName)),
                out var value))
            {
                return (T) value;
            }

            return default(T);
        }

        protected void SetProperty(object value, [CallerMemberName] string propName = null)
        {
            this[propName ?? throw new ArgumentNullException(nameof(propName))] = value;
        }

        private readonly IDictionary<string, object> _dict;

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _dict).GetEnumerator();
        }

        public void Add(KeyValuePair<string, object> item)
        {
            _dict.Add(item);
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
            _dict.CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            return _dict.Remove(item);
        }

        public int Count => _dict.Count;

        public bool IsReadOnly => _dict.IsReadOnly;

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

        public ICollection<string> Keys => _dict.Keys;

        public ICollection<object> Values => _dict.Values;
    }
}