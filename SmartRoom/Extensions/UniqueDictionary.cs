using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SmartRoom.Extensions
{
    public class UniqueDictionary<T1, T2>
    {
        public class Indexer<T3, T4>
        {
            private Dictionary<T3, T4> _dictionary;
            public Indexer(Dictionary<T3, T4> dictionary)
            {
                _dictionary = dictionary;
            }

            public T4 this[T3 key] => _dictionary[key];
        }

        private Dictionary<T1, T2> _forward;
        private Dictionary<T2, T1> _backward;

        public Indexer<T1,T2> Forward { get; private set; }
        public Indexer<T2, T1> Backward { get; private set; }

        public UniqueDictionary()
        {
            _forward = new Dictionary<T1, T2>();
            _backward = new Dictionary<T2, T1>();
            Forward = new Indexer<T1, T2>(_forward);
            Backward = new Indexer<T2, T1>(_backward);
        }

        public ICollection<T1> Keys => _forward.Keys;

        public ICollection<T2> Values => _backward.Keys;

        public int Count => _forward.Count;

        public void Add(T1 first, T2 second)
        {
            _forward.Add(first, second);
            _backward.Add(second, first);
        }

        public void Clear()
        {
            _forward.Clear();
            _backward.Clear();
        }

        public bool Contains(KeyValuePair<T1, T2> item) => _forward.Contains(item);

        public bool ContainsKey(T1 key) => _forward.ContainsKey(key);

        public bool ContainsValue(T2 value) => _backward.ContainsKey(value);

        public bool RemoveKey(T1 key)
        {
            if (_forward.TryGetValue(key, out T2 val))
            {
                _backward.Remove(val);
                _forward.Remove(key);
                return true;
            }
            else
                return false;
        }

        public bool RemoveValue(T2 value)
        {
            if (_backward.TryGetValue(value, out T1 val))
            {
                _forward.Remove(val);
                _backward.Remove(value);
                return true;
            }
            else
                return false;
        }

        public bool TryReplaceKey(T1 oldKey, T1 newKey)
        {
            T2 val;
            if (_forward.TryGetValue(oldKey, out val) == false ||
                _forward.ContainsKey(newKey) == true)
                return false;

            if (this.RemoveKey(oldKey) == false)
                return false;

            this.Add(newKey, val);
            return true;
        }

        public bool TryReplaceValue(T2 oldValue, T2 newValue)
        {
            T1 key;
            if (_backward.TryGetValue(oldValue, out key) == false ||
                _backward.ContainsKey(newValue) == true)
                return false;

            if (this.RemoveValue(oldValue) == false)
                return false;

            this.Add(key, newValue);
            return true;
        }

        public bool TryGetValue(T1 key, out T2 value)
        {
            if (_forward.TryGetValue(key, out value))
                return true;
            else
                return false;
        }

        public bool TryGetKey(T2 key, out T1 value)
        {
            if (_backward.TryGetValue(key, out value))
                return true;
            else
                return false;
        }
    }
}