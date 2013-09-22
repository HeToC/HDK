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
    [ExportViewModel("#Demo #ListCollectionView"), Shared]
    public class ListCollectionViewDemoViewModel : ViewModelBase
    {
        private ListCollectionView m_LCV;
        public ListCollectionView LCV { get { return m_LCV; } set { m_LCV = value; RaisePropertyChanged(); } }
        
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

        private Random rnd = new Random();
        public ListCollectionViewDemoViewModel()
        {
            LCV = new ListCollectionView(new List<string>());
            for (int i = 0; i < 10; i++)
                LCV.Add(string.Format("Base Item: {0}", i));

            AddNewItemsCommand = new DelegateCommand(() =>
                {
                    int count = rnd.Next(5, 20);
                    for (int i = 0; i < count; i++)
                    {
                        LCV.Add(string.Format("{1} {0}", i, DateTime.Now.TimeOfDay));
                    }
                });

            RemoveItemsCommand = new DelegateCommand((s) =>
            {
                LCV.Remove(s);
            });
        }
    }
}
