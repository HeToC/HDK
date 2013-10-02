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
    public interface IGroupInfo<out TKey, out TElement> : IGrouping<TKey, TElement>, ICollectionViewGroup
    {
    }

    //public class GroupInfo<TKey, TElement> : ObservableVector<TElement>, IGroupInfo<TKey, TElement>
    //    where TElement: Object
    //{
    //    public TKey Key { get; set; }

    //    public object Group
    //    {
    //        get { return Key; }
    //    }

    //    public IObservableVector<object> GroupItems
    //    {
    //        get { return this.Cast<object>() }
    //    }
    //}

}
