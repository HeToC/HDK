using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace System.ComponentModel
{
    public static class ViewModelExtensions
    {
        public static IEnumerable<KeyValuePair<string, PropertyInfo>> GetNavigationBoundProperties(this IViewModel viewModel)
        {
            var typeInfo = viewModel.GetType().GetTypeInfo();
            return from propertyInfo in typeInfo.DeclaredProperties
                   where propertyInfo.CanWrite
                   let navBountAttr = propertyInfo.GetCustomAttribute<NavigationBoundPropertyAttribute>()
                   where navBountAttr != null
                   select new KeyValuePair<string, PropertyInfo>(navBountAttr.Name,propertyInfo);
        }
    }
}
