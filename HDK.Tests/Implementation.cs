using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public class Implementation
    {
        public Guid ID { get; set; }
        public Implementation() : this(Guid.NewGuid())
        {
        }

        public Implementation(Guid uid)
        {
            ID = uid;
        }
    }
}
