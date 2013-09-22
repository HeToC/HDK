using Windows.UI.Xaml.Data;

namespace System.Collections.Generic
{
    public class ObservableVectorView<TElement> : ObservableVectorView<TElement, List<TElement>>
    {
    }

    public class ObservableVectorView<TElement, TInner> : ObservableVector<TElement, TInner> 
        where TInner : IList, new()
    {
        int m_CursorPosition;
        TElement m_CurrentItem;

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

    }
}
