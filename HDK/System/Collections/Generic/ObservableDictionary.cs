using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace System.Collections.Generic
{
    /// <summary>
    /// Implementation of IObservableMap that supports reentrancy for use as a default view
    /// model.
    /// </summary>
    public class ObservableDictionary<TKey, TValue> : IObservableMap<TKey, TValue>
    {
        private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
        public event MapChangedEventHandler<TKey, TValue> MapChanged;

        private void InvokeMapChanged(CollectionChange change, TKey key)
        {
            var eventHandler = MapChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new ObservableDictionaryChangedEventArgs<TKey>(change, key));
            }
        }

        public void Add(TKey key, TValue value)
        {
            this._dictionary.Add(key, value);
            this.InvokeMapChanged(CollectionChange.ItemInserted, key);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            this.Add(item.Key, item.Value);
        }

        public bool Remove(TKey key)
        {
            if (this._dictionary.Remove(key))
            {
                this.InvokeMapChanged(CollectionChange.ItemRemoved, key);
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            TValue currentValue;
            if (this._dictionary.TryGetValue(item.Key, out currentValue) &&
                Object.Equals(item.Value, currentValue) && this._dictionary.Remove(item.Key))
            {
                this.InvokeMapChanged(CollectionChange.ItemRemoved, item.Key);
                return true;
            }
            return false;
        }

        public TValue this[TKey key]
        {
            get
            {
                return this._dictionary[key];
            }
            set
            {
                this._dictionary[key] = value;
                this.InvokeMapChanged(CollectionChange.ItemChanged, key);
            }
        }

        public void Clear()
        {
            var priorKeys = this._dictionary.Keys.ToArray();
            this._dictionary.Clear();
            foreach (var key in priorKeys)
            {
                this.InvokeMapChanged(CollectionChange.ItemRemoved, key);
            }
        }

        public ICollection<TKey> Keys
        {
            get { return this._dictionary.Keys; }
        }

        public bool ContainsKey(TKey key)
        {
            return this._dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return this._dictionary.TryGetValue(key, out value);
        }

        public ICollection<TValue> Values
        {
            get { return this._dictionary.Values; }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return this._dictionary.Contains(item);
        }

        public int Count
        {
            get { return this._dictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return this._dictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this._dictionary.GetEnumerator();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            int arraySize = array.Length;
            foreach (var pair in this._dictionary)
            {
                if (arrayIndex >= arraySize) break;
                array[arrayIndex++] = pair;
            }
        }
    }
}
