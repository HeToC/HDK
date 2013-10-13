using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data
{
    public interface IDataWrapper<T>
    {
        T Content { get; set; }
    }
}
