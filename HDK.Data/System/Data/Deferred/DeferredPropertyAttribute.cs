using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Deferred
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DeferredPropertyBehaviorAttribute : Attribute
    {
        public bool IsMandatory { get; set; }
        public bool IsLazy { get; set; }
        public bool IsAsync { get; set; }
        public bool IsFastLoading { get; set; }
        public bool IsLarge { get; set; }

        public DeferredPropertyBehaviorAttribute(bool isMandatory, bool isLazy, bool isAsync, bool isFastLoading, bool isLarge)
        {
            IsMandatory = isMandatory;
            IsFastLoading = isFastLoading;
            IsLarge = isLarge;
            IsLazy = isLazy;
            IsAsync = isAsync;
        }
    }
}
