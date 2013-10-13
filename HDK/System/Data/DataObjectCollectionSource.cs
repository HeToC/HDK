using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace System.Data
{
    public class DataObjectCollectionSource : BindableBase// DependencyObject
    {
        IEnumerable m_ItemsSource;
        public IEnumerable ItemsSource
        {
            get { return m_ItemsSource; }
            set
            {
                m_ItemsSource = value;

                RaisePropertyChanged();

                AttachToEntitySource(value != null, (IDataObjectCollection)value);
            }
        }

        IDataObjectFilter m_Filter;
        public IDataObjectFilter Filter
        {
            get { return m_Filter; }
            set { m_Filter = value; RaisePropertyChanged(); }
        }

        IDataObjectSelector m_Selector;
        public IDataObjectSelector Selector
        {
            get { return m_Selector; }
            set { m_Selector = value; RaisePropertyChanged(); }
        }

        IDataObjectSorter m_Sorter;
        public IDataObjectSorter Sorter
        {
            get { return m_Sorter; }
            set { m_Sorter = value; RaisePropertyChanged(); }
        }

        /*
         *        public IEnumerable ItemsSource
        {
            get { return (IDataObjectCollection)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(DataObjectCollectionSource), new PropertyMetadata(null, OnPropertyChanged));

        public IDataObjectFilter Filter
        {
            get { return (IDataObjectFilter)GetValue(EntityFilterProperty); }
            set { SetValue(EntityFilterProperty, value); }
        }

        public static readonly DependencyProperty EntityFilterProperty = DependencyProperty.Register("Filter", typeof(IDataObjectFilter), typeof(DataObjectCollectionSource), new PropertyMetadata(null, OnPropertyChanged));

        public IDataObjectSelector Selector
        {
            get { return (IDataObjectSelector)GetValue(RowSelectorProperty); }
            set { SetValue(RowSelectorProperty, value); }
        }

        public static readonly DependencyProperty RowSelectorProperty = DependencyProperty.Register("Selector", typeof(IDataObjectSelector), typeof(DataObjectCollectionSource), new PropertyMetadata(null, OnPropertyChanged));

        public IDataObjectSorter Sorter
        {
            get { return (IDataObjectSorter)GetValue(SorterProperty); }
            set { SetValue(SorterProperty, value); }
        }

        public static readonly DependencyProperty SorterProperty = DependencyProperty.Register("Sorter", typeof(IDataObjectSorter), typeof(DataObjectCollectionSource), new PropertyMetadata(null, OnPropertyChanged));

         */
        public IEnumerable Source { get { return _viewSource; } }

        private readonly ObservableCollection<object> _viewSource = new ObservableCollection<object>();
        private readonly object _itemsSourceLock = new object();

        private WeakEventHandler<NotifyCollectionChangedEventArgs> _weakEventHandler;
        private WeakEventHandler<PropertyChangedEventArgs> _entityPropertyChangedHandler;

        private IDataObjectCollection _currentEntitySource;


        public DataObjectCollectionSource()
        {
        }

        //protected static void OnPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        //{
        //    var collectionSource = target as DataObjectCollectionSource;
        //    if (collectionSource == null) return;
        //    if (e.Property == ItemsSourceProperty)
        //    {
        //        bool hasNewValue = e.NewValue != null;
        //        collectionSource.AttachToEntitySource(hasNewValue, (IDataObjectCollection)e.NewValue);
        //    }
        //    else if (e.Property == RowSelectorProperty)
        //    {
        //        collectionSource._rowSelector = e.NewValue as IDataObjectSelector;
        //    }
        //    else if (e.Property == SorterProperty)
        //    {
        //        collectionSource._sorter = e.NewValue as IDataObjectSorter;
        //    }
        //    else if (e.Property == EntityFilterProperty)
        //    {
        //        collectionSource._dataObjectFilter = e.NewValue as IDataObjectFilter;
        //    }
        //}

        private void AttachToEntitySource(bool hasNewValue, IDataObjectCollection view)
        {
            if (_currentEntitySource != null)
            {
                _currentEntitySource.CollectionChanged -= _weakEventHandler.Invoke;
                _currentEntitySource.EntityPropertyChanged -= _entityPropertyChangedHandler.Invoke;
            }
            _weakEventHandler = null;
            _entityPropertyChangedHandler = null;
            if (hasNewValue)
            {
                _currentEntitySource = view;
                _weakEventHandler = new WeakEventHandler<NotifyCollectionChangedEventArgs>(ListChanged);
                _entityPropertyChangedHandler = new WeakEventHandler<PropertyChangedEventArgs>(EntityPropertyChanged);
                _currentEntitySource.CollectionChanged += _weakEventHandler.Invoke;
                _currentEntitySource.EntityPropertyChanged += _entityPropertyChangedHandler.Invoke;
                if (this.Selector != null)
                {
                    SelectRows();
                }
                else
                {
                    UpdateViewSource();
                }
            }
            else
            {
                _currentEntitySource = null;
                lock (_itemsSourceLock)
                {
                    _viewSource.Clear();
                }
            }
        }

        private void SelectRows()
        {
            lock (_itemsSourceLock)
            {
                Selector.Evaluate(_currentEntitySource, _viewSource);
            }
        }

        private void EntityPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args != null &&
                (Filter != null && Filter.RequiresEvaluation(args.PropertyName) ||
                Sorter != null && Sorter.RequiresEvaluation(args.PropertyName)))
            {
                UpdateViewSource();
            }
        }

        private void ListChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Reset)
            {
                lock (_itemsSourceLock)
                {
                    _viewSource.Clear();
                }
                return;
            }
            if (Selector == null)
            {
                UpdateViewSource();
            }
            else
            {
                SelectRows();
            }
        }

        private void UpdateViewSource()
        {
            lock (_itemsSourceLock)
            {
                var rowViews = (from DataObject x in _currentEntitySource
                                where Filter == null || Filter.Evaluate(x)
                                select x).ToList();
                if (Sorter != null)
                {
                    rowViews = rowViews.OrderBy(Sorter.SortFunction).ToList();
                }
                for (int i = _viewSource.Count - 1; i > -1; i--)
                {
                    if (rowViews.Contains(_viewSource[i])) continue;
                    _viewSource.RemoveAt(i);
                }
                int newIndex = 0;
                foreach (DataObject x in rowViews)
                {
                    if (x == null)
                        continue;
                    var index = _viewSource.IndexOf(x);
                    if (index > -1)
                    {
                        if (index != newIndex)
                        {
                            _viewSource.RemoveAt(index);
                            _viewSource.Insert(newIndex, x);
                        }
                    }
                    else
                    {
                        _viewSource.Insert(newIndex, x);
                    }
                    newIndex++;
                }
            }

        }
    }
}
