using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Services
{
    /// <summary>
    ///   A factory that creates new instances of the specified type.
    /// </summary>
    /// <typeparam name="T"> Type of instances to be created. </typeparam>
    public interface ICompositionFactory<T>
        where T : class
    {
        /// <summary>
        /// Creates and returns a new instance of T.
        /// </summary>
        T NewInstance();
    }

}
