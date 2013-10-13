using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Data.Deferred
{
    [Flags]
    public enum DeferredDataState
    {
        /// <summary>
        /// No additional data is loaded
        /// </summary>
        Unloaded = 1,

        /// <summary>
        /// Data that is quick to load and doesn't use much memory is loaded
        /// </summary>
        /// <remarks>
        /// The item is in the virtual list but is not (yet) visible
        /// </remarks>
        Minimum = 2,

        /// <summary>
        /// All fast data has been loaded, and slow items are loading
        /// </summary>
        /// <remarks>
        /// The item is in the list and visible, and all content is loading
        /// </remarks>
        Loading = 4,

        /// <summary>
        /// All data has been loaded.
        /// </summary>
        /// <remarks>
        /// The item is in the list and visible, and all content has loaded
        /// </remarks>
        Loaded = 8,

        /// <summary>
        /// Large data items have been released, only small ones (regardless of speed) are loaded
        /// </summary>
        /// <remarks>
        /// The item is in the virtual list but is no longer visible
        /// </remarks>
        Cached = 16,

        /// <summary>
        /// All items except the large, slow ones are loaded (fast ones are loaded; small ones were cached)
        /// </summary>
        /// <remarks>
        /// The item is in the list and has become visible again after being invisible
        /// </remarks>
        Reloading = 32,

        All = Unloaded | Minimum | Loading | Loaded | Cached | Reloading
    }
}
