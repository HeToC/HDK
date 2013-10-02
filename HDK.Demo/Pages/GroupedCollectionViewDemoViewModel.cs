using CollectionView;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HDK.Demo.Pages
{
    [ExportViewModel("#Demo #GroupedCollectionView"), Shared]
    public class GroupedCollectionViewDemoViewModel : ListCollectionViewDemoViewModel
    {
        public GroupedCollectionViewDemoViewModel() : base()
        {
        }
    }
}
