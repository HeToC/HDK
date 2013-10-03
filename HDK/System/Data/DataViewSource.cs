using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace System.Data
{
    public interface IDataViewSource
    {

    }
    public class DataViewSource : DependencyObject, IDataViewSource, ICollectionViewFactory
    {
        public DataViewSource()
        {
        }   

        public object Source
        {
            get { return (object)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Source.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(object), typeof(DataViewSource), new PropertyMetadata(null, new PropertyChangedCallback(OnSourceChanged)));

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public bool IsSourceGrouped
        {
            get { return (bool)GetValue(IsSourceGroupedProperty); }
            set { SetValue(IsSourceGroupedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSourceGrouped.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSourceGroupedProperty =
            DependencyProperty.Register("IsSourceGrouped", typeof(bool), typeof(DataViewSource), new PropertyMetadata(false, new PropertyChangedCallback(OnIsSourceGroupedChanged)));

        private static void OnIsSourceGroupedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public PropertyPath ItemsPath
        {
            get { return (PropertyPath)GetValue(ItemsPathProperty); }
            set { SetValue(ItemsPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsPathProperty =
            DependencyProperty.Register("ItemsPath", typeof(PropertyPath), typeof(DataViewSource), new PropertyMetadata(new PropertyPath(""), new PropertyChangedCallback(OnItemsPathChanged)));

        private static void OnItemsPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public ICollectionView View
        {
            get { return (ICollectionView)GetValue(ViewProperty); }
            set { SetValue(ViewProperty, value); }
        }

        // Using a DependencyProperty as the backing store for View.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewProperty =
            DependencyProperty.Register("View", typeof(ICollectionView), typeof(DataViewSource), new PropertyMetadata(null, new PropertyChangedCallback(OnViewChanged)));

        private static void OnViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public ICollectionView CreateView()
        {
            return new ObservableVectorView(Source as IEnumerable<object>);
        }
    }
}
