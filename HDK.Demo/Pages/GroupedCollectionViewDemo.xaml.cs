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

            // get the selected group
            IGroupInfo<object, object> selectedGroup = e.SourceItem.Item as IGroupInfo<object, object>;

            // identify the selected group in the zoomed in data source (here I do it by its name, YMMV)
            var vectorView = zoomedInGridView.ItemsSource as ObservableVectorView<object>;
            var myItemGroups = vectorView.CollectionGroups;
            IGroupInfo<object, object> myGroup = myItemGroups.First((g) =>
            {
                var gi = g as IGroupInfo<object, object>;
                if (gi == null)
                    return false;
                return gi.Key == selectedGroup.Key;
            }) as IGroupInfo<object, object>;

            if (myGroup != null)
            {
                object firstInGroup = myGroup.First();
                object vvi = vectorView[vectorView.IndexOf(firstInGroup)];

                bool eq = vvi == firstInGroup;

                // workaround: need to reset the scroll position first, otherwise ScrollIntoView won't work
                SemanticZoomLocation zoomloc = new SemanticZoomLocation();
                zoomloc.Bounds = new Windows.Foundation.Rect(0, 0, 1, 1);
                zoomloc.Item = firstInGroup;// myItemGroups.First();//[0];
                zoomedInGridView.MakeVisible(zoomloc);

                // now we can scroll to the selected group in the zoomed in view
                zoomedInGridView.ScrollIntoView(firstInGroup, ScrollIntoViewAlignment.Leading);
            }
        }
    }
}
