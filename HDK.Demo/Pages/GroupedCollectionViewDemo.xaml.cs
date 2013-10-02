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

    }
}
