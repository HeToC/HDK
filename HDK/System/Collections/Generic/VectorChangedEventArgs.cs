using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace System.Collections.Generic
{

    /// <summary>
    /// Class that implements IVectorChangedEventArgs so we can fire VectorChanged events.
    /// </summary>
    public sealed class VectorChangedEventArgs : IVectorChangedEventArgs
    {
        public VectorChangedEventArgs(CollectionChange change, int index = -1, object item = null)
        {
            CollectionChange = change;
            Index = (uint)index;
            Item = item;
        }

        public VectorChangedEventArgs(CollectionChange change, uint index, object item = null)
        {
            CollectionChange = change;
            Index = index;
            Item = item;
        }

        public VectorChangedEventArgs(NotifyCollectionChangedAction action, int index, object item = null)
        {
            switch (action)
            {
                case NotifyCollectionChangedAction.Add:
                    CollectionChange = CollectionChange.ItemInserted;
                    break;
                case NotifyCollectionChangedAction.Remove:
                    CollectionChange = CollectionChange.ItemRemoved;
                    break;
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Replace:
                    CollectionChange = CollectionChange.ItemChanged;
                    break;
                case NotifyCollectionChangedAction.Reset:
                    CollectionChange = CollectionChange.Reset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("action");
            }
            Index = (uint)index;
            Item = item;
        }

        /// <summary>
        /// Gets the affected item.
        /// </summary>
        public object Item { get; private set; }

        /// <summary>
        /// Gets the type of change that occurred in the vector.
        /// </summary>
        public CollectionChange CollectionChange { get; private set; }

        /// <summary>
        /// Gets the position where the change occurred in the vector.
        /// </summary>
        public uint Index { get; private set; }
    }
}
