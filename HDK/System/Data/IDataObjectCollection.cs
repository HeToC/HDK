using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace System.Data
{
    public interface IDataObjectCollection : IList
    {
        DataObjectSet Context { get; }
        event NotifyCollectionChangedEventHandler CollectionChanged;
        event PropertyChangedEventHandler EntityPropertyChanged;
        object FindByPrimaryKeyBase(long id);
    }

}
