using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Virtualization;
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
        //private VirtualizingDataList<string> m_Vector;
        //public VirtualizingDataList<string> Vector { get { return m_Vector; } set { m_Vector = value; RaisePropertyChanged(); } }
        
        private PagedList<string> m_Vector;
        public PagedList<string> Vector { get { return m_Vector; } set { m_Vector = value; RaisePropertyChanged(); } }

        private object m_SelectedItem;
        public object SelectedItem
        {
            get { return m_SelectedItem; }
            set { m_SelectedItem = value; RaisePropertyChanged(); }
        }

        public ICommand UpdateItemCommand { get; set; }
        public ICommand AddNewItemsCommand { get; set; }
        public ICommand AddOneItemCommand { get; set; }

        public ICommand InsertBeforeCommand { get; set; }
        public ICommand InsertAfterCommand { get; set; }
        
        public ICommand RemoveSelectedCommand { get; set; }

        private Random rnd = new Random();
        public PagedListDemoViewModel()
        {
            //SampleDataSource dataSource = new SampleDataSource();
            //m_Vector = new VirtualizingDataList<string>(dataSource);

            Vector = new PagedList<string>();

            for (int i = 0; i < 206; i++)
                Vector.Add(string.Format("{0}", i));
            //dataSource.items.Add(string.Format("Base Item: {0}", i));

            AddNewItemsCommand = new DelegateCommand(() =>
                {
                    int count = 10;// rnd.Next(5, 20);
                    for (int i = 0; i < count; i++)
                    {
                        //dataSource.items.Add(string.Format("{1} {0}", i, DateTime.Now.TimeOfDay));
                        Vector.Add(string.Format("{1} {0}", i, DateTime.Now.TimeOfDay));
                    }
                });

            RemoveSelectedCommand = new DelegateCommand(() =>
                {
                    Vector.Remove((string)SelectedItem);
                });

            AddOneItemCommand = new DelegateCommandSync(() =>
            {
                //dataSource.items.Add("oneItem");
                Vector.Add("oneItem");
            });

            InsertBeforeCommand = new DelegateCommand((o) =>
                {
                    string so = (string)o;
                    int index = Vector.IndexOf(so);
                    Vector.Insert(index, "B-I");
                }, (o) =>
                {
                    return o !=null;
                });
            InsertAfterCommand = new DelegateCommand((o) =>
                {
                    string so = (string)o;
                    int index = Vector.IndexOf(so);
                    index++;
                    Vector.Insert(index, "A-I");
                }, (o) =>
                {
                    return o !=null;
                });

            UpdateItemCommand = new DelegateCommand((o)=>
                {
                    int oi = Vector.IndexOf((string)o);
                    Vector[oi] = Convert.ToString(DateTime.Now.Ticks);
                }, (o) => o != null);
        }

        //public class SampleDataSource : PagedDataListSource<string>
        //{
        //    public ObservableCollection<string> items { get; set; }
        //    public SampleDataSource()
        //    {
        //        items = new ObservableCollection<string>();
        //    }

        //    protected async override Task<DataListPageResult<string>> FetchCountAsync()
        //    {
        //        await Task.Yield();

        //        return new DataListPageResult<string>(items.Count, null, null, null);
        //    }

        //    protected async override Task<DataListPageResult<string>> FetchPageSizeAsync()
        //    {
        //        await Task.Yield();

        //        return new DataListPageResult<string>(null, 5, null, null);
        //    }

        //    static Random rnd = new Random();

        //    protected async override Task<DataListPageResult<string>> FetchPageAsync(int pageNumber)
        //    {
        //        await Task.Delay(1000 * rnd.Next(1,20));

        //        string[] pageItems = items.Skip((pageNumber - 1) * 5).Take(5).ToArray();

        //        return new DataListPageResult<string>(null, null, pageNumber, pageItems);
        //    }
        //}
    }
}
