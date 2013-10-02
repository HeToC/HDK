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
    public class DataViewSource : DependencyObject, IDataViewSource
    {
        public DataViewSource()
        {
        }

        // Summary:
        //     Gets or sets a value that indicates whether source data is grouped.
        //
        // Returns:
        //     True if data is grouped. False if data is not grouped.
        public bool IsSourceGrouped { get; set; }
        //
        // Summary:
        //     Identifies the IsSourceGrouped dependency property.
        //
        // Returns:
        //     The identifier for the IsSourceGrouped dependency property.
        public static DependencyProperty IsSourceGroupedProperty { get; }
        //
        // Summary:
        //     Gets or sets the property path to follow from the top level item to find
        //     groups within the CollectionViewSource.
        //
        // Returns:
        //     The property path to follow from the top level item to find groups. The default
        //     is a PropertyPath created from an empty string. This path implies that the
        //     object itself is the collection.
        public PropertyPath ItemsPath { get; set; }
        //
        // Summary:
        //     Identifies the ItemsPath dependency property.
        //
        // Returns:
        //     The identifier for the ItemsPath dependency property.
        public static DependencyProperty ItemsPathProperty { get; }
        //
        // Summary:
        //     Gets or sets the collection object from which to create this view.
        //
        // Returns:
        //     The collection to create the view from.
        public object Source { get; set; }
        //
        // Summary:
        //     Identifies the Source dependency property.
        //
        // Returns:
        //     The identifier for the Source dependency property.
        public static DependencyProperty SourceProperty { get; }
        //
        // Summary:
        //     Gets the view object that is currently associated with this instance of CollectionViewSource.
        //
        // Returns:
        //     The view object that is currently associated with this instance of CollectionViewSource.
        public ICollectionView View { get; }
        //
        // Summary:
        //     Identifies the View dependency property.
        //
        // Returns:
        //     The identifier for the View dependency property.
        public static DependencyProperty ViewProperty { get; }
    }
}
