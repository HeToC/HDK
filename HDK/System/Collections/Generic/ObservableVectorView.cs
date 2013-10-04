using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;

namespace System.Collections.Generic
{
    public class ObservableVectorView : ObservableVectorView<object>, ICollectionView
    {
        public ObservableVectorView(IEnumerable<object> source)
            : base(source)
        {
        }


        public bool HasMoreItems
        {
            get { return false; }
        }

        public global::Windows.Foundation.IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            //throw new NotImplementedException();
            return null;
        }
    }

    public class ObservableVectorView<TElement> : ObservableVectorView<TElement, List<TElement>>
        where TElement : class, new()
    {
        public ObservableVectorView(IEnumerable<TElement> source)
            : base(source)
        {
        }
    }

    public class ObservableVectorView<TElement, TInner> : ObservableVector<TElement, TInner>
        where TInner : IList, new()
        where TElement : class, new()
    {
        int m_CursorPosition;
        TElement m_CurrentItem;

        public ObservableVectorView(IEnumerable<TElement> source)
            : base(source)
        {

            m_GroupDescriptors = new ObservableCollection<Func<TElement, object>>();
            Source = m_OriginalSource = source;
        }

        private object m_OriginalSource;
        private object m_Source;
        public object Source
        {
            get
            {
                return m_Source;
            }
            set
            {
                if (m_Source != value)
                {
                    OnSourceChanging();
                    m_Source = value;
                    RaisePropertyChanged();
                    OnSourceChanged();
                }
            }
        }

        IEnumerable<TElement> m_sourceCollectionEnumerable;
        IQueryable<TElement> m_sourceCollectionQueryable;
        INotifyCollectionChanged m_sourceCollectionNotifyable;
        IObservableVector<TElement> m_sourceVerctorNotifyable;
        ISupportIncrementalLoading m_sourceIncrementalLoading;

        //TODO Is it possible to use WeakDelegate ?
        /// <summary>
        /// 
        /// 
        /// Usage:
        ///  protected override void OnSourceChanged()
        ///  {
        ///     base.OnSourceChanged();
        ///     ........
        ///     ........
        ///     ........
        ///  }
        /// 
        /// </summary>
        protected virtual void OnSourceChanged()
        {
            m_sourceCollectionEnumerable = Source as IEnumerable<TElement>;
            m_sourceCollectionQueryable = Source as IQueryable<TElement>;
            m_sourceCollectionNotifyable = Source as INotifyCollectionChanged;
            m_sourceVerctorNotifyable = Source as IObservableVector<TElement>;
            m_sourceIncrementalLoading = Source as ISupportIncrementalLoading;

            if (m_sourceCollectionNotifyable != null)
                m_sourceCollectionNotifyable.CollectionChanged += OnSourceCollectionChanged;

            if(m_sourceVerctorNotifyable!=null)
                m_sourceVerctorNotifyable.VectorChanged += OnSourceVectorChanged;

            base.InitializeInnerContainer(m_sourceCollectionEnumerable);

            RebuildGroups();
        }

        /// <summary>
        /// 
        /// 
        /// 
        /// Usage:
        ///  protected override void OnSourceChanging()
        ///  {
        ///     base.OnSourceChanging();
        ///  }
        /// </summary>
        protected virtual void OnSourceChanging()
        {
            if (m_sourceCollectionNotifyable != null)
                m_sourceCollectionNotifyable.CollectionChanged -= OnSourceCollectionChanged;

            if (m_sourceVerctorNotifyable != null)
                m_sourceVerctorNotifyable.VectorChanged -= OnSourceVectorChanged;
        }

        protected virtual void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
             switch (e.Action)
             {
                 case NotifyCollectionChangedAction.Add:
                     if (e.NewItems.Count == 1)
                     {
                         HandleItemAdded(e.NewStartingIndex, e.NewItems[0]);
                     }
                     else
                     {
                         HandleSourceChanged();
                     }
                     break;
                 case NotifyCollectionChangedAction.Remove:
                     if (e.OldItems.Count == 1)
                     {
                         HandleItemRemoved(e.OldStartingIndex, e.OldItems[0]);
                     }
                     else
                     {
                         HandleSourceChanged();
                     }
                     break;
                 case NotifyCollectionChangedAction.Move:
                 case NotifyCollectionChangedAction.Replace:
                 case NotifyCollectionChangedAction.Reset:
                     HandleSourceChanged();
                     break;
                 default:
                     throw new Exception("Unrecognized collection change notification: " + e.Action.ToString());
             }
            RebuildGroups();
        }

        private void HandleItemRemoved(int p1, object p2)
        {
            RebuildGroups();

            RaiseVectorChanged(CollectionChange.Reset);
        }

        private void HandleSourceChanged()
        {
            RebuildGroups();
            RaiseVectorChanged(CollectionChange.Reset);
        }

        private void HandleItemAdded(int p1, object p2)
        {
            RebuildGroups();

            RaiseVectorChanged(CollectionChange.Reset);
        }

        protected virtual void OnSourceVectorChanged(IObservableVector<TElement> sender, IVectorChangedEventArgs @e)
        {
            RebuildGroups();

            RaiseVectorChanged(CollectionChange.Reset);
        }

        /// <summary>
        /// Occurs after the current item has changed.
        /// </summary>
        public event EventHandler<TElement> CurrentChanged;
        /// <summary>
        /// Occurs before the current item changes.
        /// </summary>
        public event CurrentChangingEventHandler CurrentChanging;

        /// <summary>
        /// Gets the current item in the view.
        /// </summary>
        public TElement CurrentItem
        {
            get
            {
                return m_CursorPosition > -1 && m_CursorPosition < this.Count ? this[m_CursorPosition] : default(TElement);
            }
            set
            {
                MoveCurrentTo(value);
                m_CurrentItem = value;
            }
        }
        /// <summary>
        /// Gets the ordinal position of the current item in the view.
        /// </summary>
        public int CurrentPosition
        {
            get
            {
                return m_CursorPosition;
            }
            set
            {
                MoveCurrentToIndex(value);
            }
        }

        public bool IsCurrentAfterLast
        {
            get
            {
                return m_CursorPosition >= base.Count; 
            } 
        }

        public bool IsCurrentBeforeFirst
        {
            get
            {
                return m_CursorPosition < 0; 
            } 
        }

        public bool MoveCurrentToFirst()
        { 
            return MoveCurrentToIndex(0);
        }

        public bool MoveCurrentToLast() 
        { 
            return MoveCurrentToIndex(base.Count - 1);
        }

        public bool MoveCurrentToNext() 
        {
            return MoveCurrentToIndex(m_CursorPosition + 1); 
        }

        public bool MoveCurrentToPosition(int index) 
        { 
            return MoveCurrentToIndex(index); 
        }

        public bool MoveCurrentToPrevious() 
        {
            return MoveCurrentToIndex(m_CursorPosition - 1); 
        }

        public bool MoveCurrentTo(TElement item) 
        {
            return !object.ReferenceEquals(m_CurrentItem,CurrentItem) ? MoveCurrentToIndex(IndexOf(item)) : true;
        }

        /// <summary>
        /// Raises the <see cref="CurrentChanging"/> event.
        /// </summary>
        protected virtual void OnCurrentChanging(CurrentChangingEventArgs e)
        {
            if (CurrentChanging != null)
                CurrentChanging(this, e);
        }

        /// <summary>
        /// Raises the <see cref="CurrentChanged"/> event.
        /// </summary>
        protected virtual void OnCurrentChanged(TElement e)
        {
            if (CurrentChanged != null)
            {
                CurrentChanged(this, e);
                RaisePropertyChanged("CurrentItem");
            }
        }

        // move the cursor to a new position
        bool MoveCurrentToIndex(int index)
        {
            // invalid?
            if (index < -1 || index >= base.Count)
                return false;

            // no change?
            if (index == m_CursorPosition)
                return false;

            // fire changing
            var e = new CurrentChangingEventArgs();
            OnCurrentChanging(e);
            if (e.Cancel)
            {
                return false;
            }

            // change and fire changed
            m_CursorPosition = index;
            OnCurrentChanged(CurrentItem);
            return true;
        }

        private IObservableVector<object> m_CollectionGroups = null;
        public IObservableVector<object> CollectionGroups
        {
            get
            {
                if (m_CollectionGroups == null)
                    RebuildGroups();
                return m_CollectionGroups;
            }
            set
            {
                m_CollectionGroups = value;
                OnCollectionGroupChanged();
                RaisePropertyChanged();
            }
        }

        private void OnCollectionGroupChanged()
        {
        }

        private ObservableCollection<Func<TElement, object>> m_GroupDescriptors;
        public ObservableCollection<Func<TElement, object>> GroupDescriptors { get { return m_GroupDescriptors; } set { m_GroupDescriptors = value; RaisePropertyChanged(); OnCollectionGroupChanged(); } }
        private void RebuildGroups()
        {
            var first = GroupDescriptors.FirstOrDefault();
            if (first == null)
                return;

            var groups = from t in m_sourceCollectionEnumerable
                         group t by first(t) into g
                         select new GroupInfo<object, TElement>(g);

            var tmp = groups.ToList();
            CollectionGroups = new ObservableVector<object>(tmp);

        }
    }
}
