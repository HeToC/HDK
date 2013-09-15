using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.ComponentModel
{
    public interface IView
    {
        /// <summary>
        /// Gets or sets the data context of the view.
        /// </summary>
        object DataContext { get; set; }
    }

    public interface IViewMetadata
    {
        string Url { get; set; }
        string CorrelationToken { get; set; }
    }

    /// <summary>
    /// Class used to populate metadata used to identify view models
    /// </summary>
    public sealed class ViewMetadata : IViewMetadata
    {
        /// <summary>
        /// Key used to export the ViewModel.  We only allow one export for VMs.
        /// </summary>
        public string CorrelationToken { get; set; }
        public string Url { get; set; }
    }

    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExportViewAttribute : ExportAttribute, IViewMetadata
    {
        public ExportViewAttribute() : base(typeof(IView))
        {
        }

        public ExportViewAttribute(string Url, string Token = null)
            : base(typeof(IView))
        {
            CorrelationToken = Token;
            this.Url = Url;
        }

        public string Url { get; set; }
        public string CorrelationToken { get; set; }
    }
}
