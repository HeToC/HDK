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

namespace System.ComponentModel
{
    public interface IMVVMLocatorService : IService
    {
        object LocateViewModelForToken(string CorrelationToken);
        object LocateViewModelForView(object view);
    }

    [Shared]
    [ExportService("Default mvvm locator Service", "description", typeof(IMVVMLocatorService))]
    public sealed class MEFMVVMLocatorService : IMVVMLocatorService
    {
        private IList<Lazy<IViewModel, ViewModelMetadata>> m_ViewModels { get; set; }
        private IList<Lazy<IView, ViewMetadata>> m_Views { get; set; }

        [ImportingConstructor]
        public MEFMVVMLocatorService(
            [ImportMany]IList<Lazy<IView, ViewMetadata>> views,
            [ImportMany]IList<Lazy<IViewModel, ViewModelMetadata>> viewModels)
        {
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

        public void Dispose()
        {
            if (m_Views != null) m_Views.ForEach<object>(o => { if (o is IDisposable) (o as IDisposable).Dispose(); });
            if (m_ViewModels != null) m_ViewModels.ForEach<object>(o => { if (o is IDisposable) (o as IDisposable).Dispose(); });
        }
    }

}
