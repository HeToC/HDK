using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data
{
    public interface IDataObjectFilter
    {
        bool RequiresEvaluation(string propertyName);
        bool Evaluate(DataObject entity);
    }
}
