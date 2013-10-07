using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HDK.Demo.Pages
{
    [ExportViewModel("#Demo #PagedList")]
    public class PagedListDemoViewModel : ViewModelBase
    {
        private PagedList<object> m_Vector = new PagedList<object>();
        public PagedList<object> Vector { get { return m_Vector; } set { m_Vector = value; RaisePropertyChanged(); } }

        private object m_SelectedItem;
        public object SelectedItem
        {
            get { return m_SelectedItem; }
            set { m_SelectedItem = value; RaisePropertyChanged(); }
        }

        public ICommand AddNewItemsCommand { get; set; }
        public ICommand RemoveSelectedCommand { get; set; }

        private Random rnd = new Random();
        public PagedListDemoViewModel()
        {
            for (int i = 0; i < 10; i++)
                Vector.Add(string.Format("Base Item: {0}", i));

            AddNewItemsCommand = new DelegateCommand(() =>
                {
                    int count = rnd.Next(5, 20);
                    for (int i = 0; i < count; i++)
                    {
                        Vector.Add(string.Format("{1} {0}", i, DateTime.Now.TimeOfDay));
                    }
                });

            RemoveSelectedCommand = new DelegateCommand(() =>
                {
                    Vector.Remove(SelectedItem);
                });
        }
    }
}
