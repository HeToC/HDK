using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using System.Globalization;
using System.Services;

namespace System.Navigation
{
    public interface INavigationService : IService
    {

        bool Navigate(Type type);
        bool Navigate(Type type, object parameter);
        bool Navigate(string type);
        bool Navigate(string type, object parameter);
        void GoBack();
        void GoForward();

        bool CanGoBack();
        bool CanGoForward();

        void AttachToFrame(Frame frame);
    }

    [Shared]
    [ExportService("Default Navigation Service", "description", typeof(INavigationService))]
    public class NavigationService : INavigationService
    {
        IMVVMLocatorService m_MVVMLocatorService;
        IDataConverterService m_dataConverterService;
        ILoggerService m_Logger;

        [ImportingConstructor]
        public NavigationService(IServiceLocator svcLocator)//IMVVMLocatorService mvvmLocatorService, IDataConverterService dataConverterService)
        {
            m_MVVMLocatorService = svcLocator.Resolve<IMVVMLocatorService>();// mvvmLocatorService;
            m_dataConverterService = svcLocator.Resolve<IDataConverterService>();// dataConverterService;
            m_Logger = svcLocator.Resolve<ILoggerService>();
        }

        private Frame m_mainFrame;


        public bool Navigate(Type type)
        {
            return m_mainFrame.Navigate(type);
        }

        public bool Navigate(Type type, object parameter)
        {
            return m_mainFrame.Navigate(type, parameter);
        }

        public bool Navigate(string type, object parameter)
        {
            return m_mainFrame.Navigate(Type.GetType(type), parameter);
        }

        public bool Navigate(string type)
        {
            return m_mainFrame.Navigate(Type.GetType(type));
        }

        public void GoBack()
        {
            if (CanGoBack())
            {
                m_mainFrame.GoBack();
            }
        }

        public void GoForward()
        {
            if (CanGoForward())
            {
                m_mainFrame.GoForward();
            }
        }

        public void StartService()
        {
        }

        public void StopService()
        {
        }

        public void Dispose()
        {
        }

        public void AttachToFrame(Frame frame)
        {
            m_mainFrame = frame;
            m_mainFrame.Navigating += OnNavigating;
            m_mainFrame.Navigated += OnNavigated;
        }

        protected virtual void OnNavigated(object sender, NavigationEventArgs e)
        {
            var viewObject = e.Content;
            if (viewObject == null)
                return;

            m_MVVMLocatorService.AttachViewModel(viewObject, e.Parameter);
        }

        protected virtual void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {

        }

        public bool CanGoBack()
        {
            return m_mainFrame.CanGoBack;
        }

        public bool CanGoForward()
        {
            return m_mainFrame.CanGoForward;
        }
    }
}
