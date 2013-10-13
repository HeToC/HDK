using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Deferred
{
    public interface IDeferredDataItem// : IDataWrapper<TElement>
    {
        /// <summary>
        /// Returns the current state of the item
        /// </summary>
        DeferredDataState CurrentState { get; }

        /// <summary>
        /// Returns whether or not the item is currently paused
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// Returns whether or not the item is currently loading
        /// </summary>
        bool IsLoading { get; }

        /// <summary>
        /// Returns whether or not the item is currently using
        /// </summary>
        bool IsInUse { get; }
    }
}
