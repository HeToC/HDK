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

namespace System.Services
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


            var viewModel = m_MVVMLocatorService.LocateViewModelForView(viewObject) as IViewModel;
            if (viewModel == null)
                return;

            var viewPage = viewObject as Page;
            if (viewPage == null)
            {
                throw new ArgumentException("View '" + e.Content.GetType().FullName + "' should inherit from Page or one of its descendents.");
            }

            UpdateViewModelProperties(viewModel, e.Parameter);

            //TODO: create advanced binding system
            viewPage.DataContext = viewModel;

            //TODO: Call viewmodel method if it has to be notified about view readiness

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


        protected virtual void UpdateViewModelProperties(IViewModel viewModel, object parameter)
        {
            m_Logger.Log(LogSeverity.Information, this, "NavigationService.UpdateViewModelProperties({0}, '{1}')", viewModel, parameter);

            Uri uri = null;
            if (!Uri.TryCreate(Convert.ToString(parameter), UriKind.RelativeOrAbsolute, out uri))
                return;

            IEnumerable<KeyValuePair<string, string>> parsedQuery = uri.ParseQueryString();
            if (!parsedQuery.Any())
                return;

            IEnumerable<KeyValuePair<string, PropertyInfo>> boundReadyProperties = viewModel.GetNavigationBoundProperties();
            if (!boundReadyProperties.Any())
                return;

            var joined = parsedQuery.Join(boundReadyProperties, pq => pq.Key, brp => brp.Key, (qp, bp) =>
                new
                {
                    QueryPropertyName = qp.Key,
                    QueryPropertyValue = qp.Value,
                    BoundProperty = bp.Value
                });

            joined.ForEach(pii =>
            {
                m_Logger.Log(LogSeverity.Information, this, "Match Found: QueryProperty: '{0}', BoundProperty: '{1}' Value: {2}",
                    pii.QueryPropertyName, pii.BoundProperty.Name, pii.QueryPropertyValue);

                Type targetType = pii.BoundProperty.PropertyType;
                var dataConverter = m_dataConverterService[targetType];

                m_Logger.Log(LogSeverity.Information, this, "Trying to find DataConverter to '{0}'", targetType);

                if (dataConverter == null)
                {
                    //TODO: try smth else
                }
                try
                {

                    pii.BoundProperty.SetValue(viewModel, dataConverter.Convert(pii.QueryPropertyValue, targetType, null, CultureInfo.InvariantCulture.Name));
                }
                catch (Exception exc)
                {
                    exc.Data.Add("BoundProperty", pii.BoundProperty.Name);
                    exc.Data.Add("TargetType", targetType);
                    exc.Data.Add("QueryProperty", pii.QueryPropertyName);
                    exc.Data.Add("PropertyValue", pii.QueryPropertyValue);
                    exc.Data.Add("DataConverter", dataConverter == null ? null : dataConverter.GetType());

                    m_Logger.Log(LogSeverity.Error, this, "Unable to set bound property. {0}", "aga", exc);
                }
            });
        }
    }
}
