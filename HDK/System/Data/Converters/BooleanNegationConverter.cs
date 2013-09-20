using System;
using System.Composition;
using Windows.UI.Xaml.Data;

namespace System.Data.Converters
{
    /// <summary>
    /// Value converter that translates true to false and vice versa.
    /// </summary>
    [ExportDataConverter("BooleanNegationConverter", typeof(bool)), Shared]
    public class BooleanNegationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool && (bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return !(value is bool && (bool)value);
        }
    }
}
