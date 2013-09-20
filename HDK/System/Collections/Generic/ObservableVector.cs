using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace System.Collections.Generic
{
    /// <summary>
    /// IObservableVector<T> implementation.
    /// </summary>
    public class ObservableVector<T> : Collection<T>, INotifyPropertyChanged, IObservableVector<T>
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event VectorChangedEventHandler<T> VectorChanged;

        public ObservableVector() : base()
        {
        }

        public ObservableVector(IList<T> list) : base(list)
        {
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            this.RaisePropertyChanged(PropertyChanged, "Count", "Items");
            this.RaiseVectorChanged(CollectionChange.Reset, 0);
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            this.RaisePropertyChanged(PropertyChanged, "Count", "Items");
            this.RaiseVectorChanged(CollectionChange.ItemInserted, (uint)index);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            this.RaisePropertyChanged(PropertyChanged, "Count", "Items");
            this.RaiseVectorChanged(CollectionChange.ItemRemoved, (uint)index);
        }

        protected override void SetItem(int index, T item)
        {
            base.SetItem(index, item);
            this.RaisePropertyChanged(PropertyChanged, "Items");
            this.RaiseVectorChanged(CollectionChange.ItemChanged, (uint)index);
        }

        protected void RaiseVectorChanged(CollectionChange collectionChange, uint index)
        {
            this.OnVectorChanged(new VectorChangedEventArgs(collectionChange, index));
        }

        protected virtual void OnVectorChanged(IVectorChangedEventArgs e)
        {
            if (this.VectorChanged != null)
                this.VectorChanged(this, e);
        }
    }
}
