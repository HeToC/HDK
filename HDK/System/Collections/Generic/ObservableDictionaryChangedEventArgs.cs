using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace System.Collections.Generic
{
    public class ObservableDictionaryChangedEventArgs<TKey> : IMapChangedEventArgs<TKey>
    {
        public ObservableDictionaryChangedEventArgs(CollectionChange change, TKey key)
        {
            this.CollectionChange = change;
            this.Key = key;
        }

        public CollectionChange CollectionChange { get; private set; }
        public TKey Key { get; private set; }
    }
}
