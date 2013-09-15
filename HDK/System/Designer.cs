using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class Designer
    {
        /// <summary>
        /// Returns true/false whether the code is currently being executed by a designer surface
        /// (Blend or Visual Studio).
        /// </summary>
        public static bool InDesignMode
        {
            get
            {
                return global::Windows.ApplicationModel.DesignMode.DesignModeEnabled;
            }
        }
    }

}
