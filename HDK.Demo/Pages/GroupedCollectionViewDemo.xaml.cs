using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace HDK.Demo.Views.Pages
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    [ExportView(typeof(GroupedCollectionViewDemo), "#Demo #GroupedCollectionView")]
    public sealed partial class GroupedCollectionViewDemo : Page, IView
    {
        public GroupedCollectionViewDemo()
        {
            this.InitializeComponent();
        }

        private void OnSemanticZoomViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e)
        {
            // only interested in zoomed out->zoomed in transitions
            if (e.IsSourceZoomedInView)
            {
                return;
            }

        //    // get the selected group
        //    IGroupInfo<object, object> selectedGroup = e.SourceItem.Item as IGroupInfo<object, object>;

        //    // identify the selected group in the zoomed in data source (here I do it by its name, YMMV)
        //    IEnumerable<IGroupInfo<object, object>> myItemGroups = zoomedInGridView.ItemsSource as IEnumerable<IGroupInfo<object, object>>;
        //    IGroupInfo<object, object> myGroup = myItemGroups.First((g) => { return g.Key == selectedGroup.Key; });

        //    // workaround: need to reset the scroll position first, otherwise ScrollIntoView won't work
        //    SemanticZoomLocation zoomloc = new SemanticZoomLocation();
        //    zoomloc.Bounds = new Windows.Foundation.Rect(0, 0, 1, 1);
        //    zoomloc.Item = myItemGroups.First();//[0];
        //    zoomedInGridView.MakeVisible(zoomloc);

        //    // now we can scroll to the selected group in the zoomed in view
        //    zoomedInGridView.ScrollIntoView(myGroup, ScrollIntoViewAlignment.Leading);

        //}

    }
}
