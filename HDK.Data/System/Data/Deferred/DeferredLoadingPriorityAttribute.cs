using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Deferred
{
    /// <summary>
    /// TODO: Change Priority from int to custom Enum
    /// </summary>
    public sealed class DeferredLoadingPriorityAttribute : Attribute
    {
        public int Priority { get; set; }

        public DeferredLoadingPriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }
}
