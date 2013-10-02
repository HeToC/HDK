using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;

namespace System.Collections.Generic
{
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
            m_OriginalSource = source;
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

            OnCollectionGroupChanged();
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
        }

        protected virtual void OnSourceVectorChanged(IObservableVector<TElement> sender, IVectorChangedEventArgs @e)
        {
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

        private ObservableVector<object> m_CollectionGroups = new ObservableVector<object>();
        public IObservableVector<object> CollectionGroups
        {
            get
            {
                return m_CollectionGroups;
            }
            set
            {
                m_CollectionGroups = new ObservableVector<object>(
                    from t in m_sourceCollectionEnumerable
                    group t by t.GetType().FullName into g
                    select g);
                OnCollectionGroupChanged();
                RaisePropertyChanged();
            }
        }

        private void OnCollectionGroupChanged()
        {
            
        }
    }
}
