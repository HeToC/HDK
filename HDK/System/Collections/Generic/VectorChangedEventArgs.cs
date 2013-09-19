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
        static VectorChangedEventArgs _reset = new VectorChangedEventArgs(CollectionChange.Reset);

        CollectionChange _cc = CollectionChange.Reset;
        uint _index = (uint)0xffff;

        public static VectorChangedEventArgs Reset
        {
            get { return _reset; }
        }

        public VectorChangedEventArgs(CollectionChange cc, int index = -1, object item = null)
        {
            _cc = cc;
            _index = (uint)index;
        }

        public CollectionChange CollectionChange
        {
            get { return _cc; }
        }

        public uint Index
        {
            get { return _index; }
        }
    }
}
