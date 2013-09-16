using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace System.Data.Converters
{
    [MetadataAttribute]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ExportDataConverterAttribute : ExportAttribute
    {
        public string Name { get; set; }
        public Type TargetType { get; set; }

        public ExportDataConverterAttribute()
            : base(typeof(IValueConverter))
        {
        }

        public ExportDataConverterAttribute(string name, Type targetType)
            : base(typeof(IValueConverter))
        {
            Name = name;
            TargetType = targetType;
        }
    }
}
