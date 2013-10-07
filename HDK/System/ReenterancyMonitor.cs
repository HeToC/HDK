using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{

    // this class helps prevent reentrant calls 
    public class ReenterancyMonitor<T> : IDisposable
    {
        private T m_parent;

        public ReenterancyMonitor(T parent)
        {
            m_parent = parent;
        }

        public void Enter()
        {
            ++_busyCount;
        }

        public void Dispose()
        {
            --_busyCount;
        }

        public bool Busy { get { return _busyCount > 0; } }
        int _busyCount;
    }
}
