using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;

namespace System.Data
{
    public class DataObjectCollection<T> : 
        ObservableCollection<T>, 
        IDataObjectCollection
        //,        ISupportIncrementalLoading
        where T : DataObject
    {
        private readonly object _writeLock = new object();

        private readonly Dictionary<long, T> _primaryIndex = new Dictionary<long, T>();

        private readonly Func<DataObjectSet> _findContext;

        IDataObjectCollectionLoader<T> m_Loader;

        public DataObjectCollection(Func<DataObjectSet> findContext, IDataObjectCollectionLoader<T> loader = null)
        {
            _findContext = findContext;
            m_Loader = loader;
        }

        public DataObjectSet Context { get { return _findContext(); } }

        public override event NotifyCollectionChangedEventHandler CollectionChanged;
        public event PropertyChangedEventHandler EntityPropertyChanged;
        protected void RaiseEntityPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            var ev = EntityPropertyChanged;
            if (ev != null)
            {
                ev(this, eventArgs);
            }
        }

        public object FindByPrimaryKeyBase(long id)
        {
            return FindByPrimaryKey(id);
        }

        public T FindByPrimaryKey(long id)
        {
            T result;
            var entityExists = _primaryIndex.TryGetValue(id, out result);
            return entityExists ? result : default(T);
        }

        private void AddToIndex(T entity)
        {
            if (Context != null && entity.Context != Context)
            {
                throw new ArgumentException("The object belongs to a different DataObjectSet");
            }
            if (entity.Id == 0)
            {
                throw new ArgumentException("DataObject must not have Id of zero, when adding to collection");
            }
            if (!_primaryIndex.ContainsKey(entity.Id))
            {
                _primaryIndex.Add(entity.Id, entity);
                entity.PropertyChanged += OnObjectPropertyChanged;
            }
            else
            {
                throw new ArgumentException(string.Format("DataObject with primary key {0} already exists.", entity.Id));
            }
        }

        private void RemoveFromIndex(T entity)
        {
            if (_primaryIndex.ContainsKey(entity.Id))
            {
                _primaryIndex.Remove(entity.Id);
                entity.PropertyChanged -= OnObjectPropertyChanged;
            }
        }

        protected override void InsertItem(int index, T item)
        {
            lock (_writeLock)
            {
                AddToIndex(item);
                base.InsertItem(index, item);
            }
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            lock (_writeLock)
            {
                base.MoveItem(oldIndex, newIndex);
            }
        }

        protected override void RemoveItem(int index)
        {
            lock (_writeLock)
            {
                var item = this[index];
                RemoveFromIndex(item);
                base.RemoveItem(index);
            }
        }

        protected override void SetItem(int index, T item)
        {
            lock (_writeLock)
            {
                var oldItem = this[index];
                RemoveFromIndex(oldItem);
                try
                {
                    AddToIndex(item);
                    base.SetItem(index, item);
                }
                catch (Exception)
                {
                    AddToIndex(oldItem);
                    throw;
                }
            }
        }

        protected override void ClearItems()
        {
            lock (_writeLock)
            {
                var array = (from x in this select x).ToArray();
                _primaryIndex.Clear();
                base.ClearItems();
                foreach (var x in array)
                {
                    x.Delete();
                }
            }
        }

        private void OnObjectPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == DataObject.ObjectContextName)
            {
                var entity = sender as T;
                if (entity != null && entity.Context != Context)
                {
                    Remove(entity);
                }
            }
            else
            {
                RaiseEntityPropertyChanged(e);
            }
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            using (BlockReentrancy())
            {
                var ev = CollectionChanged;
                if (ev == null) return;
                var invocationList = ev.GetInvocationList();
                var dispatcherObject = Context;
                //bool isOnDifferentThread = dispatcherObject != null && dispatcherObject.Dispatcher != null &&
                //                                 dispatcherObject.Dispatcher.HasThreadAccess == false;
                foreach (NotifyCollectionChangedEventHandler handler in invocationList)
                {
                    //if (isOnDifferentThread)
                    //{
                    //    var handler1 = handler;
                    //    await dispatcherObject.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => handler1(this, e));
                    //}
                    //else
                    {
                        handler(this, e);
                    }
                }
            }
        }

        //private bool m_HasMoreItems = false;
        //public bool HasMoreItems
        //{
        //    get { FetchHasMoreItems();  return m_HasMoreItems; }
        //    set { m_HasMoreItems = value; OnPropertyChanged(new PropertyChangedEventArgs("HasMoreItems")); }
        //}

        //private async void FetchHasMoreItems()
        //{
        //    HasMoreItems = m_Loader == null ? false : this.Count < await m_Loader.FetchCount();
        //}

        //public global::Windows.Foundation.IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        //{
        //    return LoadMoreItems((int)count).AsAsyncOperation<LoadMoreItemsResult>();
        //}

        //private async Task<LoadMoreItemsResult> LoadMoreItems(int count)
        //{
        //    if(m_Loader == null)
        //        return new LoadMoreItemsResult();
            
        //    int newCount = await m_Loader.FetchCount();
        //    int delta = Math.Min(0, newCount - this.Count); 
        //    var ret = new LoadMoreItemsResult() { Count = (uint)delta};

        //    for (int i = 0; i < delta; i++)
        //        m_Loader.FetchItem(Count + i);

        //    return ret;
        //}
    }

}
