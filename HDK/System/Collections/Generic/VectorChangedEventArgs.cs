using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation.Collections;

namespace System.Collections.Generic
{

    /// <summary>
    /// Class that implements IVectorChangedEventArgs so we can fire VectorChanged events.
    /// </summary>
    public class VectorChangedEventArgs : IVectorChangedEventArgs
    {
        private readonly CollectionChange m_CollectionChange;
        private readonly uint m_Index;

        public VectorChangedEventArgs(CollectionChange change, int index = -1, object item = null)
        {
            m_CollectionChange = change;
            m_Index = (uint)index;
        }

        public VectorChangedEventArgs(CollectionChange change, uint index, object item = null)
        {
            m_CollectionChange = change;
            m_Index = index;
        }

        public CollectionChange CollectionChange
        {
            get { return m_CollectionChange; }
        }

        public uint Index
        {
            get { return m_Index; }
        }
    }
}
