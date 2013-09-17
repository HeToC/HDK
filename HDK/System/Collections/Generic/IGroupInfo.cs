using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;

namespace System.Collections.Generic
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TElement"></typeparam>
    public interface IGroupInfo<TKey, TElement> : IGrouping<TKey, TElement>, ICollectionViewGroup
    {
    }

}
