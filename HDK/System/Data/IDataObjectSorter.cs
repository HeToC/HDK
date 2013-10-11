using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data
{
    public interface IDataObjectSorter
    {
        bool RequiresEvaluation(string propertyName);
        Func<DataObject, object> SortFunction { get; }
    }
}
