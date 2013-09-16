using HDK.Demo.Views.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Navigation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HDK.Demo
{
    [ExportViewModel("#Shell")]
    public class ShellViewModel : ViewModelBase
    {
        private string m_ButtonText;
        public string ButtonText { get { return m_ButtonText; } set { m_ButtonText = value; RaisePropertyChanged(); } }

        public ICommand ButtonClickCommand { get; set; }

        [NavigationBound("TestProperty")]
        public string NavigationBoundText { get; set; }

        [ImportingConstructor]
        public ShellViewModel(IServiceLocator svcLocator)
        {
            var svcNav = svcLocator.Resolve<INavigationService>();
            ButtonText = "Navigate To page 1";
            ButtonClickCommand = new DelegateCommand(() => svcNav.Navigate(typeof(Page1)));
        }

    }
}
