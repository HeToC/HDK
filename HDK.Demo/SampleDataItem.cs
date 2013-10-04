using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDK.Demo
{
    public class SampleDataItem : System.ComponentModel.BindableBase
    {
        public string Group { get; set; }
        public Guid Value { get; set; }

        public SampleDataItem()
        {
        }
        public override bool Equals(object obj)
        {
            return object.Equals(obj, Value);
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
