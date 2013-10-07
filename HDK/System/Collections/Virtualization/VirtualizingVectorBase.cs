using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Data;

namespace System.Collections.Virtualization
{
    public abstract class VirtualizingVectorBase<T> : 
        IList, IList<T>, 
        INotifyPropertyChanged, INotifyCollectionChanged
       // , ISupportIncrementalLoading
    {
        // *** Constants ***

        private const string PropertyNameCount = "Count";
        private const string PropertyNameIndexer = "Item[]";

        // *** Fields ***

        private Task<int> getCountTask;
        private int count;
        private bool isLoading;

        private HashSet<int> currentFetchingIndicies = new HashSet<int>();

        private int? currentFetchedIndex;
        private T currentFetchedItem;

        // *** Events ***

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        // *** IList<T> Properties ***

        public T this[int index]
        {
            get
            {
                return GetItem(index);
            }
            set
            {
                throw new InvalidOperationException();//ResourceHelper.GetErrorResource("Exception_InvalidOperation_CannotModifyReadOnlyCollection"));
            }
        }

        object IList.this[int index]
        {
            get
            {
                return GetItem(index);
            }
            set
            {
                throw new InvalidOperationException();//ResourceHelper.GetErrorResource("Exception_InvalidOperation_CannotModifyReadOnlyCollection"));
            }
        }

        public int Count
        {
            get
            {
                return GetCount();
            }
        }

        public bool IsFixedSize
        {
            get
            {
                return false;
            }
        }

        public bool IsLoading
        {
            get
            {
                return isLoading;
            }
            protected set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    OnPropertyChanged("IsLoading");
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public object SyncRoot
        {
            get
            {
                return this;
            }
        }

        // *** IList<T> Methods ***

        public void Add(T item)
        {
            throw new InvalidOperationException();//ResourceHelper.GetErrorResource("Exception_InvalidOperation_CannotModifyReadOnlyCollection"));
        }

        int IList.Add(object value)
        {
            throw new InvalidOperationException();//ResourceHelper.GetErrorResource("Exception_InvalidOperation_CannotModifyReadOnlyCollection"));
        }

        public void Clear()
        {
            throw new InvalidOperationException();//ResourceHelper.GetErrorResource("Exception_InvalidOperation_CannotModifyReadOnlyCollection"));
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        bool IList.Contains(object value)
        {
            if (value is T)
                return Contains((T)value);
            else
                return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            CopyToInternal(array, arrayIndex).Wait();
        }

        void ICollection.CopyTo(Array array, int index)
        {
            CopyToInternal(array, index).Wait();
        }

        public int IndexOf(T item)
        {
            return GetIndexOf(item);
        }

        int IList.IndexOf(object value)
        {
            if (value is T)
                return GetIndexOf((T)value);
            else
                return -1;
        }

        public void Insert(int index, T item)
        {
            throw new InvalidOperationException();//ResourceHelper.GetErrorResource("Exception_InvalidOperation_CannotModifyReadOnlyCollection"));
        }

        void IList.Insert(int index, object value)
        {
            throw new InvalidOperationException();//ResourceHelper.GetErrorResource("Exception_InvalidOperation_CannotModifyReadOnlyCollection"));
        }

        public bool Remove(T item)
        {
            throw new InvalidOperationException();//ResourceHelper.GetErrorResource("Exception_InvalidOperation_CannotModifyReadOnlyCollection"));
        }

        void IList.Remove(object value)
        {
            throw new InvalidOperationException();//ResourceHelper.GetErrorResource("Exception_InvalidOperation_CannotModifyReadOnlyCollection"));
        }

        public void RemoveAt(int index)
        {
            throw new InvalidOperationException();//ResourceHelper.GetErrorResource("Exception_InvalidOperation_CannotModifyReadOnlyCollection"));
        }

        // *** IEnumerable<T> Methods ***

        public IEnumerator<T> GetEnumerator()
        {
            int count = GetCountAsync().Result;

            for (int i = 0; i < Count; i++)
                yield return GetItemAsync(i).Result;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

        // *** Protected Methods ***

        protected void OnItemsAdded(int index, int count)
        {
            // If we are still awaiting the count for the collection then we can just ignore this

            if (IsLoading)
                return;

            // Get a list of all the items being fetched prior to the update

            int[] fetchingIndicies = currentFetchingIndicies.Where(i => i >= index).ToArray();

            // Increment the cached count for this collection

            this.count += count;

            // Raise property changed events

            OnPropertyChanged(PropertyNameCount);
            OnPropertyChanged(PropertyNameIndexer);

            // Raise collection changed events for each new item (in ascending order)

            for (int i = index; i < index + count; i++)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new object[] { null }, i));

            // Raise collection changed events for each previously fetching item
            // For example: The bound list may have requested item 5 and recieved a placeholder - if two items are added we need to fetch item 7
            //              NB - It is assumed that when item 5 is returned it is the "new" item 5

            foreach (int i in fetchingIndicies)
            {
                int newIndex = i + count;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new object[] { null }, new object[] { null }, newIndex));
            }
        }

        protected void OnItemsRemoved(int index, int count)
        {
            // If we are still awaiting the count for the collection then we can just ignore this

            if (IsLoading)
                return;

            // Get a list of all the items being fetched prior to the update

            int[] fetchingIndicies = currentFetchingIndicies.Where(i => i >= index + count).ToArray();

            // Decrement the cached count for this collection

            this.count -= count;

            // Raise property changed events

            OnPropertyChanged(PropertyNameCount);
            OnPropertyChanged(PropertyNameIndexer);

            // Raise collection changed events for each removed item (in decending order)

            for (int i = index + count - 1; i >= index; i--)
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new object[] { null }, i));

            // Raise collection changed events for each previously fetching item
            // For example: The bound list may have requested item 5 and recieved a placeholder - if two items are removed before this then we need to fetch item 3
            //              NB - It is assumed that when item 5 is returned it is the "new" item 5

            foreach (int i in fetchingIndicies)
            {
                int newIndex = i - count;
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new object[] { null }, new object[] { null }, newIndex));
            }
        }

        protected void Reset()
        {
            // If we are still awaiting the count for the collection then we can just ignore this

            if (IsLoading)
                return;

            // Reset the cached count and task count

            count = 0;
            getCountTask = null;

            // Raise property and collection changed events

            OnPropertyChanged(PropertyNameCount);
            OnPropertyChanged(PropertyNameIndexer);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        // *** Protected Abstract Methods ***

        abstract protected Task<int> GetCountAsync();

        abstract protected Task<T> GetItemAsync(int index);

        abstract protected int GetIndexOf(T item);

        // *** Event Handlers ***

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler eventHandler = PropertyChanged;

            if (eventHandler != null)
                eventHandler(this, e);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler eventHandler = CollectionChanged;

            if (eventHandler != null)
                eventHandler(this, e);
        }

        // *** Private Methods ***

        private int GetCount()
        {
            if (getCountTask == null)
            {
                getCountTask = GetCountAsync();

                // If the GetCountAsync call completed syncronously then return the count

                if (getCountTask.IsCompleted)
                    count = getCountTask.Result;

                // Otherwise fetch the count in the background (raising changed events)

                else
                    GetCountBackground(getCountTask);
            }

            // Return the cached value of count (or zero if this is still being fetched in the background)

            return count;
        }

        private async void GetCountBackground(Task<int> getCountTask)
        {
            // Await for the count to be returned

            IsLoading = true;
            int newCount = await getCountTask;
            IsLoading = false;

            if (count != newCount)
            {
                count = newCount;

                // Raise property and collection changed events

                OnPropertyChanged(PropertyNameCount);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        private T GetItem(int index)
        {
            // If we are within a property or collection changed event for this index then return the cached values

            if (currentFetchedIndex != null)
                return currentFetchedItem;

            // Otherwise retrieve the requested item

            Task<T> getItemTask = GetItemAsync(index);

            // If the GetItemAsync call completed syncronously then return the item directly

            if (getItemTask.IsCompleted)
                return getItemTask.Result;

            // Otherwise fetch the count in the background and return a placeholder (raising changed events)

            GetItemBackground(index, getItemTask);
            return default(T);
        }

        private async void GetItemBackground(int index, Task<T> getItemTask)
        {
            // If we are currently fetching this item then we can ignore this request

            if (currentFetchingIndicies.Contains(index))
                return;

            // Get the item (making a note of the index as it is being retrieved)

            currentFetchingIndicies.Add(index);
            T item = await getItemTask;
            currentFetchingIndicies.Remove(index);

            // Store the current index and value
            // NB: The data bound item will be retrieved during the property and collection changes, so we can use these cached values

            currentFetchedIndex = index;
            currentFetchedItem = item;

            // Raise property and collection changed events

            OnPropertyChanged(PropertyNameIndexer);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, default(T), index));

            // Reset the currently fetched state

            currentFetchedIndex = null;
            currentFetchedItem = default(T);
        }

        private async Task CopyToInternal(Array array, int index)
        {
            int count = await GetCountAsync();

            for (int i = 0; i < count; i++)
                array.SetValue(await GetItemAsync(i), new int[] { i + index });
        }
    }


    public class IncrementalLoadingDataList<T> : VirtualizingVectorBase<T>, ISupportIncrementalLoading, IUpdatableCollection
    {
        // *** Fields ***

        private readonly IDataListSource<T> dataListSource;

        private int minimumPagingSize;
        private int currentCount;
        private int? sourceCount;

        // *** Constructors ***

        public IncrementalLoadingDataList(IDataListSource<T> dataListSource)
        {
            // Validate the parameters

            if (dataListSource == null)
                throw new ArgumentNullException("dataListSource");

            // Set the fields

            this.dataListSource = dataListSource;
        }

        // *** Properties ***

        public int MinimumPagingSize
        {
            get
            {
                return minimumPagingSize;
            }
            set
            {
                if (minimumPagingSize != value)
                {
                    minimumPagingSize = value;
                    OnPropertyChanged("MinimumPagingSize");
                }
            }
        }

        // *** IUpdatableCollection Members ***

        void IUpdatableCollection.Update(DataListUpdate update)
        {
            switch (update.Action)
            {
                case DataListUpdateAction.Add:
                    Update_Add(update);
                    break;
                case DataListUpdateAction.Remove:
                    Update_Remove(update);
                    break;
                case DataListUpdateAction.Reset:
                    Update_Reset();
                    break;
            }
        }

        // *** ISupportIncrementalLoading Members ***

        public bool HasMoreItems
        {
            get
            {
                // If we have not yet retrived the number of items from the data list source then return true

                if (sourceCount == null)
                    return true;

                // Otherwise return true only if there are items yet to be retrived

                else
                    return currentCount < sourceCount;
            }
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return LoadMoreItemsAsyncInternal((int)count).AsAsyncOperation();
        }

        // *** Overridden Base Methods ***

        protected override Task<int> GetCountAsync()
        {
            return Task.FromResult<int>(currentCount);
        }

        protected override Task<T> GetItemAsync(int index)
        {
            // Validate arguments

            if (index < 0 || index > currentCount)
                throw new ArgumentOutOfRangeException("index");//, ResourceHelper.GetErrorResource("Exception_ArgumentOutOfRange_ArrayIndexOutOfRange"));

            // Return the value from the source

            return dataListSource.GetItemAsync(index);
        }

        protected override int GetIndexOf(T item)
        {
            // Get the index from the source

            int index = dataListSource.IndexOf(item);

            // If the index is in the currently visible region then return the index, otherwise return -1

            return index < currentCount ? index : -1;
        }

        // *** Private Methods ***

        private async Task<LoadMoreItemsResult> LoadMoreItemsAsyncInternal(int count)
        {
            IsLoading = true;

            // If we currently do not know the number of items then fetch this

            if (sourceCount == null)
                sourceCount = await dataListSource.GetCountAsync();

            // Set a minimum paging size if requested

            count = Math.Max(count, MinimumPagingSize);

            // Limit the number of items to fetch to the number of remaining items

            count = Math.Min(count, sourceCount.Value - currentCount);

            // Get all the items and wait until they are all fetched

            Task<T>[] fetchItemTasks = new Task<T>[count];
            int startIndex = currentCount;

            for (int i = 0; i < count; i++)
                fetchItemTasks[i] = dataListSource.GetItemAsync(startIndex + i);

            await Task.WhenAll(fetchItemTasks);

            // Increment the current count

            currentCount += count;

            // Set properties and raise property changed for HasMoreItems if this is not false

            IsLoading = false;

            if (currentCount == sourceCount)
                OnPropertyChanged("HasMoreItems");

            // Raise collection changed events

            OnItemsAdded(startIndex, count);

            // Return the number of items added

            return new LoadMoreItemsResult() { Count = (uint)count };
        }

        private void Update_Add(DataListUpdate update)
        {
            // If the entire update is outside of the visible collection then ignore it

            if (update.Index > currentCount)
                return;

            currentCount += update.Count;
            base.OnItemsAdded(update.Index, update.Count);
        }

        private void Update_Remove(DataListUpdate update)
        {
            // If the entire update is outside of the visible collection then ignore it

            if (update.Index >= currentCount)
                return;

            // If the update overlaps the boundary of the visible collection then only remove visible items

            int removedItemCount = Math.Min(update.Count, currentCount - update.Index);

            currentCount -= removedItemCount;
            base.OnItemsRemoved(update.Index, removedItemCount);
        }

        private void Update_Reset()
        {
            // Set the count to zero

            currentCount = 0;

            // Raise the Reset events

            base.Reset();

            // Raise a 'HasMoreItems' property changed event

            OnPropertyChanged("HasMoreItems");
        }
    }

}
