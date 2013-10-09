using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace System.Collections.ObjectModel
{
    
    public class PagedList<T> : BindableBase,
        IList<T>, IList, INotifyCollectionChanged//, ICollection<T>, IReadOnlyList<T>, IReadOnlyCollection<T>, IEnumerable<T>,  ICollection, IEnumerable,
        //INotifyPropertyChanged, INotifyCollectionChanged , IObservableVector<T>
    {
        private T[][] internalPages = new T[0][];
        private int m_PageCacheSize = int.MaxValue;
        private List<int> recentlyAccessedPageList = new List<int>();

        private ReenterancyMonitor<PagedList<T>> m_monitor;

        public PagedList(int pageSize = 10)
        {
            this.PageSize = pageSize;
            m_monitor = new ReenterancyMonitor<PagedList<T>>(this);
        }
        public T this[int index]
        {
            get
            {
                return GetItem(index);
            }
            set
            {
                SetItem(index, value);
            }
        }

        private T GetItem(int index)
        {
            // Validate that the index is within range
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException();

            // If the page does not exist then return a placeholder
            int pageIndex = index / PageSize;
            T[] page = internalPages[pageIndex];

            if (page == null)
                return default(T);

            // Add the page to the recently accessed page list
            AddRecentlyUsedPage(pageIndex);

            // Return the element
            int elementIndex = index % PageSize;
            return page[elementIndex];
        }

        private void SetItem(int index, T value)
        {
            // Validate that the index is within range
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException();

            // If the page does not exist then create it
            int pageIndex = index / PageSize;
            T[] page = internalPages[pageIndex];

            if (page == null)
            {
                page = new T[PageSize];
                internalPages[pageIndex] = page;
            }

            // Add the page to the recently accessed page list

            AddRecentlyUsedPage(pageIndex);

            // Set the element

            int elementIndex = index % PageSize;

            T oldValue = this[index];
            page[elementIndex] = value;

            RaisePropertyChanged("Item[]");
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue, index));
        }

        private int m_Count;
        public int Count
        {
            get { return m_Count; }
            private set { m_Count = value; RaisePropertyChanged(); }
        }

        private int m_PageSize;
        public int PageSize
        {
            get { return m_PageSize; }
            private set { m_PageSize = value; RaisePropertyChanged(); }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public int PageCacheSize
        {
            get
            {
                return m_PageCacheSize;
            }
            set
            {
                if (value <= 0)
                    throw new ArgumentOutOfRangeException();

                m_PageCacheSize = value;

                RaisePropertyChanged();
            }
        }

        public void UpdateCount(int count, int pageSize)
        {
            // Validate that the arguments are within range

            if (count < 0)
                throw new ArgumentOutOfRangeException();

            if (pageSize < 1)
                throw new ArgumentOutOfRangeException();

            // If the page size has changed then clear the internal page list

            if (this.PageSize != pageSize)
                internalPages = new T[0][];

            // Update the properties

            this.Count = count;
            this.PageSize = pageSize;

            // If the number of pages has changed then update the internal page list

            int pageCount = (Count - 1) / PageSize + 1;

            if (pageCount > internalPages.Length)
            {
                T[][] newInternalPages = new T[pageCount][];
                internalPages.CopyTo(newInternalPages, 0);
                internalPages = newInternalPages;
            }

            RaisePropertyChanged("Count");
        }

        public virtual int Add(T item)
        {
//            CheckReentrancy();

            int newIndex = Count;
            UpdateCount(Count + 1, PageSize);

            // Raise collection changed events for each new item (in ascending order)
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new object[] { null }, newIndex));

            this[newIndex] = item;

            RaisePropertyChanged("Item[]");

            return newIndex;
        }

        void ICollection<T>.Add(T item)
        {
            Add(item);
        }

        #region IList


        public virtual void Clear()
        {
//            CheckReentrancy();
            Count = 0;
            internalPages = new T[0][];

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(T item)
        {
            return IndexOf(item) != -1;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            T[] blankPage = new T[PageSize];

            // For each page, copy to the destination in turn (or use the blank page to overwrite elsewhere)

            for (int pageIndex = 0; pageIndex < internalPages.Length - 1; pageIndex++)
            {
                T[] page = internalPages[pageIndex] ?? blankPage;
                page.CopyTo(array, arrayIndex + pageIndex * PageSize);
            }

            // For the last page only copy the items up to count

            T[] lastPage = internalPages[internalPages.Length - 1] ?? blankPage;
            int lastPageStartIndex = (internalPages.Length - 1) * PageSize;
            int lastPageLength = Count - lastPageStartIndex;

            Array.ConstrainedCopy(lastPage, 0, array, arrayIndex + lastPageStartIndex, lastPageLength);
        }

        public int IndexOf(T item)
        {
            // Enumerate all the pages

            for (int pageIndex = 0; pageIndex < internalPages.Length; pageIndex++)
            {
                T[] page = internalPages[pageIndex];

                if (page != null)
                {
                    // If the page exists then enumerate all the items within the page

                    for (int itemIndex = 0; itemIndex < PageSize; itemIndex++)
                    {
                        T internalItem = page[itemIndex];

                        if (internalItem != null && internalItem.Equals(item))
                            return pageIndex * PageSize + itemIndex;
                    }
                }
            }

            // Otherwise return -1

            return -1;
        }

        public virtual void Insert(int index, T item)
        {
 //           CheckReentrancy();

            // Validate that the index is within range
            if (index < 0 || index > Count)
                throw new ArgumentOutOfRangeException();

            // Ensure that there is enough room in the collection
            UpdateCount(Count + 1, PageSize);

            // If there are items after the inserted item them move them along
            Insert_MoveItems(index);

            // Insert the new item
            this[index] = item;
//            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));

        }

        public virtual bool Remove(T item)
        {
            int index = IndexOf(item);
            
            if (index == -1)
                return false;

            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
//            CheckReentrancy();

            // Validate that the index is within range
            if (index < 0 || index >= Count)
                throw new ArgumentOutOfRangeException();

            var item = this[index];
            // Reduce the count (NB: We don't worry about contracting the internal page list)
            Count--;

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, new object[] { null }, index));

            RaisePropertyChanged("Item[]");

            // Move all items after the removed item to the left
            Remove_MoveItems(index);
//            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        #endregion IList

        #region IEnumerable

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
        }

        #endregion

        // *** Private Methods ***

        private void AddRecentlyUsedPage(int pageIndex)
        {
            // If we are not limiting the number of pages to cache then skip this

            if (PageCacheSize == int.MaxValue)
                return;

            // Otherwise more the specified page index to the end of the list

            recentlyAccessedPageList.Remove(pageIndex);
            recentlyAccessedPageList.Add(pageIndex);

            // If there are more than the maxium pages then remove the last recently used

            if (recentlyAccessedPageList.Count > PageCacheSize)
            {
                internalPages[recentlyAccessedPageList[0]] = null;
                recentlyAccessedPageList.RemoveAt(0);
            }
        }

        private void Insert_MoveItems(int index)
        {
            int insertPage = index / PageSize;
            int insertIndex = index % PageSize;

            // Move full pages (starting at the end)

            for (int pageIndex = internalPages.Length - 1; pageIndex >= insertPage; pageIndex--)
            {
                T[] page = internalPages[pageIndex];

                if (page != null)
                {
                    int startIndex = pageIndex == insertPage ? insertIndex : 0;
                    int endIndex = pageIndex == internalPages.Length - 1 ? (Count - 1) % PageSize - 1 : PageSize - 1;

                    // If we need to copy the last element into the next page then do this

                    if (endIndex == PageSize - 1)
                    {
                        int lastItemIndex = Math.Min(this.Count-1, pageIndex * PageSize + endIndex);
                        this[lastItemIndex + 1] = this[lastItemIndex];
                    }

                    int length = endIndex - startIndex;
                    if (length != PageSize - 1 && pageIndex != insertPage)
                        length++;

                    // Move the rest of the items along
                    Array.ConstrainedCopy(page, startIndex, page, startIndex + 1, length);

                    //if (endIndex == PageSize-1)
                    //{
                    //    //move last element to next page
                    //    T element = page[endIndex];
                    //    T[] nextPage = internalPages[pageIndex + 1];
                    //    nextPage[0] = element;
                    //}



  //                  OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, page.ToList(), insertIndex, startIndex));
                }
            }
        }

        private void Remove_MoveItems(int index)
        {
            int removePage = index / PageSize;
            int removeIndex = index % PageSize;

            // Move full pages (starting at the start)

            for (int pageIndex = removePage; pageIndex < internalPages.Length; pageIndex++)
            {
                T[] page = internalPages[pageIndex];
                T[] nextPage = pageIndex == internalPages.Length - 1 ? null : internalPages[pageIndex + 1];

                if (page != null)
                {
                    int startIndex = pageIndex == removePage ? removeIndex + 1 : 1;

                    // Move all the items to the left by one

                    Array.ConstrainedCopy(page, startIndex, page, startIndex - 1, PageSize - startIndex);

                    // Set the last item of this page to the first item of the next page

                    if (nextPage != null)
                        page[PageSize - 1] = nextPage[0];
                    else
                        page[PageSize - 1] = default(T);
                }
            }
        }

        #region INotifyCollectionChanged

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        protected void OnCollectionChanged(NotifyCollectionChangedEventArgs ea)
        {

            var handler = CollectionChanged;
            if (handler != null)
                //using (BlockReentrancy())
                    handler(this, ea);
            //RaiseVectorChanged(new VectorChangedEventArgs(ea, ea.i
        }

        #endregion


        ////public event VectorChangedEventHandler<T> VectorChanged;
        ////protected void RaiseVectorChanged(VectorChangedEventArgs @ea)
        ////{
        ////    var handler = VectorChanged;
        ////    if (handler != null)
        ////        handler(this, ea);
        ////}

        int IList.Add(object value)
        {
            return this.Add((T)value);
        }

        bool IList.Contains(object value)
        {
            return this.Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            return this.IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            this.Insert(index, (T)value);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        void IList.Remove(object value)
        {
            this.Remove((T)value);
        }

        object IList.this[int index]
        {
            get
            {
                return GetItem(index);
            }
            set
            {
                SetItem(index, (T)value);
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            this.CopyTo(array.OfType<T>().ToArray(), index);
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return m_monitor; }
        }


        ///// <summary>

        ///// Disallow reentrant attempts to change this collection. E.g. a event handler 

        ///// of the CollectionChanged event is not allowed to make changes to this collection. 

        ///// </summary>

        ///// <remarks> 

        ///// typical usage is to wrap e.g. a OnCollectionChanged call with a using() scope:

        ///// <code>

        /////         using (BlockReentrancy())

        /////         { 

        /////             CollectionChanged(this, new NotifyCollectionChangedEventArgs(action, item, index));

        /////         } 

        ///// </code> 

        ///// </remarks>

        //protected IDisposable BlockReentrancy()
        //{

        //    m_monitor.Enter();

        //    return m_monitor;

        //}



        ///// <summary> Check and assert for reentrant attempts to change this collection. </summary> 

        ///// <exception cref="InvalidOperationException"> raised when changing the collection 

        ///// while another collection change is still being notified to other listeners </exception>

        //protected void CheckReentrancy()
        //{
        //    if (m_monitor.Busy)
        //    {

        //        // we can allow changes if there's only one listener - the problem 

        //        // only arises if reentrant changes make the original event args

        //        // invalid for later listeners.  This keeps existing code working 

        //        // (e.g. Selector.SelectedItems). 

        //        if ((CollectionChanged != null) && (CollectionChanged.GetInvocationList().Length > 1))
        //            throw new InvalidOperationException();
        //    }

        //}
    }

}
