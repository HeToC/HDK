using CollectionView;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HDK.Demo.Pages
{
    [ExportViewModel("#Demo #ListCollectionView"), Shared]
    public class ListCollectionViewDemoViewModel : ViewModelBase
    {
        private ObservableVectorView<object> m_LCV;
        public ObservableVectorView<object> LCV { get { return m_LCV; } set { m_LCV = value; RaisePropertyChanged(); } }
        
        private object m_SelectedItem;
        public object SelectedItem {
            get
            { 
                return m_SelectedItem;
            }
            set 
            {
                m_SelectedItem = value; RaisePropertyChanged(); 
            } 
        }

        public ICommand AddNewItemsCommand { get; set; }
        public ICommand RemoveItemsCommand { get; set; }
        public ICommand RemoveAllItemsCommand { get; set; }

        private Random rnd = new Random();
        public ListCollectionViewDemoViewModel()
        {
            var src = new ObservableCollection<SampleDataItem>();

            for (int i = 0; i < 100; i++)
                src.Add(new SampleDataItem()
                {
                    Group = string.Format("Grp {0}", rnd.Next(0, 20)),
                    Value = Guid.NewGuid()
                });

            LCV = new ObservableVectorView(src);
            LCV.GroupDescriptors.Add((o) => (o as SampleDataItem).Group);

            AddNewItemsCommand = new DelegateCommand(() =>
                {
                    int count = rnd.Next(1, 50);
                    for (int i = 0; i < count; i++)
                    {
                        src.Add(new SampleDataItem() {
                            Group = string.Format("Grp {0}", rnd.Next(0,20)),
                            Value = Guid.NewGuid()
                        });
                    }
                });

            RemoveItemsCommand = new DelegateCommand((s) =>
            {
                src.Remove((SampleDataItem)s);
            });

            RemoveAllItemsCommand = new DelegateCommand((s) =>
            {
                src.Clear();
            });
        }
    }
}
