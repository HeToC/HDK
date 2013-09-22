using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;

namespace System.Collections.Generic
{
    public class ObservableVector<TElement> : ObservableVector<TElement, List<object>>
    {
    }

    public class ObservableVector<TElement, TInner> :BindableBase, IObservableVector<TElement> where TInner: IList, new()
    {
        private TInner inner;

        public ObservableVector()
        {
            inner = InitializeInnerContainer();
        }

        protected virtual TInner InitializeInnerContainer()
        {
            var tmp = (TInner)(new List<TElement>() as IList);
            return tmp;
        }

        public event VectorChangedEventHandler<TElement> VectorChanged;
        protected virtual void RaiseVectorChanged(CollectionChange collectionChange, int index = -1, TElement item = default(TElement))
        {
            var handler = this.VectorChanged;
            if (handler != null)
                handler(this, new VectorChangedEventArgs(collectionChange,index));
        }

        public virtual int IndexOf(TElement item)
        {
            return this.inner.IndexOf(item);
        }

        public virtual void Insert(int index, TElement item)
        {
            CheckReadOnly();
            this.inner.Insert(index, item);
            RaisePropertyChanged("Items");
            this.RaiseVectorChanged(CollectionChange.ItemInserted, index);
        }

        public virtual void RemoveAt(int index)
        {
            CheckReadOnly();
            this.inner.RemoveAt(index);
            RaisePropertyChanged("Count", "Items");
            this.RaiseVectorChanged(CollectionChange.ItemRemoved, index);
        }

        public virtual TElement this[int index]
        {
            get
            {
                return (TElement)this.inner[index];
            }
            set
            {
                CheckReadOnly();
                this.inner[index] = value;
                this.RaisePropertyChanged("Items");
                this.RaiseVectorChanged(CollectionChange.ItemChanged, index);
            }
        }

        public virtual void Add(TElement item)
        {
            CheckReadOnly();
            this.inner.Add(item);
            this.RaisePropertyChanged("Count", "Items");
            this.RaiseVectorChanged(CollectionChange.ItemInserted, this.inner.Count - 1);
        }

        public virtual void Clear()
        {
            CheckReadOnly();
            this.inner.Clear();
            this.RaisePropertyChanged("Count", "Items");
            this.RaiseVectorChanged(CollectionChange.Reset, 0);
        }

        public virtual bool Contains(TElement item)
        {
            return this.inner.Contains(item);
        }

        public virtual void CopyTo(TElement[] array, int arrayIndex)
        {
            this.inner.CopyTo(array, arrayIndex);
        }

        public virtual int Count
        {
            get { return this.inner.Count; }
        }

        public virtual bool IsReadOnly
        {
            get { return false; } // return inner.IsReadOnly; }
        }

        public virtual bool Remove(TElement item)
        {
            CheckReadOnly();

            var index = this.inner.IndexOf(item);
            if (index == -1)
            {
                return false;
            }
            this.RemoveAt(index);
            return true;
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return this.inner.GetEnumerator() as IEnumerator<TElement>;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.inner.GetEnumerator();
        }

        void CheckReadOnly()
        {
            if (IsReadOnly)
            {
                throw new Exception("The source collection cannot be modified.");
            }
        }
    }
}
