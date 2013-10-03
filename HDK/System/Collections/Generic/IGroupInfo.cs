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
    public interface IGroupInfo<out TKey, TElement> : IGrouping<TKey, TElement>//, ICollectionViewGroup
        where TElement : class, new()
    {
    }

    public class GroupInfo<TKey, TElement> : ObservableVector<object>, IGroupInfo<TKey, object>
        where TElement : class, new()
    {
        private TKey m_Key;
        public TKey Key { get { return m_Key; } set { m_Key = value; RaisePropertyChanged(); RaisePropertyChanged("Group"); } }

        public object Group
        {
            get { return Key; }
        }

        public IObservableVector<object> GroupItems
        {
            get { return this; }
        }

        public GroupInfo(IGrouping<TKey, object> source)
            : base(source)
        {
            Key = source.Key;
        }
    }

}
