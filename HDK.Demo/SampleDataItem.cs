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
        public string Value { get; set; }

        public SampleDataItem()
        {
        }
    }
}
