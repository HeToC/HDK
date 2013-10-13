using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.Deferred;

namespace HDK.Data.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public AsyncLazy<SampleDataObject> data;

        public MainWindow()
        {
            this.DataContext = data = new AsyncLazy<SampleDataObject>(async () =>
            {
                        await Task.Delay(1000);
                        return new SampleDataObject()
                        {
                            ID = 100,
                            Name = "Initialized"
                        };
             
            } , new SampleDataObject()
                    {
                        ID = -1,
                        Name = "dummy"
                    });

            InitializeComponent();
        }
    }
}
