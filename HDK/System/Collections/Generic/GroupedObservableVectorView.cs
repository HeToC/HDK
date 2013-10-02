using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace System.Collections.Generic
{
    public class GroupedObservableVectorView : GroupedObservableVectorView<string>
    {
    }

    public class GroupedObservableVectorView<TGroup> : ObservableVector<object>, IGroupInfo<TGroup, object>
    {
        public TGroup Key { get; set; }

        public object Group
        {
            get { return Key; }
        }

        public IObservableVector<object> GroupItems
        {
            get { return this; }
        }
    }
}
