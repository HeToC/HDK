using System;
using System.Collections.Generic;
using System.Composition;
using System.Data.Converters;
using System.Linq;
using System.Services;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace System.Services
{
    public interface IDataConverterService : IService
    {
        IValueConverter this[string name] { get; }
        IValueConverter this[Type toType] { get; }
    }
    [Shared]
    [ExportService("DataConverterService","", typeof(IDataConverterService))]
    public class DataConverterService : IDataConverterService
    {
        IEnumerable<Lazy<IValueConverter, ExportDataConverterAttribute>> m_ValueConverters;

        [ImportingConstructor]
        public DataConverterService([ImportMany]IEnumerable<Lazy<IValueConverter, ExportDataConverterAttribute>> discoveredConverters)
        {
            m_ValueConverters = discoveredConverters;
        }

        public IValueConverter this[string name]
        {
            get
            {
                var lazyConverter = m_ValueConverters.FirstOrDefault(lo => string.Equals(lo.Metadata.Name, name));
                if (lazyConverter != null)
                    return lazyConverter.Value;
                return null;
            }
        }

        public IValueConverter this[Type toType]
        {
            get
            {
                var lazyConverter = m_ValueConverters.FirstOrDefault(lo => string.Equals(lo.Metadata.TargetType, toType));
                if (lazyConverter != null)
                    return lazyConverter.Value;
                return null;
            }
        }

        public void Dispose()
        {
        }
    }
}
