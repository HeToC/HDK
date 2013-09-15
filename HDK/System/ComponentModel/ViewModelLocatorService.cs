using System;
using System.Reflection;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Services;

namespace System.ComponentModel
{
    public interface IMVVMLocatorService : IService
    {
    }

    /// <summary>
    /// This class holds ViewModels that are registered with the ExportViewModelAttribute.
    /// </summary>

    [Shared]
    [ExportService("Default mvvm locator Service", "description", typeof(IMVVMLocatorService))]
    public sealed class MEFMVVMLocatorService : IMVVMLocatorService
    {
        /// <summary>
        /// Located view models
        /// </summary>
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
        /// Operator to retrieve view models.
        /// </summary>
        /// <returns>Read-only version of view model collection</returns>
        public object this[string CorrelationToken]
        {
            get
            {
                return LocateViewModel(CorrelationToken);
            }
        }

        /// <summary>
        /// Finds the VM based on the CorrelationToken.
        /// </summary>
        /// <param name="key">Correlation Token to search for</param>
        /// <returns>Located view model or null</returns>
        public object LocateViewModel(string CorrelationToken)
        {
            var view = m_ViewModels.FirstOrDefault(vm => vm.Metadata.CorrelationToken == CorrelationToken);
            
            if(view == null)
                throw new Exception("Could not locate view model: " + CorrelationToken);

            return view.Value;
        }

        public void Dispose()
        {
            if (m_Views != null) m_Views.ForEach<object>(o => { if (o is IDisposable) (o as IDisposable).Dispose(); });
            if (m_ViewModels != null) m_ViewModels.ForEach<object>(o => { if (o is IDisposable) (o as IDisposable).Dispose(); });
        }
    }

}
