using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HDK.Demo.Pages
{
    [ExportViewModel("#Demo #ObservableVector")]
    public class ObservableVectorDemoViewModel : ViewModelBase
    {
        private ObservableVector<object> m_Vector;
        public ObservableVector<object> Vector { get { return m_Vector; } set { m_Vector = value; RaisePropertyChanged(); } }

        public ICommand AddNewItemsCommand { get; set; }

        private Random rnd = new Random();
        public ObservableVectorDemoViewModel()
        {
            Vector = new ObservableVector<object>();
            for (int i = 0; i < 10; i++)
                Vector.Add(string.Format("Base Item: {0}", i));

            AddNewItemsCommand = new DelegateCommand(() =>
                {
                    int count = rnd.Next(5, 20);
                    for (int i = 0; i < count; i++)
                    {
                        Vector.Add(string.Format("New Item: {0}", i));
                    }
                });
        }
    }
}
