using System;
using System.Reflection;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Services;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Globalization;

namespace System.ComponentModel
{
    public interface IMVVMLocatorService : IService
    {
        object LocateViewModelForToken(string CorrelationToken);
        object LocateViewModelForView(object view);
        void AttachViewModel(object viewObject, object parameter);
        object CreateView(string CorrelationToken, object parameter);
    }

    [Shared]
    [ExportService("Default mvvm locator Service", "description", typeof(IMVVMLocatorService))]
    public sealed class MEFMVVMLocatorService : IMVVMLocatorService
    {
        private IList<Lazy<IViewModel, ExportViewModelAttribute>> m_ViewModels { get; set; }
        private IList<Lazy<IView, ExportViewAttribute>> m_Views { get; set; }

        IDataConverterService m_dataConverterService;
        ILoggerService m_Logger;

        [ImportingConstructor]
        public MEFMVVMLocatorService(
            [Import]IServiceLocator svcLocator,
            [ImportMany]IList<Lazy<IView, ExportViewAttribute>> views,
            [ImportMany]IList<Lazy<IViewModel, ExportViewModelAttribute>> viewModels)
        {
            m_dataConverterService = svcLocator.Resolve<IDataConverterService>();// dataConverterService;
            m_Logger = svcLocator.Resolve<ILoggerService>();

            m_ViewModels = viewModels;
            m_Views = views;
        }

        /// <summary>
        /// Finds the VM based on the CorrelationToken.
        /// </summary>
        /// <param name="key">Correlation Token to search for</param>
        /// <returns>Located view model or null</returns>
        public object LocateViewModelForToken(string CorrelationToken)
        {
            var view = m_ViewModels.FirstOrDefault(vm => vm.Metadata.CorrelationToken == CorrelationToken);
            
            if(view == null)
                throw new Exception("Could not locate view model: " + CorrelationToken);

            return view.Value;
        }

        public object LocateViewModelForView(object view)
        {
            if (view == null)
                return null;

            var frameworkElement = view as FrameworkElement;
            if (frameworkElement != null && frameworkElement.DataContext != null)
                return frameworkElement.DataContext;

            var iView = view as IView;
            if (iView != null)
            {
                var viewTypeInfo = iView.GetType().GetTypeInfo();
                var exportViewAttribute = viewTypeInfo.GetCustomAttribute<ExportViewAttribute>();
                if (exportViewAttribute != null && !string.IsNullOrEmpty(exportViewAttribute.CorrelationToken))
                {
                    var lazyVM = m_ViewModels.FirstOrDefault(vm => vm.Metadata.CorrelationToken == exportViewAttribute.CorrelationToken);
                    if (lazyVM != null)
                        return lazyVM.Value;
                }
            }

            return null;
        }

        public object LocateViewForToken(string CorrelationToken)
        {
            var view = m_Views.FirstOrDefault(vm => vm.Metadata.CorrelationToken == CorrelationToken);

            if (view == null)
                throw new Exception("Could not locate view model: " + CorrelationToken);

            return view.Value;
        }

        public Type LocateViewTypeForToken(string CorrelationToken)
        {
            var view = m_Views.FirstOrDefault(vm => vm.Metadata.CorrelationToken == CorrelationToken);

            if (view == null)
                throw new Exception("Could not locate view model: " + CorrelationToken);

            return view.Metadata.ViewType;
        }

        public object CreateView(string CorrelationToken, object parameter)
        {
            var view = LocateViewForToken(CorrelationToken);

            AttachViewModel(view, parameter);
            return view;
        }

        public void AttachViewModel(object viewObject, object parameter)
        {
            var viewModel = LocateViewModelForView(viewObject) as IViewModel;
            if (viewModel == null)
                return;

            var viewPage = viewObject as Page;
            if (viewPage == null)
            {
                throw new ArgumentException("View '" + viewObject.GetType().FullName + "' should inherit from Page or one of its descendents.");
            }

            UpdateViewModelProperties(viewModel, parameter);

            //TODO: create advanced binding system
            viewPage.DataContext = viewModel;

            //TODO: Call viewmodel method if it has to be notified about view readiness
        }

        private void UpdateViewModelProperties(IViewModel viewModel, object parameter)
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

        public void Dispose()
        {
            if (m_Views != null) m_Views.ForEach<object>(o => { if (o is IDisposable) (o as IDisposable).Dispose(); });
            if (m_ViewModels != null) m_ViewModels.ForEach<object>(o => { if (o is IDisposable) (o as IDisposable).Dispose(); });
        }
    }

}
