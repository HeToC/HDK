using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDK.Demo
{
    [ExportViewModel("#Shell")]
    public class ShellViewModel : ViewModelBase
    {
        private string m_ButtonText;
        public string ButtonText { get { return m_ButtonText; } set { m_ButtonText = value; RaisePropertyChanged(); } }
        
        public ShellViewModel()
        {
            ButtonText = "dslkjfdslkjfdslkfsdljk";
        }

    }
}
